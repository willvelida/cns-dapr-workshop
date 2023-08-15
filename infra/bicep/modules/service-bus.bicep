@description('The location to deploy our Service Bus resources.')
param location string

@description('The name of the Service Bus namespace')
param serviceBusName string

@description('The name of the Service Bus Topic')
param serviceBusTopicName string

@description('The name of the Service Bus Topic\'s Auth rule')
param serviceBusTopicAuthRuleName string

@description('The name of the Service Bus Topic Subscriber')
param serviceBusTopicSubscriberName string

@description('The tags assigned to the Service Bus resources')
param tags object

resource serviceBus 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  name: serviceBusName
  location: location
  tags: tags
  sku: {
    name: 'Standard'
  }
}

resource serviceBusTopic 'Microsoft.ServiceBus/namespaces/topics@2022-10-01-preview' = {
  name: serviceBusTopicName
  parent: serviceBus
}

resource serviceBusTopicAuthRule 'Microsoft.ServiceBus/namespaces/topics/authorizationRules@2022-10-01-preview' = {
  name: serviceBusTopicAuthRuleName
  parent: serviceBusTopic
  properties: {
    rights: [
      'Listen'
      'Send'
      'Manage'
    ]
  }
}

resource serviceBusTopicSubscription 'Microsoft.ServiceBus/namespaces/topics/subscriptions@2022-10-01-preview' = {
  name: serviceBusTopicSubscriberName
  parent: serviceBusTopic
}

@description('The name of the Service Bus namespace.')
output serviceBusName string = serviceBus.name

@description('The name of the Service Bus topic')
output serviceBusTopicName string = serviceBusTopic.name

@description('The name of the Service Bus topic\'s Auth rule')
output serviceBusTopicAuthorizationRuleName string = serviceBusTopicAuthRule.name
