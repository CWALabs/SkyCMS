using './main.bicep'

// Development environment parameters
param baseName = 'skycms'
param environment = 'dev'
param deployPublisher = true
param dockerImage = 'toiyabe/sky-editor:latest'
// param mysqlAdminPassword = ''  // Must be provided at deployment time via command line
param mysqlDatabaseName = 'skycms'
param minReplicas = 0  // Scale to zero for cost savings
param maxReplicas = 3
param containerCpu = '0.5'
param containerMemory = '1Gi'
