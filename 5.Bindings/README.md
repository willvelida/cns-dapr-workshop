## Working with Dapr Bindings

Serverless offerings such as Azure Functions and AWS Lambda have gained wided adoption across the distributed architecture space thanks to their ability to handle events from, or invoke events in, external systems. They abstract away the underlying complexity and plumbing concerns.

The Dapr bindings building block brings resources such as datastores, message systems etc. to your Dapr applications.

In this module, we're going to extend the backend background processor service to interface with an external system outside our Tasks Tracker application.

First, let's discuss how Bindings work in Dapr, and what problem it solves.

## How Bindings in Dapr work

Like other Dapr APIs, we use Bindings through component configuration files. Again, this is a YAML file that describes the resource to which we bind to in our application. Once this is configured, our service can receive events from the resource or trigger events to it.

There are two types of Bindings in Dapr, *Input* and *Output* bindings.

Input bindings trigger your code with incoming events from external resources. To receive events and data from those resources, you register a public endpoint from your service that becomes the event *handler*. Take a look at the following:

![Dapr input binding flow](https://learn.microsoft.com/en-us/dotnet/architecture/dapr-for-net-developers/media/bindings/input-binding-flow.png)

1. The Dapr sidecar reads the binding configuration file and subscribes to the event specified for the external resource. In this diagram, the event source is a Twitter account.
2. When a matching tweet is published on Twitter, the binding component is the Dapr sidecar picks it up and triggers an event.
3. The Dapr sidecar invokes the endpoint configured for the binding. In this diagram, the service listens for an HTTP POST on the ```/tweet``` endpoint on port 6000. Because it's a POST, the JSON payload for the event is passed in the request body.
4. After handling the event, the service returns ```200 OK```.

Output bindings enable your service to trigger an event that will invoke an external resource. Once you've configured the binding, you trigger an event that invokes the bindings API on the Dapr sidecar of your application. Let's take a look at the following example:

![Dapr output binding flow](https://learn.microsoft.com/en-us/dotnet/architecture/dapr-for-net-developers/media/bindings/output-binding-flow.png)

1. The Dapr sidecar reads the binding configuration file with the information on how to connect to the external resource.
2. Your application invokes the ```/v1.0/bindings/sms``` endpoint on the Dapr sidecar. In this example, it uses an HTTP POST to invoke the API. You can also use gRPC.
3. The binding component running in the Dapr sidecar calls the external messaging system to send the message. The message will contain the payload passed in the POST request.

## Implementing Bindings into our application

To see Bindings in actions, we're going to extend our background service to work with an external system outside our application. This system owns an Azure Storage Queue which our application reacts to through an Input Binding that receives and processes the message coming to the storage queue.

Once this message is processed and stores the task into Cosmos DB, the system will trigger an event via an Output binding that stores the content of the message into an Azure Blob Storage container.

We will also remove the SendGrid SDK as well as the custom code created in the previous module to send emails and replace it with Dapr SendGrid output bindings.

![](https://azure.github.io/aca-dotnet-workshop/assets/images/06-aca-dapr-bindingsapi/simple-binding.jpg)

First, we'll need to create an Azure Storage account to start responding to messages published to a queue, and store blob files as external events. Open a termainal and run the following PowerShell commands to create an Azure Storage Account and get the master key:

```powershell
$STORAGE_ACCOUNT_NAME = "<replace with a globally unique storage name."

az storage account create `
--name $STORAGE_ACCOUNT_NAME `
--resource-group $RG_NAME `
--location $LOCATION `
--sku Standard_LRS `
--kind StorageV2

# list azure storage keys
az storage account keys list -g $RG_NAME -n $STORAGE_ACCOUNT_NAME
```

Now we can create an event handler, which will be an API endpoint, in our Processor project to respond to messages published to Azure Storage Queue.

Add a new controller in your Processor project called **ExternalTasksProcessorController.cs**, and add the following code:

```csharp
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using TasksTracker.Processor.Backend.Svc.Models;

namespace TasksTracker.Processor.Backend.Svc.Controllers
{
    [Route("ExternalTasksProcessor")]
    [ApiController]
    public class ExternalTasksProcessorController : ControllerBase
    {
        private readonly ILogger<ExternalTasksProcessorController> _logger;
        private readonly DaprClient _daprClient;

        public ExternalTasksProcessorController(ILogger<ExternalTasksProcessorController> logger,
                                                DaprClient daprClient)
        {
            _logger = logger;
            _daprClient = daprClient;
        }

        [HttpPost("process")]
        public async Task<IActionResult> ProcessTaskAndStore([FromBody] TaskModel taskModel)
        {
            try
            {
                _logger.LogInformation("Started processing external task message from storage queue. Task Name: '{0}'", taskModel.TaskName);

                taskModel.TaskId = Guid.NewGuid();
                taskModel.TaskCreatedOn = DateTime.UtcNow;

                //Dapr SideCar Invocation (save task to a state store)
                await _daprClient.InvokeMethodAsync(HttpMethod.Post, "tasksmanager-backend-api", $"api/tasks", taskModel);

                _logger.LogInformation("Saved external task to the state store successfully. Task name: '{0}', Task Id: '{1}'", taskModel.TaskName, taskModel.TaskId);

                //ToDo: code to invoke external binding and store queue message content into blob file in Azure storage

                return Ok();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
```

Let's break this code down:

- We defined an action method named ```ProcessTaskAndStore``` which can be accessed by sending HTTP POST operation on the endpoint ```ExternalTasksProcessor/Process```.
- This action method accepts the TaskModel in the request body as JSON payload.This is what will be received from the external service (Azure Storage Queue).
- Within this action method, we are going to store the received task by sending a POST request to ```/api/tasks``` which is part of the backend api named ```tasksmanager-backend-api```.
- Then we return ```200 OK``` to acknowledge that message received is processed successfully and should be removed from the external service queue.

We can now create the Input Binding component file which defines how our processor will handle events from our Azure Storage Queue. To do this, create a new component file called ```dapr-bindings-in-storagequeue.yaml``` in your **components** folder:

```yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: externaltasksmanager
spec:
  type: bindings.azure.storagequeues
  version: v1
  metadata:
    - name: storageAccount
      value: "<Your Storage Account Name>"
    - name: storageAccessKey
      value: "<Your Storage Account Key>"
    - name: queue
      value: "external-tasks-queue"
    - name: decodeBase64
      value: "true"
    - name: route
      value: /externaltasksprocessor/process
```

We also need to create the component configuration file that will describe how our backend API will be able to invoke the external service, using Blob Storage. This will be able to create and store a JSON blob file that contains the content of the message that we receive from our Azure Storage Queue.

To do this, create another component file called ```dapr-binding-out-blobstorage.yaml``` in your **components** folder:

```yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: externaltasksblobstore
spec:
  type: bindings.azure.blobstorage
  version: v1
  metadata:
    - name: storageAccount
      value: "<Your Storage Account Name>"
    - name: storageAccessKey
      value: "<Your Storage Account Key>"
    - name: container
      value: "externaltaskscontainer"
    - name: decodeBase64
      value: false
```

We can now update our **ExternalTasksProcessorController.cs** controller to invoke the output binding by using the Dapr SDK:

```csharp
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using TasksTracker.Processor.Backend.Svc.Models;

namespace TasksTracker.Processor.Backend.Svc.Controllers
{
    [Route("ExternalTasksProcessor")]
    [ApiController]
    public class ExternalTasksProcessorController : ControllerBase
    {
        private readonly ILogger<ExternalTasksProcessorController> _logger;
        private readonly DaprClient _daprClient;
        private const string OUTPUT_BINDING_NAME = "externaltasksblobstore";
        private const string OUTPUT_BINDING_OPERATION = "create";

        public ExternalTasksProcessorController(ILogger<ExternalTasksProcessorController> logger,
                                                DaprClient daprClient)
        {
            _logger = logger;
            _daprClient = daprClient;
        }

        [HttpPost("process")]
        public async Task<IActionResult> ProcessTaskAndStore([FromBody] TaskModel taskModel)
        {
            try
            {
                _logger.LogInformation("Started processing external task message from storage queue. Task Name: '{0}'", taskModel.TaskName);

                taskModel.TaskId = Guid.NewGuid();
                taskModel.TaskCreatedOn = DateTime.UtcNow;

                //Dapr SideCar Invocation (save task to a state store)
                await _daprClient.InvokeMethodAsync(HttpMethod.Post, "tasksmanager-backend-api", $"api/tasks", taskModel);

                _logger.LogInformation("Saved external task to the state store successfully. Task name: '{0}', Task Id: '{1}'", taskModel.TaskName, taskModel.TaskId);

                //code to invoke external binding and store queue message content into blob file in Azure storage
                IReadOnlyDictionary<string,string> metaData = new Dictionary<string, string>()
                    {
                        { "blobName", $"{taskModel.TaskId}.json" },
                    };


                 await _daprClient.InvokeBindingAsync(OUTPUT_BINDING_NAME, OUTPUT_BINDING_OPERATION, taskModel, metaData);

                _logger.LogInformation("Invoked output binding '{0}' for external task. Task name: '{1}', Task Id: '{2}'", OUTPUT_BINDING_NAME, taskModel.TaskName, taskModel.TaskId);

                return Ok();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
```

In our last module, we sent notification emails when tasks are assigned to a user by using the SendGrid SDK and writing custom code to handle this. Dapr can simplify this by using an Output Binding.

Let's replace our custom code implementation with Dapr! To begin, let's create a new Output Binding that uses SendGrid. Creae a new file called **dapr-bindings-out-sendgrid.yaml** in your **components** folder:

```yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: sendgrid
spec:
  type: bindings.twilio.sendgrid
  version: v1
  metadata:
    - name: emailFrom
      value: "<Your email which was white listed with SendGrid when you obtained the API Key>"
    - name: emailFromName
      value: "Tasks Tracker Notification"
    - name: apiKey
      value: "<Send Grid API Key>"
```

We can now remove the SendGrid package from our Backend API and update it to use the Dapr SDK instead. To start, remove the reference to the SendGrid NuGet package from our **csproj** file in our Backend API.

We also need to delete the SDK code in our **TasksNotificatiionController.cs** file like so:

```csharp
using Dapr.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TasksTracker.Processor.Backend.Svc.Models;

namespace TasksTracker.Processor.Backend.Svc.Controllers
{
    [Route("api/tasksnotifier")]
    [ApiController]
    public class TasksNotifierController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly ILogger _logger;
        private readonly DaprClient _daprClient;
        public TasksNotifierController(IConfiguration config, ILogger<TasksNotifierController> logger, DaprClient daprClient)
        {
            _config = config;
            _logger = logger;
            _daprClient = daprClient;
        }

        [Dapr.Topic("dapr-pubsub-servicebus", "tasksavedtopic")]
        [HttpPost("tasksaved")]
        public async Task<IActionResult> TaskSaved([FromBody] TaskModel taskModel)
        {
            _logger.LogInformation("Started processing message with Task Name '{0}'", taskModel.TaskName);

            var sendGridResponse = await SendEmail(taskModel);

            if (sendGridResponse)
            {
                return Ok();
            }

            return BadRequest("Failed to send an email");
        }

        private async Task<bool> SendEmail(TaskModel taskModel)
        {
            var integrationEnabled = _config.GetValue<bool>("SendGrid:IntegrationEnabled");
            var sendEmailResponse = true;
            var subject = $"Task '{taskModel.TaskName}' is assigned to you!";
            var plainTextContent = $"Task '{taskModel.TaskName}' is assigned to you. Task should be completed by the end of: {taskModel.TaskDueDate.ToString("dd/MM/yyyy")}";

            try
            {
                //Send actual email using Dapr SendGrid Outbound Binding (Disabled when running load test)
                if (integrationEnabled)
                {
                    IReadOnlyDictionary<string, string> metaData = new Dictionary<string, string>()
                {
                    { "emailTo", taskModel.TaskAssignedTo },
                    { "emailToName", taskModel.TaskAssignedTo },
                    { "subject", subject }
                };
                    await _daprClient.InvokeBindingAsync("sendgrid", "create", plainTextContent, metaData);
                }
                else
                {
                    //Introduce artificial delay to slow down message processing
                    _logger.LogInformation("Simulate slow processing for email sending for Email with Email subject '{0}' Email to: '{1}'", subject, taskModel.TaskAssignedTo);
                    Thread.Sleep(1000);
                }

                if (sendEmailResponse)
                {
                    _logger.LogInformation("Email with subject '{0}' sent to: '{1}' successfully", subject, taskModel.TaskAssignedTo);
                }
            }
            catch (System.Exception ex)
            {
                sendEmailResponse = false;
                _logger.LogError(ex, "Failed to send email with subject '{0}' To: '{1}'.", subject, taskModel.TaskAssignedTo);
                throw;
            }
            return sendEmailResponse;
        }
    }
}
```

In the next module, we'll [deploy our Dapr application to Azure Container Apps](../6.DeployToAzure/DeployToACA.md).