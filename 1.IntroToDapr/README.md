# Getting started with Dapr

In this module, you'll install the Dapr CLI on your local machine and initialize the Dapr runtime so you can start working with Dapr in your applications.

- [Step 1: Install the Dapr CLI](#step-1-install-the-dapr-cli)
- [Step 2: Initialize Dapr](#step-2-initialize-dapr-🚀)
- [More resources and next steps](#end-of-the-section)

## Step 1: Install the Dapr CLI

Our first task will be to install the Dapr CLI.

The Dapr CLI is the main tool that you'll use for various Dapr-related tasks, such as:
- Running an application with a Dapr Sidecar.
- Review sidecar logs.
- List running services.
- Run the Dapr dashboard.

The [Dapr documentation](https://docs.dapr.io/) has detailed, up-to-date guides on installing the Dapr CLI on your local machine. *(It's worth bookmarking the Dapr docs for future reference!)*

To avoid rewriting all those steps, and potentially making this workshop content stale, follow **Step 1: Install the Dapr CLI** step in the documentation to install the CLI on your local machine: https://docs.dapr.io/getting-started/install-dapr-cli/

To verify that the installation was successful, restart your terminal, and run the following command:

```bash
dapr -h
```

The output from that command should look similar to the following:

```bash
         __
    ____/ /___ _____  _____
   / __  / __ '/ __ \/ ___/
  / /_/ / /_/ / /_/ / /
  \__,_/\__,_/ .___/_/
              /_/

===============================
Distributed Application Runtime

Usage:
  dapr [command]

Available Commands:
  completion     Generates shell completion scripts
  components     List all Dapr components. Supported platforms: Kubernetes
  configurations List all Dapr configurations. Supported platforms: Kubernetes
  dashboard      Start Dapr dashboard. Supported platforms: Kubernetes and self-hosted
  help           Help about any command
  init           Install Dapr on supported hosting platforms. Supported platforms: Kubernetes and self-hosted
  invoke         Invoke a method on a given Dapr application. Supported platforms: Self-hosted
  list           List all Dapr instances. Supported platforms: Kubernetes and self-hosted
  logs           Get Dapr sidecar logs for an application. Supported platforms: Kubernetes
  mtls           Check if mTLS is enabled. Supported platforms: Kubernetes
  publish        Publish a pub-sub event. Supported platforms: Self-hosted
  run            Run Dapr and (optionally) your application side by side. Supported platforms: Self-hosted
  status         Show the health status of Dapr services. Supported platforms: Kubernetes
  stop           Stop Dapr instances and their associated apps. . Supported platforms: Self-hosted
  uninstall      Uninstall Dapr runtime. Supported platforms: Kubernetes and self-hosted
  upgrade        Upgrades a Dapr control plane installation in a cluster. Supported platforms: Kubernetes
  version        Print the Dapr runtime and CLI version

Flags:
  -h, --help      help for dapr
  -v, --version   version for dapr

Use "dapr [command] --help" for more information about a command.
```

## Step 2: Initialize Dapr 🚀

Now that the Dapr CLI is installed on your local machine, you can use the CLI to initialize it.

Dapr runs as a sidecar process alongside your application. Since you're running Dapr on your local machine, you're running Dapr in **self-hosted** mode.

By initializing Dapr, you:

- Fetch and install the Dapr sidecar binaries locally.
- Create a development environment that streamlines application development with Dapr.

Dapr initialization includes:

1. Running a Redis container instance to be used as a local state store and message broker.
1. Running a Zipkin container instance for observability.
1. Creating a default components folder with component definitions for the above.
1. Running a Dapr placement service container instance for local actor support.

Let's initialize Dapr on your local machine. You'll need to run an elevated terminal, so start your terminal and run the following commands:

```bash
dapr init
```

This command will install the latest Dapr runtime binaries.

```bash
dapr --version
```

This will verify which version of Dapr you are using. The output will look similar to the following:

```bash
CLI version: 1.11.0
Runtime version: 1.11.0
```

The ```dapr init``` command launches a couple of containers that you use to get started with Dapr. To verify that these container instances have been launched, run the following:

```
docker ps
```

The output should be similar to the following:

```bash
CONTAINER ID   IMAGE                COMMAND                  CREATED       STATUS                   PORTS                              NAMES
a6f4ff6ec599   daprio/dapr:1.11.0   "./placement"            7 weeks ago   Up 9 minutes             0.0.0.0:6050->50005/tcp            dapr_placement
22693c04ff0c   redis:6              "docker-entrypoint.s…"   7 weeks ago   Up 9 minutes             0.0.0.0:6379->6379/tcp             dapr_redis
30d3572f2257   openzipkin/zipkin    "start-zipkin"           7 weeks ago   Up 9 minutes (healthy)   9410/tcp, 0.0.0.0:9411->9411/tcp   dapr_zipkin
```

In this workshop, you'll be using **Docker** as the container runtime. You can also use **Podman** to initialize Dapr. To do so, you can do so by running the following Dapr CLI command:

```bash
dapr init --container-runtime podman
```

## End of the section!

That's the end of this section. In this section, you:

- Learned about the Dapr Framework, including the core concepts and it's architecture.
- Installed the Dapr CLI on your local machine.
- Initialized Dapr on your local machine in self-hosted mode.

If you want to learn more about the basics of Dapr and its architecture, check out the following resources:

- [Dapr Overview](https://docs.dapr.io/concepts/overview/)
- [Dapr Concepts](https://docs.dapr.io/concepts/)
- [Dapr Terminology](https://docs.dapr.io/concepts/terminology/)

Once you're ready, head to the next section, where you will learn about Dapr Service Invocation.
