// Azure Blob Storage module for SkyCMS Publisher
// Provides static website hosting with optional CDN

@description('Location for all resources')
param location string = resourceGroup().location

@description('Name of the storage account (must be globally unique, 3-24 chars, lowercase alphanumeric)')
param storageAccountName string

@description('Storage account SKU')
@allowed([
  'Standard_LRS'
  'Standard_GRS'
  'Standard_RAGRS'
  'Standard_ZRS'
  'Premium_LRS'
])
param skuName string = 'Standard_LRS'

@description('Enable static website hosting')
param enableStaticWebsite bool = true

@description('Index document name')
param indexDocument string = 'index.html'

@description('Error document name')
param errorDocument404Path string = '404.html'

@description('Allow blob public access')
param allowBlobPublicAccess bool = true

@description('Minimum TLS version')
@allowed([
  'TLS1_0'
  'TLS1_1'
  'TLS1_2'
])
param minimumTlsVersion string = 'TLS1_2'

@description('Tags to apply to resources')
param tags object = {}

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: storageAccountName
  location: location
  tags: tags
  sku: {
    name: skuName
  }
  kind: 'StorageV2'
  properties: {
    accessTier: 'Hot'
    allowBlobPublicAccess: allowBlobPublicAccess
    minimumTlsVersion: minimumTlsVersion
    supportsHttpsTrafficOnly: true
    encryption: {
      services: {
        blob: {
          enabled: true
        }
        file: {
          enabled: true
        }
      }
      keySource: 'Microsoft.Storage'
    }
  }
}

// Enable static website hosting
resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2023-01-01' = if (enableStaticWebsite) {
  parent: storageAccount
  name: 'default'
  properties: {
    deleteRetentionPolicy: {
      enabled: true
      days: 7
    }
  }
}

// Note: Static website configuration must be done via Azure CLI or PowerShell
// The Bicep deployment will output the commands needed

@description('The name of the storage account')
output storageAccountName string = storageAccount.name

@description('The resource ID of the storage account')
output storageAccountId string = storageAccount.id

@description('The primary blob endpoint')
output primaryBlobEndpoint string = storageAccount.properties.primaryEndpoints.blob

@description('The primary web endpoint (static website)')
output primaryWebEndpoint string = storageAccount.properties.primaryEndpoints.web

@description('Static website URL')
output staticWebsiteUrl string = replace(storageAccount.properties.primaryEndpoints.web, 'https://', '')

@description('Primary connection string for the storage account (for web app configuration)')
#disable-next-line outputs-should-not-contain-secrets
output primaryConnectionString string = 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=${environment().suffixes.storage};'

@description('Azure CLI command to enable static website (run this after deployment)')
output enableStaticWebsiteCommand string = 'az storage blob service-properties update --account-name ${storageAccountName} --static-website --index-document ${indexDocument} --404-document ${errorDocument404Path}'
