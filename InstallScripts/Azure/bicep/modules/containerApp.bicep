// Azure Container Apps module for SkyCMS Editor
// Hosts the containerized Editor application with auto-scaling

@description('Location for all resources')
param location string = resourceGroup().location

@description('Name of the Container App')
param containerAppName string

@description('Name of the Container Apps Environment')
param environmentName string

@description('Docker image for the container')
param imageName string = 'toiyabe/sky-editor:latest'

@description('Target port for the container')
param targetPort int = 80

@description('Minimum number of replicas')
param minReplicas int = 1

@description('Maximum number of replicas')
param maxReplicas int = 3

@description('CPU allocation (in cores)')
param cpu string = '0.5'

@description('Memory allocation')
param memory string = '1Gi'

@description('Key Vault URI for secrets')
param keyVaultUri string

@description('Key Vault secret name for DB connection string')
param dbConnectionSecretName string

@description('Managed identity ID for Key Vault access')
param managedIdentityId string

@description('Enable external ingress')
param external bool = true

@description('Allow insecure traffic (HTTP)')
param allowInsecure bool = false

@description('Storage account name for blob storage')
param storageAccountName string = ''

@description('MySQL server FQDN')
param mysqlServerFqdn string

@description('MySQL database name')
param mysqlDatabaseName string

@description('MySQL admin username')
param mysqlAdminUsername string

@description('Tags to apply to resources')
param tags object = {}

// Create Container Apps Environment
resource containerAppEnvironment 'Microsoft.App/managedEnvironments@2023-05-01' = {
  name: environmentName
  location: location
  tags: tags
  properties: {
    zoneRedundant: false
  }
}

// Create Container App
resource containerApp 'Microsoft.App/containerApps@2023-05-01' = {
  name: containerAppName
  location: location
  tags: tags
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentityId}': {}
    }
  }
  properties: {
    managedEnvironmentId: containerAppEnvironment.id
    configuration: {
      ingress: {
        external: external
        targetPort: targetPort
        allowInsecure: allowInsecure
        traffic: [
          {
            latestRevision: true
            weight: 100
          }
        ]
      }
      secrets: [
        {
          name: 'db-connection-string'
          keyVaultUrl: '${keyVaultUri}secrets/${dbConnectionSecretName}'
          identity: managedIdentityId
        }
      ]
      registries: [] // Using Docker Hub public registry
    }
    template: {
      containers: [
        {
          name: 'skycms-editor'
          image: imageName
          resources: {
            cpu: json(cpu)
            memory: memory
          }
          env: [
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
            {
              name: 'ConnectionStrings__ApplicationDbContextConnection'
              secretRef: 'db-connection-string'
            }
            // Note: SMTP configuration can be added here for email functionality
            // Similar to AWS SES configuration in the CDK stack
          ]
        }
      ]
      scale: {
        minReplicas: minReplicas
        maxReplicas: maxReplicas
        rules: [
          {
            name: 'http-scaling'
            http: {
              metadata: {
                concurrentRequests: '10'
              }
            }
          }
        ]
      }
    }
  }
}

@description('The FQDN of the Container App')
output fqdn string = containerApp.properties.configuration.ingress.fqdn

@description('The URL of the Container App')
output url string = 'https://${containerApp.properties.configuration.ingress.fqdn}'

@description('The name of the Container App')
output containerAppName string = containerApp.name

@description('The resource ID of the Container App')
output containerAppId string = containerApp.id

@description('The name of the Container Apps Environment')
output environmentName string = containerAppEnvironment.name
