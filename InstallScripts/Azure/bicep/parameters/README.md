# Azure Bicep Parameters

This folder contains parameter files for deploying SkyCMS to different environments.

## Parameter Files

### .bicepparam files (Bicep native)
- **dev.bicepparam** - Development environment configuration
- **prod.bicepparam** - Production environment configuration

### Using .bicepparam files

```powershell
# Deploy with bicepparam file (recommended)
az deployment group create \
    --resource-group rg-skycms-dev \
    --template-file ../main.bicep \
    --parameters dev.bicepparam \
    --parameters mysqlAdminPassword='YourSecurePassword123!'
```

### Using JSON parameter files

```json
{
  "$schema": "https://schema.management.azure.com/schemas/2019-04-01/deploymentParameters.json#",
  "contentVersion": "1.0.0.0",
  "parameters": {
    "baseName": { "value": "skycms" },
    "environment": { "value": "dev" },
    "deployPublisher": { "value": true },
    "dockerImage": { "value": "toiyabe/sky-editor:latest" },
    "mysqlDatabaseName": { "value": "skycms" },
    "minReplicas": { "value": 0 },
    "maxReplicas": { "value": 3 }
  }
}
```

Then deploy:
```powershell
az deployment group create \
    --resource-group rg-skycms-dev \
    --template-file ../main.bicep \
    --parameters @dev.parameters.json \
    --parameters mysqlAdminPassword='YourSecurePassword123!'
```

## Important Notes

**Never commit passwords or secrets to parameter files!**
- Always pass sensitive parameters via command line
- Use Azure Key Vault references for production deployments
- The `.gitignore` file excludes `*-local.bicepparam` and `*-secrets.bicepparam` for local testing

## Environment-Specific Settings

### Development
- `minReplicas: 0` - Scale to zero to save costs
- `containerCpu: '0.5'` - Smaller CPU allocation
- `backupRetentionDays: 7` - Shorter backup retention

### Production
- `minReplicas: 2` - High availability with multiple replicas
- `containerCpu: '1.0'` - More resources
- `backupRetentionDays: 30` - Longer backup retention
- Use specific image tags (not `latest`)
