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

@description('Administrator email address for the CMS')
param adminEmail string = ''

// Docker Image Configuration
@description('Docker image for the SkyCMS Editor')
param dockerImage string = 'toiyabe/sky-editor:latest'

// Database Credentials
@description('Database administrator password (used for MySQL or Azure SQL). Leave blank to auto-generate a strong password.')
@secure()
param databaseAdminPassword string = ''

@description('Password seed used when auto-generating credentials (auto-filled).')
@secure()
param passwordSeed string = newGuid()

@description('Random seed to ensure unique resource names per deployment run')
param randomSeed string = newGuid()

@description('Minimum worker instances for App Service Plan')
@minValue(1)
@maxValue(10)
param minReplicas int = 1

// Optional Email (Azure Communication Services)
@description('Azure Communication Services Email connection string (optional)')
@secure()
param acsConnectionString string = ''

@description('ACS sender email address (optional)')
param acsSenderEmail string = ''

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

// Always use Azure SQL
var databaseProvider = 'sql'

var uniqueSuffix = uniqueString(resourceGroup().id, baseName)
var randomSuffix = toLower(uniqueString(resourceGroup().id, baseName, randomSeed))
var randomSuffix8 = substring(randomSuffix, 0, 8)

var keyVaultName = 'kv-${baseName}-${uniqueSuffix}'
var mysqlServerName = 'mysql-${baseName}-${randomSuffix8}'
var sqlServerName = 'sql-${baseName}-${randomSuffix8}'
var cosmosAccountName = 'cosmos-${baseName}-${randomSuffix8}'
var appServicePlanName = 'plan-${baseName}-${environment}-${randomSuffix8}'
var webAppName = 'editor-${baseName}-${environment}-${randomSuffix8}'
var managedIdentityName = 'id-${baseName}-${environment}-${randomSuffix8}'
var storageAccountName = 'st${substring(baseName, 0, min(10, length(baseName)))}${substring(randomSuffix, 0, 10)}'
var dbConnectionSecretName = 'db-connection-string'
var storageConnectionSecretName = 'storage-connection-string'
var acsConnectionSecretName = 'acs-connection-string'
var databaseName = 'skycms'
var mysqlServerFqdn = '${mysqlServerName}.mysql.database.azure.com'
var sqlServerHostnameSuffix = az.environment().suffixes.sqlServerHostname
var sqlServerFqdn = startsWith(sqlServerHostnameSuffix, '.')
  ? '${sqlServerName}${sqlServerHostnameSuffix}'
  : '${sqlServerName}.${sqlServerHostnameSuffix}'
var keyVaultDnsSuffix = az.environment().suffixes.keyvaultDns
var keyVaultBaseUri = startsWith(keyVaultDnsSuffix, '.')
  ? 'https://${keyVaultName}${keyVaultDnsSuffix}'
  : 'https://${keyVaultName}.${keyVaultDnsSuffix}'
var dbConnectionSecretUri = '${keyVaultBaseUri}/secrets/${dbConnectionSecretName}'
var storageConnectionSecretUriValue = deployPublisher ? '${keyVaultBaseUri}/secrets/${storageConnectionSecretName}' : ''
var acsConnectionSecretUriValue = !empty(acsConnectionString) ? '${keyVaultBaseUri}/secrets/${acsConnectionSecretName}' : ''
var generatedAdminUsername = 'admin${substring(uniqueSuffix, 0, 6)}'
var generatedAdminPassword = '${toUpper(substring(passwordSeed, 0, 8))}!${substring(passwordSeed, 9, 4)}${substring(passwordSeed, 14, 4)}${substring(passwordSeed, 19, 4)}'
var adminUsernameFinal = generatedAdminUsername
var adminPasswordFinal = empty(databaseAdminPassword) ? generatedAdminPassword : databaseAdminPassword

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

module mysql 'modules/mysql.bicep' = if (databaseProvider == 'mysql') {
  name: 'mysql-deployment'
  params: {
    location: location
    serverName: mysqlServerName
    administratorLogin: adminUsernameFinal
    administratorPassword: adminPasswordFinal
    databaseName: databaseName
    skuName: environment == 'prod' ? 'Standard_B2s' : 'Standard_B1ms'
    skuTier: 'Burstable'
    storageSizeGB: 20
    backupRetentionDays: environment == 'prod' ? 30 : 7
    tags: tags
  }
}

module sql 'modules/sqlDatabase.bicep' = if (databaseProvider == 'sql') {
  name: 'sql-deployment'
  params: {
    location: location
    serverName: sqlServerName
    administratorLogin: adminUsernameFinal
    administratorPassword: adminPasswordFinal
    databaseName: databaseName
    tags: tags
  }
}

