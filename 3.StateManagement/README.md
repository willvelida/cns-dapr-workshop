## Working with State Management

In this module, we'll save our conference sessions into Azure Cosmos DB using Dapr State Management. We'll do this without having to use the Cosmos DB SDK or write any specific code to integrate our API with Azure Cosmos DB.

In this module, you'll see that you can switch between different state stores without have to make any code changes. This is possible thanks to the Dapr pluggable state stores feature. All we need to do is add a new Dapr Component file, and the underlying state store will be changed.

## How does the State Management API work?

The Dapr State Management API allows you to save, read, and query the key/value pairs in supported state stores.

Under the hood, the sidecar API consumes the state store component to persist data. As developers, we can choose a variety of supported state stores.

The API can be called with either HTTP or gRPC. For example:

```http
http://localhost:<dapr-port>/v1.0/state/<store-name>
```

- `<dapr-port>`: The HTTP port that Dapr listens on.
- `<store-name>`: The name of the state store component to use.

Let's use the *SessionManager* application to illustrate this:

Put diagram here

Let's break these steps down:

1. Our *SessionManager.API* service calls the state management API on the Dapr sidecar. The body of our request is a JSON array that can contain multiple key/value pairs.
2. The Dapr sidecar stores the state based on the state store component configured. In our case, this is Azure Cosmos DB.
3. The sidecar persists the data to Cosmos DB.

Retreiving our session is just a API call. For example:

```console
curl http://localhost:<dapr-port>/v1.0/state/<store-name>/<key-value>
```

## Implementing State Management in our application