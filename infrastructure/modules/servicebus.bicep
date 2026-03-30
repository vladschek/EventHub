@description('Azure region for the namespace.')
param location string

@description('Globally unique Service Bus namespace name (3–50 chars, alphanumeric-hyphen).')
param namespaceName string

@description('Queue name for new events — must stay `events` per product architecture.')
param queueName string = 'events'

resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  name: namespaceName
  location: location
  sku: {
    name: 'Standard'
  }
  properties: {
    minimumTlsVersion: '1.2'
  }
}

resource eventsQueue 'Microsoft.ServiceBus/namespaces/queues@2022-10-01-preview' = {
  parent: serviceBusNamespace
  name: queueName
  properties: {
    maxDeliveryCount: 10
  }
}

resource rootAuthRule 'Microsoft.ServiceBus/namespaces/authorizationRules@2022-10-01-preview' existing = {
  parent: serviceBusNamespace
  name: 'RootManageSharedAccessKey'
}

output namespaceId string = serviceBusNamespace.id
output namespaceNameOut string = serviceBusNamespace.name
output queueNameOut string = eventsQueue.name
@secure()
output connectionString string = rootAuthRule.listKeys().primaryConnectionString
