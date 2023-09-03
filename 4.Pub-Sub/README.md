# Working with Dapr Pub/Sub

The Publish/Subscribe messaging pattern (or Pub/Sub for short) is well known and widely used in distributed applications. Putting it together can be a little complex, as there are subtle differences between different messaging products. The Pub/Sub building block in Dapr provides a simplified implementation of pub/sub functionality.

In this module, we'll be adding a new service that will be responsible for letting users know when tasks are assigned to them. In order to send messages to this service, we will add Pub/Sub capability to our backend API which will simply tell our new service who to notify, as we just want our API to deal with state.

First, let's dive into Pub/Sub capabilities in Dapr, and what problems it solves.

## How Pub/Sub in Dapr works

The Pub/Sub building block provides a platform-agnostic API to send and receive messages. Messages are published to a topic, and services subscribe to that topic to consume those messages. The service calls the Pub/Sub API on the Dapr sidecar, which then makes calls into a pre-defined Dapr pub/sub component.

![The Dapr pub/sub stack](https://learn.microsoft.com/en-us/dotnet/architecture/dapr-for-net-developers/media/publish-subscribe/pub-sub-buildingblock.png)

We can invoke the Pub/Sub API using HTTP or gRPC. For example, to publish a message we can run the following:

```http
GET http://localhost:<dapr-port>/v1.0/publish/<pub-sub-name>/<topic>
```

There are several Dapr specific URL segments in the above call:

- **dapr-port** provides the port number upon which the Dapr sidecar is listening.
- **pub-sub-name** provides the name of the selected Dapr pub/sub component.
- **topic** provides the name of the topic to which the message is published.

Using the curl command-line tool to publish a message, you can try it out:

```curl
curl -X POST http://localhost:3500/v1.0/publish/pubsub/newOrder \
  -H "Content-Type: application/json" \
  -d '{ "orderId": "1234", "productId": "5678", "amount": 2 }'
```

To receive this message, services need to subscribe to a topic. At application startup, the Dapr runtime will call the application on a endpoint to identify and create the required subscription:

```http
http://localhost:<app-port>/dapr/subscribe
```

The response from the call contains a list of topics that the application will subscribe to. Each will include an endpoint to call when the topic receives a message. An example response may look like this:

```json
[
  {
    "pubsubname": "pubsub",
    "topic": "newOrder",
    "route": "/orders"
  },
  {
    "pubsubname": "pubsub",
    "topic": "newProduct",
    "route": "/productCatalog/products"
  }
]
```

Your application will register both the ```/orders``` and ```/productCatalog/products``` endpoints. For both subscriptions, it will bind to the Dapr component named ```pubsub```.

Let's visualize this:

![pub/sub flow with Dapr](https://learn.microsoft.com/en-us/dotnet/architecture/dapr-for-net-developers/media/publish-subscribe/pub-sub-flow.png)

1. The Dapr sidecar for Service B will call the ```/dapr/subscribe``` endpoint from Service B (which is our consumer). The service responds with the subscriptions it wants to create.
2. The sidecar for Service B creates the subscription on the message broker.
3. Service A publishes a message at the ```/v1.0/publish/<pub-sub-name>/<topic>``` endpoint on the Service A sidecar.
4. Service A sidecar publishes the message to the broker.
5. The broker sends a copy of the message to the Service B sidecar.
6. Service B sidecar calls the endpoint corresponding to the subscription on Service B. The service responds with an 200 HTTP status code so the sidecar considers the message as being handled successfully.

## Implementing Pub/Sub capability into our application

Let's implement Pub/Sub into our application. For this, we'll need to create a new Web API project named **TasksTracker.Processor.Backend.Svc**. Open up a termainal, navigate to the root folder of your project and run the following:

```dotnet
dotnet new webapi -o TasksTracker.Processor.Backend.Svc
```

We'll also need to add a docker file so we can containerize this application. To do so Open the VS Code Command Palette (Ctrl+Shift+P) and select Docker: Add Docker Files to Workspace...

- Use .NET: ASP.NET Core when prompted for application platform.
- Choose Linux when prompted to choose the operating system.
- You will be asked if you want to add Docker Compose files. Select No.
- Take a note of the provided application port as we will be using later on. You can always find it again inside the designated DockerFile inside the newly created project's directory.
- Dockerfile and .dockerignore files are added to the workspace.

We'll also need to add the model that will be used to represent our published message. Create a new folder called **Models** and create the following **TaskModel.cs** class:

```csharp
public class TaskModel
    {
        public Guid TaskId { get; set; }
        public string TaskName { get; set; } = string.Empty;
        public string TaskCreatedBy { get; set; } = string.Empty;
        public DateTime TaskCreatedOn { get; set; }
        public DateTime TaskDueDate { get; set; }
        public string TaskAssignedTo { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public bool IsOverDue { get; set; }
    }
```

We'll also need to install the Dapr SDK to be able to work with Pub/Sub programmatically. To do so, we can run the following command in the Processor project directory:

```dotnet
Install-Package Dapr.AspNetCore
```

We're also going to be sending emails via SendGrid, so we'll need to install the SendGrid NuGet package, which we can do like so:

```dotnet
Install-Package SendGrid
```

Now, we can add an enpoint that will be responsible for subscribing to the topic in the message broker we will configure. This endpoint will start receiving messages that are published from our Backend API. Add a new controller called **TasksNotifierController.cs** in our **Controllers** folder:

```csharp
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using SendGrid;
using SendGrid.Helpers.Mail;
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

            if (sendGridResponse.Item1)
            {
                return Ok($"SendGrid response staus code: {sendGridResponse.Item1}");
            }

            return BadRequest($"Failed to send email, SendGrid response status code: {sendGridResponse.Item1}");
        }

        private async Task<Tuple<bool, string>> SendEmail(TaskModel taskModel)
        {

            var apiKey = _config.GetValue<string>("SendGrid:ApiKey");
            var sendEmailResponse = true;
            var sendEmailStatusCode = System.Net.HttpStatusCode.Accepted;
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("taiseer.joudeh@gmail.com", "Tasks Tracker Notification");
            var subject = $"Task '{taskModel.TaskName}' is assigned to you!";
            var to = new EmailAddress(taskModel.TaskAssignedTo, taskModel.TaskAssignedTo);
            var plainTextContent = $"Task '{taskModel.TaskName}' is assigned to you. Task should be completed by the end of: {taskModel.TaskDueDate.ToString("dd/MM/yyyy")}";
            var htmlContent = plainTextContent;
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

         var response = await client.SendEmailAsync(msg);
         sendEmailResponse = response.IsSuccessStatusCode;
         sendEmailStatusCode = response.StatusCode;

         return new Tuple<bool, string>(sendEmailResponse, sendEmailStatusCode.ToString());

        }
    }
}
```

Let's break this code down:

- We have added an action method named TaskSaved which can be accessed on the route ```api/tasksnotifier/tasksaved```
- We have attributed this action method with the attribute ```Dapr.Topic``` which accepts the Dapr pub/sub component to target as the first argument, and the second argument is the topic to subscribe to, which in our case is ```tasksavedtopic```.
- The action method expects to receive a ```TaskModel``` object.
- Now once the message is received by this endpoint, we can start out the business logic to trigger sending an email (more about this next) and then return ```200 OK``` response to indicate that the consumer processed the message successfully and the broker can delete this message.
- If anything went wrong during sending the email (i.e. Email service not responding) and we want to retry processing this message at a later time, we return ```400 Bad Request```, which will inform the message broker that the message needs to be retired based on the configuration in the message broker.
- If we need to drop the message as we are aware it will not be processed even after retries (i.e Email to is not formatted correctly) we return a ```404 Not Found``` response. This will tell the message broker to drop the message and move it to dead-letter or poison queue.

In our Processor project, we'll need to add some settings into our ```appsettings.json``` file to work with SendGrid. Once you've signed up and got an API key, you can add it like so:

```json
  {
  "SendGrid": {
    "ApiKey": "",
    "IntegrationEnabled":false
  }
}
```

We'll then need to register the Dapr and Subscribe Handler at the Consumer Startup. To do so, update the ```Program.cs``` file in the Processor project like so:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddDapr();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseCloudEvents();

app.MapControllers();

app.MapSubscribeHandler();

app.Run();
```

Again, let's break this down:

- On line ```builder.Services.AddControllers().AddDapr();```, the extension method ```AddDapr``` registers the necessary services to integrate Dapr into the MVC pipeline. It also registers a ```DaprClient``` instance into the dependency injection container, which then can be injected anywhere into your service. We will see how we are injecting DaprClient in the controller constructor later on.
- On line ```app.UseCloudEvents();```, the extension method ```UseCloudEvents``` adds CloudEvents middleware into the ASP.NET Core middleware pipeline. This middleware will unwrap requests that use the CloudEvents structured format, so the receiving method can read the event payload directly. You can read more about CloudEvents here which includes specs for describing event data in a common and standard way.
- On line ```app.MapSubscribeHandler();```, we make the endpoint ```http://localhost:<appPort>/dapr/subscribe``` available for the consumer so it responds and returns available subscriptions. When this endpoint is called, it will automatically find all WebAPI action methods decorated with the Dapr.Topic attribute and instruct Dapr to create subscriptions for them.

We'll now update our Backend API to publish messages whenever we save tasks. In our Backend API, update the ***TaskStoreManager.cs*** file to include the following:

```csharp
//Add new private method
private async Task PublishTaskSavedEvent(TaskModel taskModel)
{
    _logger.LogInformation("Publish Task Saved event for task with Id: '{0}' and Name: '{1}' for Assigne: '{2}'",
    taskModel.TaskId, taskModel.TaskName, taskModel.TaskAssignedTo);
    await _daprClient.PublishEventAsync("dapr-pubsub-servicebus", "tasksavedtopic", taskModel);
}

//Update the below method:
public async Task<Guid> CreateNewTask(string taskName, string createdBy, string assignedTo, DateTime dueDate)
{
    var taskModel = new TaskModel()
    {
        TaskId = Guid.NewGuid(),
        TaskName = taskName,
        TaskCreatedBy = createdBy,
        TaskCreatedOn = DateTime.UtcNow,
        TaskDueDate = dueDate,
        TaskAssignedTo = assignedTo,
    };

    _logger.LogInformation("Save a new task with name: '{0}' to state store", taskModel.TaskName);
    await _daprClient.SaveStateAsync<TaskModel>(STORE_NAME, taskModel.TaskId.ToString(), taskModel);
    await PublishTaskSavedEvent(taskModel);
    return taskModel.TaskId;
}

//Update the below method:
public async Task<bool> UpdateTask(Guid taskId, string taskName, string assignedTo, DateTime dueDate)
    {
        _logger.LogInformation("Update task with Id: '{0}'", taskId);
        var taskModel = await _daprClient.GetStateAsync<TaskModel>(STORE_NAME, taskId.ToString());
        var currentAssignee = taskModel.TaskAssignedTo;
        if (taskModel != null)
        {
            taskModel.TaskName = taskName;
            taskModel.TaskAssignedTo = assignedTo;
            taskModel.TaskDueDate = dueDate;
            await _daprClient.SaveStateAsync<TaskModel>(STORE_NAME, taskModel.TaskId.ToString(), taskModel);
            if (!taskModel.TaskAssignedTo.Equals(currentAssignee, StringComparison.OrdinalIgnoreCase))
            {
                await PublishTaskSavedEvent(taskModel);
            }
            return true;
        }
        return false;
    }
```

We can now create an Azure Service Bus namespace to act as our message broker. We can do so by running the following AZ CLI commands:

```powershell
$NamespaceName="[your globally unique namespace goes here. e.g. taskstracker-wk-42 where wk are your initials and 42 is the year you were born]"
$TopicName="tasksavedtopic"
$TopicSubscription="tasks-processor-subscription"

##Create servicebus namespace
az servicebus namespace create --resource-group $RG_NAME --name $NamespaceName --location $LOCATION --sku Standard

##Create a topic under the namespace
az servicebus topic create --resource-group $RG_NAME --namespace-name $NamespaceName --name $TopicName

##Create a topic subscription
az servicebus topic subscription create `
--resource-group $RG_NAME `
--namespace-name $NamespaceName `
--topic-name $TopicName `
--name $TopicSubscription

##List connection string
az servicebus namespace authorization-rule keys list `
--resource-group $RG_NAME `
--namespace-name $NamespaceName `
--name RootManageSharedAccessKey `
--query primaryConnectionString `
--output tsv
```

With this, we'll be able to create a Pub/Sub component for our Service Bus message broker:

```yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: dapr-pubsub-servicebus
spec:
  type: pubsub.azure.servicebus
  version: v1
  metadata:
    - name: connectionString # Used for local dev testing.
      value: "<connection string from step 1>"
    - name: consumerID
      value: "tasks-processor-subscription"
scopes:
  - tasksmanager-backend-api
  - tasksmanager-backend-processor
```

