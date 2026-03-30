@description('Azure region.')
param location string

@description('Consumption plan name (unique within the resource group).')
param planName string

@description('Function app name (globally unique under *.azurewebsites.net).')
param functionAppName string

@description('WEBSITE_CONTENTSHARE — lowercase letters and numbers, max 63.')
param contentShareName string

@secure()
param storageConnectionString string

@description('SQL server FQDN.')
param sqlServerFqdn string

@description('Database name.')
param sqlDatabaseName string

param sqlUser string

@secure()
param sqlPassword string

@secure()
param serviceBusConnectionString string

@secure()
param applicationInsightsConnectionString string

var sqlConnectionString = 'Server=tcp:${sqlServerFqdn},1433;Initial Catalog=${sqlDatabaseName};Persist Security Info=False;User ID=${sqlUser};Password=${sqlPassword};MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'

resource plan 'Microsoft.Web/serverfarms@2023-01-01' = {
  name: planName
  location: location
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
  }
  properties: {
    reserved: true
  }
}

resource functionApp 'Microsoft.Web/sites@2023-01-01' = {
  name: functionAppName
  location: location
  kind: 'functionapp,linux'
  properties: {
    serverFarmId: plan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNET-ISOLATED|10.0'
      appSettings: [
        { name: 'AzureWebJobsStorage', value: storageConnectionString }
        { name: 'WEBSITE_CONTENTAZUREFILECONNECTION', value: storageConnectionString }
        { name: 'WEBSITE_CONTENTSHARE', value: toLower(take(contentShareName, 63)) }
        { name: 'FUNCTIONS_EXTENSION_VERSION', value: '~4' }
        { name: 'FUNCTIONS_WORKER_RUNTIME', value: 'dotnet-isolated' }
        { name: 'ConnectionStrings__DefaultConnection', value: sqlConnectionString }
        { name: 'ServiceBus__ConnectionString', value: serviceBusConnectionString }
        { name: 'ServiceBusConnection', value: serviceBusConnectionString }
        { name: 'APPLICATIONINSIGHTS_CONNECTION_STRING', value: applicationInsightsConnectionString }
      ]
    }
  }
}

output functionAppUrl string = 'https://${functionApp.properties.defaultHostName}'
output functionAppNameOut string = functionApp.name
