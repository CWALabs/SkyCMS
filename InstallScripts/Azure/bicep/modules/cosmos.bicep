// Cosmos DB SQL API serverless (lowest-cost) for SkyCMS

@description('Location for all resources')
param location string = resourceGroup().location

@description('Cosmos DB account name')
param accountName string

@description('Database name')
param databaseName string = 'skycms'

@description('Tags to apply to resources')
param tags object = {}

resource cosmosAccount 'Microsoft.DocumentDB/databaseAccounts@2023-11-15' = {
  name: accountName
  location: location
  tags: tags
  kind: 'GlobalDocumentDB'
  properties: {
    databaseAccountOfferType: 'Standard'
    enableFreeTier: false
    enableAutomaticFailover: false
    enableAnalyticalStorage: false
    consistencyPolicy: {
      defaultConsistencyLevel: 'Session'
    }
    locations: [
      {
        locationName: location
        failoverPriority: 0
        isZoneRedundant: false
      }
    ]
    capabilities: [
      {
        name: 'EnableServerless'
      }
    ]
    publicNetworkAccess: 'Enabled'
  }
}

resource sqlDb 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2023-11-15' = {
  name: databaseName
  parent: cosmosAccount
  properties: {
    resource: {
      id: databaseName
    }
  }
}

@description('Cosmos account name')
output account string = cosmosAccount.name

@description('Cosmos database name')
output database string = databaseName
