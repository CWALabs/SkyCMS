using './main.bicep'

// Production environment parameters
param baseName = 'skycms'
param environment = 'prod'
param deployPublisher = true
param dockerImage = 'toiyabe/sky-editor:v1.0.0'  // Use specific version tag for prod
// param mysqlAdminPassword = ''  // Must be provided at deployment time via command line
param mysqlDatabaseName = 'skycms'
param minReplicas = 2  // Always have at least 2 replicas for HA
param maxReplicas = 10
param containerCpu = '1.0'
param containerMemory = '2Gi'
