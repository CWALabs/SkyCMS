// Azure Communication Services module for SkyCMS email delivery
// Creates ACS resource with Azure-managed email domain

@description('Location for all resources')
param location string = resourceGroup().location

@description('Name of the Communication Services resource')
param communicationServiceName string

@description('Tags to apply to resources')
param tags object = {}

// Communication Services resource
resource communicationService 'Microsoft.Communication/communicationServices@2023-04-01' = {
  name: communicationServiceName
  location: 'global' // ACS is a global service
  tags: tags
  properties: {
    dataLocation: location == 'eastus' || location == 'eastus2' ? 'UnitedStates' : location == 'westeurope' || location == 'northeurope' ? 'Europe' : location == 'australiaeast' || location == 'southeastasia' ? 'AsiaPacific' : 'UnitedStates'
  }
}

// Email Services resource
resource emailService 'Microsoft.Communication/emailServices@2023-04-01' = {
  name: '${communicationServiceName}-email'
  location: 'global'
  tags: tags
  properties: {
    dataLocation: location == 'eastus' || location == 'eastus2' ? 'UnitedStates' : location == 'westeurope' || location == 'northeurope' ? 'Europe' : location == 'australiaeast' || location == 'southeastasia' ? 'AsiaPacific' : 'UnitedStates'
  }
}

// Azure-managed email domain
resource emailDomain 'Microsoft.Communication/emailServices/domains@2023-04-01' = {
  parent: emailService
  name: 'AzureManagedDomain'
  location: 'global'
  tags: tags
  properties: {
    domainManagement: 'AzureManaged'
    userEngagementTracking: 'Disabled'
  }
}

@description('The name of the Communication Services resource')
output communicationServiceName string = communicationService.name

@description('The resource ID of the Communication Services')
output communicationServiceId string = communicationService.id

@description('The endpoint for the Communication Services')
output endpoint string = communicationService.properties.hostName

@description('Azure Communication Services connection string')
#disable-next-line outputs-should-not-contain-secrets
output connectionString string = 'endpoint=https://${communicationService.properties.hostName}/;accesskey=${communicationService.listKeys().primaryKey}'

@description('Email domain FQDN (e.g., xxxxxxxx.azurecomm.net)')
output emailDomainName string = emailDomain.properties.mailFromSenderDomain

@description('Sender email address (DoNotReply@domain)')
output senderEmailAddress string = 'DoNotReply@${emailDomain.properties.mailFromSenderDomain}'

