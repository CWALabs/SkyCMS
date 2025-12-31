// Azure Web App (App Service) module for SkyCMS Editor
// Hosts the containerized Editor application as a Web App for Containers

@description('Location for all resources')
param location string = resourceGroup().location

@description('Name of the Web App')
param webAppName string

@description('Name of the App Service plan')
param planName string

@description('Docker image for the Web App')
param imageName string = 'toiyabe/sky-editor:latest'

@description('Database connection string')
@secure()
param dbConnectionString string

@description('Storage connection string (optional)')
@secure()
param storageConnectionString string = ''

@description('Administrator email address')
param adminEmail string = ''

@description('Publisher static website URL')
param publisherUrl string = ''

@description('Managed identity ID for Key Vault access')
param managedIdentityId string

@description('App Service SKU name')
param skuName string = 'B2'

@description('App Service SKU tier')
param skuTier string = 'Basic'

@description('App Service plan instance count')
@minValue(1)
param capacity int = 1

@description('Tags to apply to resources')
param tags object = {}

// Build connection strings array
var connectionStrings = [
  {
    name: 'ApplicationDbContextConnection'
    connectionString: dbConnectionString
    type: 'Custom'
  }
  {
    name: 'StorageConnectionString'
    connectionString: storageConnectionString
    type: 'Custom'
  }
]

// Essential app settings for Single Tenant mode
var appSettings = [
  {
    name: 'WEBSITES_ENABLE_APP_SERVICE_STORAGE'
    value: 'false'
  }
  {
    name: 'WEBSITES_PORT'
    value: '8080'
  }
  {
    name: 'ASPNETCORE_ENVIRONMENT'
    value: 'Production'
  }
  {
    name: 'CosmosAllowSetup'
    value: 'true'
  }
  {
    name: 'AdminEmail'
    value: adminEmail
  }
  {
    name: 'CosmosPublisherUrl'
    value: publisherUrl
  }
]

// App Service Plan (Linux)
resource appServicePlan 'Microsoft.Web/serverfarms@2022-09-01' = {
  name: planName
  location: location
  tags: tags
  sku: {
    name: skuName
    tier: skuTier
    size: skuName
    capacity: capacity
  }
  kind: 'linux'
  properties: {
    reserved: true // required for Linux plans
  }
}

// Web App for Containers
resource webApp 'Microsoft.Web/sites@2022-09-01' = {
  name: webAppName
  location: location
  tags: tags
  kind: 'app,linux,container'
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentityId}': {}
    }
  }
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    keyVaultReferenceIdentity: managedIdentityId
    siteConfig: {
      linuxFxVersion: 'DOCKER|${imageName}'
      alwaysOn: true
      ftpsState: 'Disabled'
      connectionStrings: connectionStrings
      appSettings: appSettings
      httpLoggingEnabled: true
      detailedErrorLoggingEnabled: true
      requestTracingEnabled: true
    }
  }
}

// Enable file system logging
resource webAppLogs 'Microsoft.Web/sites/config@2022-09-01' = {
  name: 'logs'
  parent: webApp
  properties: {
    applicationLogs: {
      fileSystem: {
        level: 'Information'
      }
    }
    httpLogs: {
      fileSystem: {
        enabled: true
        retentionInMb: 35
        retentionInDays: 7
      }
    }
    failedRequestsTracing: {
      enabled: true
    }
    detailedErrorMessages: {
      enabled: true
    }
  }
}

@description('The hostname of the Web App')
output hostName string = webApp.properties.defaultHostName

@description('The URL of the Web App')
output url string = 'https://${webApp.properties.defaultHostName}'

@description('The name of the Web App')
output webAppName string = webApp.name

@description('The resource ID of the Web App')
output webAppId string = webApp.id

@description('The name of the App Service plan')
output planName string = appServicePlan.name

@description('The resource ID of the App Service plan')
output planId string = appServicePlan.id
