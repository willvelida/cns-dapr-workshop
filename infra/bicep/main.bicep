@description('The location to deploy our resources. Default is the location of the resource group.')
param location string = resourceGroup().location

@description('The suffix applied to our resources')
param applicationSuffix string = uniqueString(resourceGroup().id)

@description('The name of the container apps environment')
param containerAppEnvName string = 'env-${applicationSuffix}}'

@description('The name of the Log Analytics Workspace.')
param logAnalyticsWorkspaceName string = 'law-${applicationSuffix}'

@description('The name of the Application Insights workspace')
param applicationInsightsName string = 'ai-${applicationSuffix}'

@description('The name of the Azure Container Registry')
param containerRegistryName string = 'acr${applicationSuffix}'

@description('The name of the Cosmos DB account')
param cosmosDbAccountName string = 'db-${applicationSuffix}'

@description('The name of the Cosmos DB database')
param cosmosDbName string = 'contactsdb'

@description('The name of the Cosmos DB Container')
param cosmosDbContainerName string = 'contacts'

@description('The name of the Service Bus namespace')
param serviceBusName string = 'sb-${applicationSuffix}'

@description('The name of the Service Bus topic')
param serviceBusTopicName string = 'contactsavedtopic'

@description('The name of the Service Bus Authorization Rule')
param serviceBusTopicAuthRuleName string = 'contactsavedtopic-manage-policy'

@description('The tags assigned to the created resources')
param tags object = {
  Environment: 'Development'
  ApplicationName: 'CNS_Dapr_Workshop'
}

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: logAnalyticsWorkspaceName
  location: location
  tags: tags
  properties: {
    features: {
      searchVersion: 1
    }
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: applicationInsightsName
  location: location
  kind: 'web'
  tags: tags
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalytics.id
  }
}

resource containerAppEnv 'Microsoft.App/managedEnvironments@2023-05-01' = {
  name: containerAppEnvName
  location: location
  tags: tags
  properties: {
    daprAIConnectionString: appInsights.properties.ConnectionString
    daprAIInstrumentationKey: appInsights.properties.InstrumentationKey
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalytics.properties.customerId
        sharedKey: logAnalytics.listKeys().primarySharedKey
      }
    }
  }
}

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-06-01-preview' = {
  name: containerRegistryName
  location: location
  tags: tags
  sku: {
    name: 'Basic'
  }
  properties: {
    adminUserEnabled: true
  }
}

module cosmosDb 'modules/cosmos-db.bicep' = {
  name: 'cosmosDb'
  params: {
    cosmosDbAccountName: cosmosDbAccountName 
    cosmosDbContainerName: cosmosDbContainerName
    cosmosDbName: cosmosDbName
    location: location
    tags: tags
  }
}

module serviceBus 'modules/service-bus.bicep' = {
  name: 'serviceBus'
  params: {
    location: location
    serviceBusName: serviceBusName
    serviceBusTopicAuthRuleName: serviceBusTopicAuthRuleName
    serviceBusTopicName: serviceBusTopicName
    serviceBusTopicSubscriberName: 'contact-backend-processor'
    tags: tags
  }
}
