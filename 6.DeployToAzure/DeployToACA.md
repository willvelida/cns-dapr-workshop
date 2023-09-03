## Deploy to Azure Container Apps.

In this module, we'll deploy our Dapr applications to Azure Container Apps. Azure Container Apps provides integration with the Dapr framework, so you can deploy your Dapr applications to Azure Container Apps without having to do any heavy configuration to make it work. Take a look at the following diagram:

![Concepts related to Dapr in Azure Container Apps](https://learn.microsoft.com/en-us/azure/container-apps/dapr-overview?tabs=bicep1%2Cyaml)

Let's break this down:

1. Dapr is enabled at the container app level by configuring Dapr arguments. These values will apply to all revisions of a given container app if that container app is running in multiple revision mode.
2. Dapr APIs are exposed to your container app through a Dapr sidecar. Like normal Dapr applications, APIs can be invoked from your container app via HTTP or gRPC. The sidecar runs on HTTP port 3500 and gRPC port 50001.
3. Dapr components can be shared across multiple container apps. The Dapr app identifiers provided in the scope arrays determine which dapr-enabled container apps will load a given component at runtime.

To deploy our Dapr applications to Azure Container Apps, let's use the Azure CLI to create our resources. To make our lives a little easier, let's create some variables that we'll use for various resources. Open up a terminal and run the following Powershell commands:

```powershell
$ENVIRONMENT="tasks-tracker-containerapps-env"
$WORKSPACE_NAME="<replace this with your unique app log analytics workspace name>"
$APPINSIGHTS_NAME="<replace this with your unique app insights name>"
$BACKEND_API_NAME="tasksmanager-backend-api"
$ACR_NAME="<replace this with your unique acr name>"
```

We'll create a new Azure Container Registry to store images of all our applications. Run the following command:

```powershell
az acr create `
--resource-group $RG_NAME `
--name $ACR_NAME `
--sku Basic `
--admin-enabled true
```

We also need to create a Log Analytics workspace. This will store the system and application log data from all our Container Apps running in our Container App environment. We can create a Log Analytics workspace, and retrieve the workspace ID and secret used to authenticate to it by running the following commands:

```powershell
# create the log analytics workspace
az monitor log-analytics workspace create `
--resource-group $RG_NAME `
--workspace-name $WORKSPACE_NAME

# retrieve workspace ID
$WORKSPACE_ID=az monitor log-analytics workspace show --query customerId `
-g $RG_NAME `
-n $WORKSPACE_NAME -o tsv

# retrieve workspace secret
$WORKSPACE_SECRET=az monitor log-analytics workspace get-shared-keys --query primarySharedKey `
-g $RG_NAME `
-n $WORKSPACE_NAME -o tsv
```

We'll also use Application Insights to enable distributed tracing between different container apps within our Container App environment. To do this, we can create it using the following command:

```powershell
# Install the application-insights extension for the CLI
az extension add -n application-insights

# Create application-insights instance
az monitor app-insights component create `
-g $RG_NAME `
-l $LOCATION `
--app $APPINSIGHTS_NAME `
--workspace $WORKSPACE_NAME

# Get Application Insights Instrumentation Key
$APPINSIGHTS_INSTRUMENTATIONKEY=($(az monitor app-insights component show `
--app $APPINSIGHTS_NAME `
-g $RG_NAME)  | ConvertFrom-Json).instrumentationKey
```

We can now create all our Container App resources. Container Apps are provisioned to a Container App Environment. If you're coming from a Kubernetes background, you can think of a Container App Environment as a Kubernetes namespace. The Container Apps Environment acts as a secure boundary around a group of container apps.

To interact with Container App resources via the CLI, we'll need to install the Container Apps extension by running the following:

```powershell
# Install/Upgrade Azure Container Apps Extension
az extension add --name containerapp --upgrade
```

Create your environment by running the following:

```powershell
# Create the ACA environment
az containerapp env create `
--name $ENVIRONMENT `
--resource-group $RG_NAME `
--logs-workspace-id $WORKSPACE_ID `
--logs-workspace-key $WORKSPACE_SECRET `
--dapr-instrumentation-key $APPINSIGHTS_INSTRUMENTATIONKEY `
--location $LOCATION
```

Dapr Components are configured at the environment level, so we can go ahead and create the various Dapr components that we have used for our application. The schema for Dapr components in Azure Container Apps differ slightly from how we normally define them. The metadata for most Dapr components should be the same, so when you're defining components for your Container Apps environment, ensure that they have the right metadata values for it.

Let's start with our state management component by using the following yaml file:

```yaml
componentType: state.azure.cosmosdb
version: v1
metadata:
  - name: url
    value: <The URI value of your cosmos database account>
  - name: masterKey
    value: ""
  - name: database
    value: tasksmanagerdb
  - name: collection
    value: taskscollection
scopes:
  - tasksmanager-backend-api
```

We can then add the State Store component to our environment by running the following command:

```powershell
az containerapp env dapr-component set `
 --name $ENVIRONMENT --resource-group $RG_NAME `
 --dapr-component-name statestore `
 --yaml '<location-of-your-state-store-file>'
```

Add a component for your Azure Service Bus for Pub/Sub by using the following yaml specification:

```yaml
# pubsub.yaml for Azure Service Bus component
componentType: pubsub.azure.servicebus
version: v1
metadata:
  - name: connectionString
    value: "<connection string from step 1>"
  - name: consumerID
    value: "tasks-processor-subscription"
# Application scopes
scopes:
  - tasksmanager-backend-api
  - tasksmanager-backend-processor
```

With this defined, we can add it to our Container App environment:

```powershell
az containerapp env dapr-component set `
--name $ENVIRONMENT --resource-group $RESOURCE_GROUP `
--dapr-component-name dapr-pubsub-servicebus `
--yaml '<location-of-your-pubsub-file>'
```

For our Bindings, we can create the components for them by creating the following YAML files. We'll need two for our Storage Account (input and output) as well as one for SendGrid.

```yaml
componentType: bindings.azure.storagequeues
version: v1
metadata:
  - name: storageAccount
    value: "<Your Storage Account Name>"
  - name: storageAccessKey
    value: "***********"
  - name: queue
    value: "external-tasks-queue"
  - name: decodeBase64
    value: "true"
  - name: route
    value: /externaltasksprocessor/process
scopes:
  - tasksmanager-backend-processor
```

```yaml
componentType: bindings.azure.blobstorage
version: v1
metadata:
  - name: storageAccount
    value: "<Your Storage Account Name>"
  - name: storageAccessKey
    value: "***********"
  - name: container
    value: "externaltaskscontainer"
  - name: decodeBase64
    value: "false"
  - name: publicAccessLevel
    value: "none"
scopes:
  - tasksmanager-backend-processor
```

```yaml
componentType: bindings.twilio.sendgrid
version: v1
metadata:
  - name: emailFrom
    value: "mail@gmail.com"
  - name: emailFromName
    value: "Tasks Tracker Notification"
  - name: apiKey
    value: sendgrid-api-key
scopes:
  - tasksmanager-backend-processor
```

We can add them to our environment by running the following commands:

```powershell
##Input binding component for Azure Storage Queue
az containerapp env dapr-component set `
  --name $ENVIRONMENT --resource-group $RESOURCE_GROUP `
  --dapr-component-name externaltasksmanager `
  --yaml '.<containerapps-bindings-in-storagequeue>'

##Output binding component for Azure Blob Storage
az containerapp env dapr-component set `
 --name $ENVIRONMENT --resource-group $RESOURCE_GROUP `
  --dapr-component-name externaltasksblobstore `
 --yaml '<containerapps-bindings-out-blobstorage>'

##Output binding component for SendGrid
az containerapp env dapr-component set `
 --name $ENVIRONMENT --resource-group $RESOURCE_GROUP `
  --dapr-component-name sendgrid `
 --yaml '<sendgrid-component>'
```

Let's start by deloying our Backend API. First up, we'll need to build our docker image and push it up to Azure Container Registry. Navigate to the Backend API project directory and run the following:

```powershell
az acr build --registry $ACR_NAME --image "tasksmanager/$BACKEND_API_NAME" --file 'TasksTracker.TasksManager.Backend.Api/Dockerfile' .
```

Now, let's deploy this image to a Container App that we'll provision in our Container App Environment. We can do so by running the following:

```powershell
az containerapp create `
--name $BACKEND_API_NAME  `
--dapr-app-id $BACKEND_API_NAME `
--dapr-app-port <app-port-found-in-docker-file> `
--resource-group $RG_NAME `
--environment $ENVIRONMENT `
--image "$ACR_NAME.azurecr.io/tasksmanager/$BACKEND_API_NAME" `
--registry-server "$ACR_NAME.azurecr.io" `
--target-port [port number that was generated when you created your docker file in vs code] `
--ingress 'external' `
--enable-dapr true `
--min-replicas 1 `
--max-replicas 1 `
--cpu 0.25 --memory 0.5Gi `
--query configuration.ingress.fqdn
```

Once this is deployed, we can run similar commands for our UI application. Navigate to the project directory of your Web App and build the docker image for it by running the following:

```powershell
$FRONTEND_WEBAPP_NAME="tasksmanager-frontend-webapp"

az acr build --registry $ACR_NAME --image "tasksmanager/$FRONTEND_WEBAPP_NAME" --file 'TasksTracker.WebPortal.Frontend.Ui/Dockerfile' .
```

Verify that the docker image has been pushed to your Azure Container Registry. If it has, we can now run the following command to deploy it to our Container App.

```powershell
az containerapp create `
--name "$FRONTEND_WEBAPP_NAME"  `
--dapr-app-id $FRONTEND_WEBAPP_NAME `
--dapr-app-port <app-port-found-in-docker-file> `
--resource-group $RESOURCE_GROUP `
--environment $ENVIRONMENT `
--image "$ACR_NAME.azurecr.io/tasksmanager/$FRONTEND_WEBAPP_NAME" `
--registry-server "$ACR_NAME.azurecr.io" `
--env-vars "BackendApiConfig__BaseUrlExternalHttp=<url to your backend api goes here. You can find this on the azure portal overview tab. Look for the Application url property there.>/" `
--target-port <port number that was generated when you created your docker file in vs code for your frontend application> `
--ingress 'external' `
--enable-dapr true `
--min-replicas 1 `
--max-replicas 1 `
--cpu 0.25 --memory 0.5Gi `
--query configuration.ingress.fqdn
```

Finally, we can build and deploy our Processor application. Let's push our docker image to ACR by running the following command:

```powershell
az acr build --registry $ACR_NAME --image "tasksmanager/$BACKEND_SVC_NAME" --file 'TasksTracker.Processor.Backend.Svc/Dockerfile' .
```

Once that image has been pushed successfully to ACR, we can create our Container App for it:

```powershell
az containerapp create `
--name "$BACKEND_SVC_NAME"  `
--resource-group $RESOURCE_GROUP `
--environment $ENVIRONMENT `
--image "$ACR_NAME.azurecr.io/tasksmanager/$BACKEND_SVC_NAME" `
--registry-server "$ACR_NAME.azurecr.io" `
--min-replicas 1 `
--max-replicas 1 `
--cpu 0.25 --memory 0.5Gi `
--enable-dapr true `
--dapr-app-id  $BACKEND_SVC_NAME `
--dapr-app-port  <web api application port number found under Dockerfile for the web api project. e.g. 5071> `
--env-vars "SendGrid__IntegrationEnabled=true"
```

Once all your applications are running, we should see them in running in Azure Container Apps!.

To delete your resources, just delete the resource group by running the following:

```powershell
az group delete --name $RG_NAME
```