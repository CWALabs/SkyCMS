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

@description('Key Vault secret URI for DB connection string')
param dbConnectionSecretUri string

@description('Key Vault secret URI for Storage connection string (optional)')
param storageConnectionSecretUri string = ''

@description('Key Vault secret URI for ACS connection string (optional)')
param acsConnectionSecretUri string = ''

@description('ACS sender email (optional)')
param acsSenderEmail string = ''

@description('Managed identity ID for Key Vault access')
param managedIdentityId string

@description('Storage account name for blob storage (optional)')
param storageAccountName string = ''

@description('MySQL server FQDN')
param mysqlServerFqdn string

@description('MySQL database name')
param mysqlDatabaseName string

@description('MySQL admin username')
param mysqlAdminUsername string

@description('App Service SKU name')
param skuName string = 'B2'

@description('App Service SKU tier')
param skuTier string = 'Basic'

@description('App Service plan instance count')
@minValue(1)
param capacity int = 1

@description('Tags to apply to resources')
param tags object = {}

// Build connection strings array using KV references
var connectionStrings = concat(
  [
    {
      name: 'ApplicationDbContextConnection'
      connectionString: '@Microsoft.KeyVault(SecretUri=${dbConnectionSecretUri})'
      type: 'Custom'
    }
  ],
  !empty(storageConnectionSecretUri) ? [
    {
      name: 'StorageConnectionString'
      connectionString: '@Microsoft.KeyVault(SecretUri=${storageConnectionSecretUri})'
      type: 'Custom'
    }
  ] : [],
  !empty(acsConnectionSecretUri) ? [
    {
      name: 'AzureCommunicationConnection'
      connectionString: '@Microsoft.KeyVault(SecretUri=${acsConnectionSecretUri})'
      type: 'Custom'
    }
  ] : []
)

// Build app settings array with optional ACS sender
var appSettings = concat(
  [
    {
      name: 'WEBSITES_ENABLE_APP_SERVICE_STORAGE'
      value: 'false'
    }
    {
      name: 'WEBSITES_PORT'
      value: '80'
    }
    {
      name: 'CosmosAllowSetup'
      value: 'true'
    }
    {
      name: 'MultiTenantEditor'
      value: 'false'
    }
    {
      name: 'ASPNETCORE_ENVIRONMENT'
      value: 'Development'
    }
    {
      name: 'BlobServiceProvider'
      value: 'Azure'
    }
    {
      name: 'AzureStorageAccountName'
      value: storageAccountName
    }
    {
      name: 'SKYCMS_DB_HOST'
      value: mysqlServerFqdn
    }
    {
      name: 'SKYCMS_DB_USER'
      value: mysqlAdminUsername
    }
    {
      name: 'SKYCMS_DB_NAME'
      value: mysqlDatabaseName
    }
    {
      name: 'SKYCMS_DB_SSL'
      value: 'true'
    }
    {
      name: 'SKYCMS_DB_SSL_MODE'
      value: 'Required'
    }
  ],
  !empty(acsSenderEmail) ? [
    {
      name: 'AdminEmail'
      value: acsSenderEmail
    }
  ] : []
)

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
