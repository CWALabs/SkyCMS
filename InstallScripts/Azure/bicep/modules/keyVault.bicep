// Azure Key Vault module for SkyCMS secrets management
// Stores database credentials, connection strings, and other sensitive data

@description('Location for all resources')
param location string = resourceGroup().location

@description('Name of the Key Vault')
param keyVaultName string

@description('SKU of the Key Vault')
@allowed([
  'standard'
  'premium'
])
param skuName string = 'standard'

@description('Object ID of the user or service principal that needs access to Key Vault')
param principalId string

@description('Enable purge protection (recommended for production)')
param enablePurgeProtection bool = false

@description('Enable soft delete (recommended)')
param enableSoftDelete bool = true

@description('Soft delete retention in days')
param softDeleteRetentionInDays int = 7

@description('Tags to apply to resources')
param tags object = {}

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: keyVaultName
  location: location
  tags: tags
  properties: {
    sku: {
      family: 'A'
      name: skuName
    }
    tenantId: subscription().tenantId
    enabledForDeployment: false
    enabledForDiskEncryption: false
    enabledForTemplateDeployment: true
    enableSoftDelete: enableSoftDelete
    softDeleteRetentionInDays: softDeleteRetentionInDays
    enablePurgeProtection: enablePurgeProtection ? true : null
    enableRbacAuthorization: true // Use RBAC instead of access policies
    publicNetworkAccess: 'Enabled'
    networkAcls: {
      defaultAction: 'Allow'
      bypass: 'AzureServices'
    }
  }
}

// Grant the principal Key Vault Secrets Officer role
resource secretsOfficerRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(keyVault.id, principalId, 'Key Vault Secrets Officer')
  scope: keyVault
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'b86a8fe4-44ce-4948-aee5-eccb2c155cd7') // Key Vault Secrets Officer
    principalId: principalId
    principalType: 'ServicePrincipal'
  }
}

@description('The name of the Key Vault')
output keyVaultName string = keyVault.name

@description('The resource ID of the Key Vault')
output keyVaultId string = keyVault.id

@description('The URI of the Key Vault')
output keyVaultUri string = keyVault.properties.vaultUri
