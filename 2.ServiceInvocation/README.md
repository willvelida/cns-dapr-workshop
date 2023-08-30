# Working with Service Invocation

In this module, we'll start integrating Dapr into both of our services and see how Dapr simplifies communication between microservices using Service Invocation.

This module introduces our application *SessionManager*. This is a basic application that demonstrates a conference session management application following the mircoservices pattern. In this workshop, we will work with 3 microservices:

1. **SessionManager.API** - This is a simple ASP.NET Razor pages web application that accepts requests from speakers to manager their conference sessions.
2. **SessionManager.UI** - This is a Web API that contains our business logic, and will be responsible for data storage, and pub/sub capabilities.
3. **SessionManager.Processor** - This is a background processor that will deal with various background tasks.

As we work through the workshop, we'll be adding various Azure resources to support different Dapr components.

## How Service Invocation works

Calling between services in distributed applications can be a challenge. Some of these challenges include:

* Where the other services are location.
* How to call other services securely, given the service address
* How to handle retries when short-lived transient errors occur.
* Capturing insights across service calls (Critical to debugging issues in Production!)

Dapr addresses these challenges by providing a service invocation API that acts as a reverse proxy with built-in service discovery, while also benefiting from built-in distributed tracing, metrics, error handling, encryption etc.

Using our *SessionManager* application as an example, our UI (Service A) will need to call the ```/api/sessions``` endpoint on our API (Service B). While Service A could take a dependency on Service B and make a direct call to it, Service A instead invokes the service invocation API on the Dapr sidecar. 


Let's break this down:

## Implementing Service invocation in our application.