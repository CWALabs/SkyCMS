// Azure SQL Database (serverless) for SkyCMS

@description('Location for all resources')
param location string = resourceGroup().location

@description('SQL server name')
param serverName string

@description('Admin username')
param administratorLogin string = 'skycmsadmin'

@description('Admin password')
@secure()
param administratorPassword string

@description('Database name')
param databaseName string = 'skycms'

@description('Tags to apply to resources')
param tags object = {}

resource sqlServer 'Microsoft.Sql/servers@2023-08-01-preview' = {
  name: serverName
  location: location
  tags: tags
  properties: {
    administratorLogin: administratorLogin
    administratorLoginPassword: administratorPassword
    publicNetworkAccess: 'Enabled'
    minimalTlsVersion: '1.2'
  }
}

// Serverless General Purpose smallest SKU
resource sqlDatabase 'Microsoft.Sql/servers/databases@2023-08-01-preview' = {
  name: databaseName
  parent: sqlServer
  location: location
  sku: {
    name: 'GP_S_Gen5_1'
    tier: 'GeneralPurpose'
    family: 'Gen5'
    capacity: 1
  }
  properties: {
    autoPauseDelay: 60 // minutes
    minCapacity: json('0.5')
    maxSizeBytes: 268435456000 // 250 GB cap
    readScale: 'Disabled'
    zoneRedundant: false
  }
}

// Allow Azure services
resource allowAzure 'Microsoft.Sql/servers/firewallRules@2023-08-01-preview' = {
  parent: sqlServer
  name: 'AllowAllWindowsAzureIps'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

@description('SQL server FQDN')
output sqlServerFqdn string = sqlServer.properties.fullyQualifiedDomainName

@description('SQL admin username')
output administratorLogin string = administratorLogin

@description('SQL connection string (no password)')
output connectionStringTemplate string = 'Server=${sqlServer.properties.fullyQualifiedDomainName};Database=${databaseName};User ID=${administratorLogin};Password=<PASSWORD>;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'
