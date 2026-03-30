// Full EventHub stack (observability, SQL, Service Bus, storage, API, Functions).
// Deploy to an existing resource group:
//
//   az group create -n rg-eventhub-dev -l eastus
//   az deployment group create -g rg-eventhub-dev -f infrastructure/main.bicep -p sqlAdministratorPassword='<secure-password>'
//
// Optional overrides: sqlServerName, serviceBusNamespaceName, storageAccountName (must be globally unique where required).

targetScope = 'resourceGroup'

@description('Azure region (defaults to resource group location).')
param location string = resourceGroup().location

@secure()
@minLength(8)
@description('Password for the SQL server administrator login (meet Azure SQL password policy).')
param sqlAdministratorPassword string

@description('SQL administrator login name.')
param sqlAdministratorLogin string = 'sqladmin'

@description('Application database name.')
param sqlDatabaseName string = 'EventHub'

@description('Globally unique SQL server name.')
param sqlServerName string = 'sql-eh-${uniqueString(resourceGroup().id)}'

@description('Globally unique Service Bus namespace (3–50 chars, alphanumeric-hyphen).')
param serviceBusNamespaceName string = 'sb-eh-${uniqueString(resourceGroup().id)}'

@description('Globally unique storage account name (3–24, lowercase letters and numbers only).')
param storageAccountName string = take('ehstore${uniqueString(resourceGroup().id)}', 24)

var resourceToken = uniqueString(resourceGroup().id)
var apiWebAppName = 'app-eh-${resourceToken}'
var functionAppName = 'func-eh-${resourceToken}'
var contentShareName = take(replace(toLower('${functionAppName}cnt'), '-', ''), 63)

module monitoring 'modules/loganalytics.bicep' = {
  name: 'monitoring'
  params: {
    location: location
    baseName: resourceToken
  }
}

module serviceBus 'modules/servicebus.bicep' = {
  name: 'serviceBus'
  params: {
    location: location
    namespaceName: serviceBusNamespaceName
    queueName: 'events'
  }
}

module sql 'modules/sql.bicep' = {
  name: 'sql'
  params: {
    location: location
    serverName: sqlServerName
    databaseName: sqlDatabaseName
    administratorLogin: sqlAdministratorLogin
    administratorPassword: sqlAdministratorPassword
  }
}

module storage 'modules/storage.bicep' = {
  name: 'storage'
  params: {
    location: location
    storageAccountName: storageAccountName
  }
}

module api 'modules/appservice-api.bicep' = {
  name: 'api'
  params: {
    location: location
    planName: 'plan-api-${resourceToken}'
    appName: apiWebAppName
    sqlServerFqdn: sql.outputs.serverFqdn
    sqlDatabaseName: sql.outputs.databaseNameOut
    sqlUser: sqlAdministratorLogin
    sqlPassword: sqlAdministratorPassword
    serviceBusConnectionString: serviceBus.outputs.connectionString
    applicationInsightsConnectionString: monitoring.outputs.appInsightsConnectionString
  }
}

module functions 'modules/functionapp.bicep' = {
  name: 'functions'
  params: {
    location: location
    planName: 'plan-func-${resourceToken}'
    functionAppName: functionAppName
    contentShareName: contentShareName
    storageConnectionString: storage.outputs.connectionString
    sqlServerFqdn: sql.outputs.serverFqdn
    sqlDatabaseName: sql.outputs.databaseNameOut
    sqlUser: sqlAdministratorLogin
    sqlPassword: sqlAdministratorPassword
    serviceBusConnectionString: serviceBus.outputs.connectionString
    applicationInsightsConnectionString: monitoring.outputs.appInsightsConnectionString
  }
}

output apiBaseUrl string = api.outputs.apiUrl
output functionAppBaseUrl string = functions.outputs.functionAppUrl
output sqlServerFqdn string = sql.outputs.serverFqdn
output sqlDatabaseName string = sql.outputs.databaseNameOut
output serviceBusNamespace string = serviceBus.outputs.namespaceNameOut
output applicationInsightsName string = 'ai-${resourceToken}'
output postDeployHint string = 'Run EF migrations against sqlServerFqdn; configure SPA apiUrl to apiBaseUrl (HTTPS).'
