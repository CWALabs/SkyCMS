// SkyCMS Azure Infrastructure - Main Bicep Template
// Orchestrates deployment of Editor (Container Apps + MySQL) and optional Publisher (Blob Storage)

targetScope = 'resourceGroup'

// ============================================================================
// PARAMETERS
// ============================================================================

@description('Location for all resources')
param location string = resourceGroup().location

@description('Base name for resources (used to generate unique names)')
@minLength(3)
@maxLength(10)
param baseName string = 'skycms'

@description('Environment name (dev, staging, prod)')
@allowed([
  'dev'
  'staging'
  'prod'
])
param environment string = 'dev'

@description('Deploy publisher (Blob Storage for static website)')
param deployPublisher bool = true

// Docker Image Configuration
@description('Docker image for the SkyCMS Editor')
param dockerImage string = 'toiyabe/sky-editor:latest'

// MySQL Configuration
@description('MySQL administrator password')
@secure()
param mysqlAdminPassword string

@description('MySQL database name')
param mysqlDatabaseName string = 'skycms'

// Container Apps Configuration
@description('Minimum number of container replicas')
@minValue(0)
@maxValue(10)
param minReplicas int = 1

@description('Maximum number of container replicas')
@minValue(1)
@maxValue(30)
param maxReplicas int = 3

@description('Container CPU allocation')
param containerCpu string = '0.5'

@description('Container memory allocation')
param containerMemory string = '1Gi'

// Tags
@description('Tags to apply to all resources')
param tags object = {
  Application: 'SkyCMS'
  Environment: environment
  ManagedBy: 'Bicep'
}

// ============================================================================
// VARIABLES
// ============================================================================

var uniqueSuffix = uniqueString(resourceGroup().id, baseName)
var keyVaultName = 'kv-${baseName}-${uniqueSuffix}'
var mysqlServerName = 'mysql-${baseName}-${uniqueSuffix}'
var containerAppName = 'ca-${baseName}-editor-${environment}'
var containerAppEnvName = 'cae-${baseName}-${environment}'
var managedIdentityName = 'id-${baseName}-${environment}'
var storageAccountName = 'st${baseName}${uniqueSuffix}'
var dbConnectionSecretName = 'db-connection-string'

// ============================================================================
// MANAGED IDENTITY
// ============================================================================

resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: managedIdentityName
  location: location
  tags: tags
}

// ============================================================================
// KEY VAULT
// ============================================================================

module keyVault 'modules/keyVault.bicep' = {
  name: 'keyVault-deployment'
  params: {
    location: location
    keyVaultName: keyVaultName
    principalId: managedIdentity.properties.principalId
    enablePurgeProtection: environment == 'prod'
    enableSoftDelete: true
    softDeleteRetentionInDays: 7
    tags: tags
  }
}

// ============================================================================
// MYSQL FLEXIBLE SERVER
// ============================================================================

module mysql 'modules/mysql.bicep' = {
  name: 'mysql-deployment'
  params: {
    location: location
    serverName: mysqlServerName
    administratorPassword: mysqlAdminPassword
    databaseName: mysqlDatabaseName
    skuName: environment == 'prod' ? 'Standard_B2s' : 'Standard_B1ms'
    skuTier: 'Burstable'
    storageSizeGB: 20
    backupRetentionDays: environment == 'prod' ? 30 : 7
    tags: tags
  }
}

// ============================================================================
// STORE DB CONNECTION STRING IN KEY VAULT
// ============================================================================

resource dbConnectionSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  name: '${keyVaultName}/${dbConnectionSecretName}'
  properties: {
    value: 'Server=${mysql.outputs.serverFqdn};Port=3306;Uid=${mysql.outputs.administratorLogin};Pwd=${mysqlAdminPassword};Database=${mysql.outputs.databaseName};SslMode=Required;'
  }
  dependsOn: [
    keyVault
  ]
}

// ============================================================================
// BLOB STORAGE (PUBLISHER) - Optional
// ============================================================================

module storage 'modules/storage.bicep' = if (deployPublisher) {
  name: 'storage-deployment'
  params: {
    location: location
    storageAccountName: storageAccountName
    skuName: 'Standard_LRS'
    enableStaticWebsite: true
    tags: tags
  }
}

// Grant Managed Identity Storage Blob Data Contributor role to storage account
resource storageBlobDataContributorRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (deployPublisher) {
  name: guid(storageAccountName, managedIdentity.id, 'Storage Blob Data Contributor')
  scope: resourceGroup()
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'ba92f5b4-2d11-453d-a403-e96b0029c9fe') // Storage Blob Data Contributor
    principalId: managedIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
  dependsOn: [
    storage
  ]
}

// ============================================================================
// CONTAINER APP (EDITOR)
// ============================================================================

module containerApp 'modules/containerApp.bicep' = {
  name: 'containerApp-deployment'
  params: {
    location: location
    containerAppName: containerAppName
    environmentName: containerAppEnvName
    imageName: dockerImage
    targetPort: 80
    minReplicas: minReplicas
    maxReplicas: maxReplicas
    cpu: containerCpu
    memory: containerMemory
    keyVaultUri: keyVault.outputs.keyVaultUri
    dbConnectionSecretName: dbConnectionSecretName
    managedIdentityId: managedIdentity.id
    external: true
    allowInsecure: false
    storageAccountName: deployPublisher ? storageAccountName : ''
    mysqlServerFqdn: mysql.outputs.serverFqdn
    mysqlDatabaseName: mysql.outputs.databaseName
    mysqlAdminUsername: mysql.outputs.administratorLogin
    tags: tags
  }
  dependsOn: [
    dbConnectionSecret
    storageBlobDataContributorRole
  ]
}

// ============================================================================
// OUTPUTS
// ============================================================================

@description('✅ SkyCMS Editor URL (wait 1-2 min for deployment)')
output editorUrl string = containerApp.outputs.url

@description('Container App FQDN')
output editorFqdn string = containerApp.outputs.fqdn

@description('MySQL Server FQDN')
output mysqlServerFqdn string = mysql.outputs.serverFqdn

@description('MySQL Database Name')
output mysqlDatabaseName string = mysql.outputs.databaseName

@description('MySQL Admin Username')
output mysqlAdminUsername string = mysql.outputs.administratorLogin

@description('Key Vault Name')
output keyVaultName string = keyVault.outputs.keyVaultName

@description('Storage Account Name (if deployed)')
output storageAccountName string = deployPublisher ? storageAccountName : 'Not deployed'

@description('Static Website URL (if deployed)')
output staticWebsiteUrl string = deployPublisher ? 'https://${storageAccountName}.z13.web.${az.environment().suffixes.storage}' : 'Not deployed'

@description('Managed Identity Name')
output managedIdentityName string = managedIdentity.name

@description('Resource Group Name')
output resourceGroupName string = resourceGroup().name

@description('📋 Next Steps')
output nextSteps string = '''
1. Wait 1-2 minutes for Container App to start
2. Visit the Editor URL above
3. Complete the SkyCMS setup wizard
4. ${deployPublisher ? 'Enable static website: Run the command from storage outputs' : 'Publisher not deployed'}
'''
