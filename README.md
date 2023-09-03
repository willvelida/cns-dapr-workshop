# Building portable and reliable microservices with the Dapr Framework and Azure

Managing the complexities of distributed systems can be a daunting task for even the most experienced software engineers. This workshop aims to equip software engineers with the knowledge and skills to build portable and reliable microservices using the Distributed Application Runtime (Dapr) framework.

In this workshop, you will learn:

- The fundamental concepts and architecture of the Dapr Framework.
- What Dapr building blocks are, including Service Invocation, State Management, Pub/Sub and Bindings.
- Deploying Dapr applications to Azure Container Apps.

## Workshop Prerequisties

- [.NET 7](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)
- Docker Desktop
    - [Windows](https://docs.docker.com/desktop/install/windows-install/)
    - [Mac](https://docs.docker.com/desktop/install/mac-install/)
- An Azure Subscription
- [Visual Studio Code](https://code.visualstudio.com/download), or Visual Studio (If you have JetBrains, that's fine 😊 we'll be running our apps via the CLI anyway).
- [Azure Storage Explorer](https://azure.microsoft.com/en-us/products/storage/storage-explorer/#overview)
- [Azure CLI ](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli)

We will install Dapr as part of this workshop.

## Workshop lessons

1. [Introduction to Dapr and installing locally.](./1.IntroToDapr/README.md)
1. [Working with Service Invocation.](./2.ServiceInvocation/README.md)
1. [Working with State Management.](./3.StateManagement/README.md)
1. [Working with Pub/Sub.](./4.Pub-Sub/README.md)
1. [Working with Bindings in Dapr.](./5.Bindings/README.md)
1. Deploying Dapr Applications to Azure.
    - [ Deploy to Azure Container Apps.](./6.DeployToAzure/DeployToACA.md)