module cosmos 'modules/cosmos.bicep' = if (databaseProvider == 'cosmos') {
  name: 'cosmos-deployment'
  params: {
    location: location
    accountName: cosmosAccountName
    databaseName: databaseName
    tags: tags
  }
}

// ============================================================================
// STORE DB CONNECTION STRING IN KEY VAULT
// ============================================================================

var mysqlConnectionString = databaseProvider == 'mysql' ? 'Server=${mysqlServerFqdn};Port=3306;Uid=${adminUsernameFinal};Pwd=${adminPasswordFinal};Database=${databaseName};SslMode=Required;' : ''
var sqlConnectionString = databaseProvider == 'sql' ? 'Server=${sqlServerFqdn};Database=${databaseName};User ID=${adminUsernameFinal};Password=${adminPasswordFinal};Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;' : ''
var cosmosConnectionString = databaseProvider == 'cosmos' ? listConnectionStrings(resourceId('Microsoft.DocumentDB/databaseAccounts', cosmosAccountName), '2023-11-15').connectionStrings[0].connectionString : ''

var applicationConnectionString = !empty(mysqlConnectionString) ? mysqlConnectionString : !empty(sqlConnectionString) ? sqlConnectionString : cosmosConnectionString

resource dbConnectionSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  name: '${keyVaultName}/${dbConnectionSecretName}'
  properties: {
    value: applicationConnectionString
  }
  dependsOn: [
    keyVault
    sql
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

// Storage connection string secret (if publisher deployed)
resource storageConnectionSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = if (deployPublisher) {
  name: '${keyVaultName}/${storageConnectionSecretName}'
  properties: {
    value: storage.outputs.primaryConnectionString
  }
  dependsOn: [
    keyVault
    storage
  ]
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

// Optional ACS connection string secret
resource acsConnectionSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = if (!empty(acsConnectionString)) {
  name: '${keyVaultName}/${acsConnectionSecretName}'
  properties: {
    value: acsConnectionString
  }
  dependsOn: [
    keyVault
  ]
}

// ============================================================================
// CONTAINER APP (EDITOR)
// ============================================================================

module webApp 'modules/webApp.bicep' = {
  name: 'webApp-deployment'
  params: {
    location: location
    webAppName: webAppName
    planName: appServicePlanName
    imageName: dockerImage
    dbConnectionString: applicationConnectionString
    storageConnectionString: deployPublisher ? storage.outputs.primaryConnectionString : ''
    adminEmail: adminEmail
    publisherUrl: deployPublisher ? storage.outputs.primaryWebEndpoint : ''
    managedIdentityId: managedIdentity.id
    skuName: environment == 'prod' ? 'P2v3' : 'P1v3'
    skuTier: 'PremiumV3'
    capacity: max(1, minReplicas)
    tags: tags
  }
  dependsOn: [
    storageBlobDataContributorRole
  ]
}

// ============================================================================
// OUTPUTS
// ============================================================================

@description('SkyCMS Editor URL (wait 1-2 min for deployment)')
output editorUrl string = webApp.outputs.url

@description('Web App hostname')
output editorFqdn string = webApp.outputs.hostName

@description('MySQL Server FQDN')
output mysqlServerFqdn string = 'Not deployed (using Azure SQL instead)'

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

@description('Next Steps')
output nextSteps string = '''
1. Wait 1-2 minutes for Web App to start
2. Visit the Editor URL above
3. Complete the SkyCMS setup wizard
4. ${deployPublisher ? 'Enable static website: Run the command from storage outputs' : 'Publisher not deployed'}
'''

@description('Human-friendly deployment summary')
output deploymentSummary string = '''Deployment succeeded. Here are the outputs:

Editor URL: ${webApp.outputs.url}
Web app hostname: ${webApp.outputs.hostName}
MySQL server: ${databaseProvider == 'mysql' ? mysqlServerFqdn : 'Not deployed'}
Key Vault: ${keyVault.outputs.keyVaultName}
Storage account: ${deployPublisher ? storageAccountName : 'Not deployed'}
Static website URL: ${deployPublisher ? format('https://{0}.z13.web.{1}', storageAccountName, az.environment().suffixes.storage) : 'Not deployed'}
Managed identity: ${managedIdentity.name}

Next steps:
1) Wait ~1–2 minutes for the web app to warm up.
2) Browse the editor URL above and complete the SkyCMS setup wizard.
'''
