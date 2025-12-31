// Azure Database for MySQL - Flexible Server module
// Provides managed MySQL 8.0 database with TLS enforcement

@description('Location for all resources')
param location string = resourceGroup().location

@description('Name of the MySQL server')
param serverName string

@description('Administrator username for MySQL')
param administratorLogin string = 'skycms_admin'

@description('Administrator password for MySQL')
@secure()
param administratorPassword string

@description('MySQL version')
@allowed([
  '8.0.21'
  '5.7'
])
param version string = '8.0.21'

@description('The tier of the SKU')
@allowed([
  'Burstable'
  'GeneralPurpose'
  'MemoryOptimized'
])
param skuTier string = 'Burstable'

@description('The name of the SKU')
param skuName string = 'Standard_B1ms'

@description('Storage size in GB')
param storageSizeGB int = 20

@description('Backup retention days')
param backupRetentionDays int = 7

@description('Enable geo-redundant backup')
param geoRedundantBackup string = 'Disabled'

@description('Name of the database to create')
param databaseName string = 'skycms'

@description('Tags to apply to resources')
param tags object = {}

resource mysqlServer 'Microsoft.DBforMySQL/flexibleServers@2023-12-30' = {
  name: serverName
  location: location
  tags: tags
  sku: {
    name: skuName
    tier: skuTier
  }
  properties: {
    administratorLogin: administratorLogin
    administratorLoginPassword: administratorPassword
    version: version
    storage: {
      storageSizeGB: storageSizeGB
      autoGrow: 'Enabled'
      autoIoScaling: 'Enabled'
    }
    backup: {
      backupRetentionDays: backupRetentionDays
      geoRedundantBackup: geoRedundantBackup
    }
    highAvailability: {
      mode: 'Disabled'
    }
    network: {
      publicNetworkAccess: 'Enabled'
    }
  }
}

// Configure SSL enforcement and other server parameters
resource sslEnforcement 'Microsoft.DBforMySQL/flexibleServers/configurations@2023-12-30' = {
  parent: mysqlServer
  name: 'require_secure_transport'
  properties: {
    value: 'ON'
    source: 'user-override'
  }
}

// Allow Azure services to access the server
resource allowAzureServices 'Microsoft.DBforMySQL/flexibleServers/firewallRules@2023-12-30' = {
  parent: mysqlServer
  name: 'AllowAllAzureServicesAndResourcesWithinAzureIps'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

// Temporarily allow all IPs for development (similar to AWS setup)
resource allowAllIps 'Microsoft.DBforMySQL/flexibleServers/firewallRules@2023-12-30' = {
  parent: mysqlServer
  name: 'AllowAllIPs'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '255.255.255.255'
  }
}

// Create the application database
// Note: Using default charset/collation to avoid Azure provisioning issues
resource database 'Microsoft.DBforMySQL/flexibleServers/databases@2023-12-30' = {
  parent: mysqlServer
  name: databaseName
  properties: {
    charset: 'utf8mb3'
    collation: 'utf8mb3_general_ci'
  }
}

@description('The fully qualified domain name of the MySQL server')
output serverFqdn string = mysqlServer.properties.fullyQualifiedDomainName

@description('The name of the MySQL server')
output serverName string = mysqlServer.name

@description('The resource ID of the MySQL server')
output serverId string = mysqlServer.id

@description('The administrator login username')
output administratorLogin string = administratorLogin

@description('The database name')
output databaseName string = database.name

@description('MySQL connection string (without password)')
output connectionStringTemplate string = 'Server=${mysqlServer.properties.fullyQualifiedDomainName};Port=3306;Uid=${administratorLogin};Pwd=<PASSWORD>;Database=${databaseName};SslMode=Required;'
