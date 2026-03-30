@description('Azure region.')
param location string

@description('App Service plan name (unique within the resource group).')
param planName string

@description('Web app name (globally unique under *.azurewebsites.net).')
param appName string

@description('SQL server FQDN (no tcp/ prefix).')
param sqlServerFqdn string

@description('Database name on the server.')
param sqlDatabaseName string

@description('SQL login user id.')
param sqlUser string

@secure()
@description('SQL login password.')
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
    name: 'B1'
    tier: 'Basic'
  }
  properties: {
    reserved: true
  }
}

resource webApp 'Microsoft.Web/sites@2023-01-01' = {
  name: appName
  location: location
  kind: 'app,linux'
  properties: {
    serverFarmId: plan.id
    httpsOnly: true
    siteConfig: {
      // If deployment fails here, set to DOTNETCORE|8.0 until the region supports .NET 10.
      linuxFxVersion: 'DOTNETCORE|10.0'
      appSettings: [
        { name: 'ASPNETCORE_ENVIRONMENT', value: 'Production' }
        { name: 'ConnectionStrings__DefaultConnection', value: sqlConnectionString }
        { name: 'ServiceBus__ConnectionString', value: serviceBusConnectionString }
        { name: 'ServiceBus__DisablePublishing', value: 'false' }
        { name: 'APPLICATIONINSIGHTS_CONNECTION_STRING', value: applicationInsightsConnectionString }
      ]
    }
  }
}

output apiUrl string = 'https://${webApp.properties.defaultHostName}'
output webAppName string = webApp.name
