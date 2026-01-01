// Application Insights module for SkyCMS monitoring and telemetry
// Provides application performance monitoring, logging, and diagnostics

@description('Location for all resources')
param location string = resourceGroup().location

@description('Name of the Application Insights resource')
param appInsightsName string

@description('Name of the Log Analytics Workspace')
param workspaceName string

@description('Tags to apply to resources')
param tags object = {}

// Log Analytics Workspace
resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: workspaceName
  location: location
  tags: tags
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
    features: {
      enableLogAccessUsingOnlyResourcePermissions: true
    }
  }
}

// Application Insights
resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location
  tags: tags
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalyticsWorkspace.id
    IngestionMode: 'LogAnalytics'
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

@description('Application Insights resource name')
output appInsightsName string = appInsights.name

@description('Application Insights resource ID')
output appInsightsId string = appInsights.id

@description('Application Insights instrumentation key')
@secure()
output instrumentationKey string = appInsights.properties.InstrumentationKey

@description('Application Insights connection string')
@secure()
output connectionString string = appInsights.properties.ConnectionString

@description('Log Analytics Workspace ID')
output workspaceId string = logAnalyticsWorkspace.id
