## (Option 2): Deploy to Azure Kubernetes Service.

In this module, we'll deploy our Dapr applications to Azure Kubernetes Service. Unlike Azure Container Apps, we'll need to create and configure the Dapr extension on our AKS cluster. Once this is setup, we can deploy our Dapr applications to Azure Kubernetes Service.

First, let's define some variables that we'll use throughout this module:

```powershell
$AKS_CLUSTER_NAME="your-cluster-name"
$ACR_NAME="youracrname
```

We'll need to create an Azure Container Registry to push our docker images to. We can do so by running the following:

```powershell
az acr create `
--resource-group $RG_NAME `
--name $ACR_NAME `
--sku Basic `
--admin-enabled true
```

To create our AKS cluster, run the following command:

```powershell
az aks create \
    --resource-group $RG_NAME \
    --name $AKS_CLUSTER_NAME \
    --node-count 2 \
    --generate-ssh-keys \
    --attach-acr $ACR_NAME
```

If you haven't got it installed, you'll need to install the Kubernetes CLI to connect to the cluster from your machine.

```powershell
az aks install-cli
```

We can then connect to the cluster by running the ```az aks get-credentials``` command:

```powershell
az aks get-credentials --resource-group $RG_NAME --name $AKS_CLUSTER_NAME
```

You can verify the connection by running ```kubectl get nodes``` to return a list of cluster nodes.

With our Kubernetes cluster setup, we'll need to set up the Azure CLI extension for running cluster extensions:

```powershell
az extension add -name K8s-extension
```

You may also need to register the service provider for cluster extensions for your Azure subscription. You can register the provider with the following:

```powershell
az provider register --namespace Microsoft.KubernetesConfiguration
```

Once everything has been successfully registered, we can create our Dapr extension, which will install Dapr on your AKS cluster.

```powershell
az k8s-extension create --cluster-type managedClusters \
--cluster-name $AKS_CLUSTER_NAME \
--resource-group $RG_NAME \
--name dapr \
--extension-type Microsoft.Dapr
```

We can create our Dapr components by applying the YAML files in our AKS cluster, for example:

```powershell
kubectl apply -f ./aks-infra/dapr-pubsub-svcbus.yaml
```

We can verify that our components have been successfully configured by using the following command:

```powershell
kubectl get components.dapr-pubsub-svcbus -o yaml
```

We can now build our container images and deploy them to our cluster. To build and push your images to Azure Container Registry, run the following commands:

```powershell
$BACKEND_API_NAME=""
$FRONTEND_WEBAPP_NAME=""
$BACKEND_SVC_NAME=""

az acr build --registry $ACR_NAME --image "tasksmanager/$BACKEND_API_NAME" --file 'TasksTracker.TasksManager.Backend.Api/Dockerfile' .
az acr build --registry $ACR_NAME --image "tasksmanager/$FRONTEND_WEBAPP_NAME" --file 'TasksTracker.WebPortal.Frontend.Ui/Dockerfile' .
az acr build --registry $ACR_NAME --image "tasksmanager/$BACKEND_SVC_NAME" --file 'TasksTracker.Processor.Backend.Svc/Dockerfile' .
```

To delete your resources, just delete the resource group by running the following:

```powershell
az group delete --name $RG_NAME
```