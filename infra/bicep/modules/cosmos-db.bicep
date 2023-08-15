@description('The location to deploy our resources.')
param location string

@description('The name of the Cosmos DB account')
param cosmosDbAccountName string

@description('The name of the Cosmos DB database')
param cosmosDbName string

@description('The name of the Cosmos DB container')
param cosmosDbContainerName string

@description('The tags assigned to the created resources')
param tags object

resource cosmosDb 'Microsoft.DocumentDB/databaseAccounts@2023-04-15' = {
  name: cosmosDbAccountName
  location: location
  tags: tags
  properties: {
    databaseAccountOfferType: 'Standard'
    publicNetworkAccess: 'Enabled'
    locations: [
      {
        locationName: location
        failoverPriority: 0
        isZoneRedundant: false
      }
    ]
    capabilities: [
      {
        name: 'EnableServerless'
      }
    ]
  }
}

resource cosmosDbDatabase 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2023-04-15' = {
  name: cosmosDbName
  parent: cosmosDb
  properties: {
    resource: {
      id: cosmosDbName
    }
  }
}

resource cosmosDbContainer 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-04-15' = {
  name: cosmosDbContainerName
  parent: cosmosDbDatabase
  properties: {
    resource: {
      id: cosmosDbContainerName
      partitionKey: {
        paths: [
          '/id'
        ]
        kind: 'Hash'
      }
    }
    options: {
      throughput: 400
    }
  }
}

@description('The name of the Cosmos DB resource')
output cosmosDbName string = cosmosDb.name

@description('The name of the Cosmos DB dataase')
output cosmosDbDatabaseName string = cosmosDbDatabase.name

@description('The name of the Cosmos DB container')
output cosmosDbContainerName string = cosmosDbContainer.name
