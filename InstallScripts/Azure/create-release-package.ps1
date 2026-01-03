$ErrorActionPreference = 'Stop'

# Package Azure Bicep deployment scripts for distribution
$scriptDir = $PSScriptRoot
$version = "1.0.0"  # Update this for each release
$outputName = "skycms-azure-deployment-v$version.zip"
$tempDir = Join-Path $env:TEMP "skycms-azure-package"

Write-Host "Creating SkyCMS Azure Deployment Package v$version" -ForegroundColor Cyan
Write-Host ""

# Clean up any previous temp directory
if (Test-Path $tempDir) {
    Remove-Item -Path $tempDir -Recurse -Force
}

# Create temp directory structure
New-Item -ItemType Directory -Path $tempDir -Force | Out-Null
$packageDir = Join-Path $tempDir "skycms-azure-deployment"
New-Item -ItemType Directory -Path $packageDir -Force | Out-Null

Write-Host "Copying deployment files..." -ForegroundColor Yellow

# Copy PowerShell scripts
Copy-Item -Path (Join-Path $scriptDir "deploy-skycms.ps1") -Destination $packageDir -ErrorAction SilentlyContinue
Copy-Item -Path (Join-Path $scriptDir "destroy-skycms.ps1") -Destination $packageDir -ErrorAction SilentlyContinue
Copy-Item -Path (Join-Path $scriptDir "validate-bicep.ps1") -Destination $packageDir -ErrorAction SilentlyContinue
Copy-Item -Path (Join-Path $scriptDir "helpers.ps1") -Destination $packageDir -ErrorAction SilentlyContinue

# Copy Bicep directory (main template and modules)
$bicepSource = Join-Path $scriptDir "bicep"
$bicepDest = Join-Path $packageDir "bicep"
New-Item -ItemType Directory -Path $bicepDest -Force | Out-Null

# Copy main.bicep
Copy-Item -Path (Join-Path $bicepSource "main.bicep") -Destination $bicepDest -ErrorAction SilentlyContinue

# Copy modules directory
$modulesSource = Join-Path $bicepSource "modules"
if (Test-Path $modulesSource) {
    $modulesDest = Join-Path $bicepDest "modules"
    New-Item -ItemType Directory -Path $modulesDest -Force | Out-Null
    
    # Copy all .bicep files from modules
    Get-ChildItem -Path $modulesSource -Filter "*.bicep" | ForEach-Object {
        Copy-Item -Path $_.FullName -Destination $modulesDest
    }
}

# Copy parameters directory (if exists)
$paramsSource = Join-Path $bicepSource "parameters"
if (Test-Path $paramsSource) {
    Copy-Item -Path $paramsSource -Destination (Join-Path $bicepDest "parameters") -Recurse -Force
}

# Copy README
$readmePath = Join-Path $scriptDir "README.md"
if (Test-Path $readmePath) {
    Copy-Item -Path $readmePath -Destination $packageDir
}

# Copy QUICK_START
$quickStartPath = Join-Path $scriptDir "QUICK_START.md"
if (Test-Path $quickStartPath) {
    Copy-Item -Path $quickStartPath -Destination $packageDir
}

# Create a simple installation guide
$installGuide = @"
# SkyCMS Azure Deployment - Quick Start

## Prerequisites

1. **Azure CLI** - Install from https://aka.ms/installazurecliwindows
   ``````powershell
   az --version
   ``````

2. **PowerShell 5.1+** - Comes with Windows, or install PowerShell 7+
   ``````powershell
   `$PSVersionTable.PSVersion
   ``````

3. **Docker Image** - Your Docker image should be available in Docker Hub or Azure Container Registry
   - Default: toiyabe/sky-editor:latest

4. **Azure Subscription** - Active Azure subscription with appropriate permissions
   ``````powershell
   az login
   az account show
   ``````

## What Gets Deployed

- **Azure App Service** (Premium v3) - Hosts the SkyCMS Editor container
- **Deployment Slot** (Staging) - Zero-downtime deployments with auto-swap
- **Azure SQL Database** - Managed database with TLS encryption
- **Azure Key Vault** - Secure secrets storage with RBAC
- **Managed Identity** - Passwordless authentication between services
- **Blob Storage** (Optional) - Static website hosting for publisher
- **Health Monitoring** - Continuous health checks on /___healthz endpoint

**Estimated Cost:** ~`$60-95/month for development environment

## Installation Steps

### 1. Extract the Package

Extract this ZIP file to a directory of your choice:
``````powershell
# Example: Extract to D:\SkyCMS
Expand-Archive -Path skycms-azure-deployment-v$version.zip -DestinationPath D:\SkyCMS
``````

### 2. Navigate to the Directory

``````powershell
cd D:\SkyCMS\skycms-azure-deployment
``````

### 3. (Optional) Validate Bicep Templates

``````powershell
.\validate-bicep.ps1
``````

### 4. Run the Deployment Script

``````powershell
.\deploy-skycms.ps1
``````

### 5. Follow Interactive Prompts

You'll be asked for:
- Resource group name (e.g., `rg-skycms-dev`)
- Azure region (e.g., `eastus`)
- Base name for resources (3-10 characters, alphanumeric)
- Environment (dev/staging/prod)
- Docker image (default: `toiyabe/sky-editor:latest`)
- Minimum App Service instances
- Whether to deploy publisher (Blob Storage)
- Whether to deploy email (Azure Communication Services)
- Whether to deploy Application Insights

### 6. Wait for Deployment

Deployment takes **10-15 minutes**. You'll see:
- Resource creation progress
- Validation of each component
- Final deployment summary with URLs

## Post-Deployment

### Access the Editor

After deployment completes, you'll see:
``````
✅ SkyCMS Editor URL: https://editor-skycms-dev-abc12345.azurewebsites.net
``````

**The production slot is deployed first** - your editor URL works immediately!

### Complete Setup Wizard

1. **Visit the editor URL** (health check ensures it's ready in 30-60 seconds)
2. **Run the setup wizard** at `https://<your-editor>.azurewebsites.net/___setup`
3. **Configure storage, admin account, and publisher settings**
4. **Start creating content!**

### Understanding Deployment Slots

- **First deployment:** Goes to **production slot** (immediate access)
- **Staging slot:** Created but empty (ready for future updates)
- **Future updates:** Deploy to staging → health check → auto-swap to production
- **Zero downtime:** Users never experience interruptions

## Managing Your Deployment

### View Logs
``````powershell
.\helpers.ps1 -Action ViewLogs -ResourceGroupName "rg-skycms-dev"
``````

### Restart the App
``````powershell
.\helpers.ps1 -Action RestartWebApp -ResourceGroupName "rg-skycms-dev" -WebAppName "editor-skycms-dev-abc12345"
``````

### Scale Instances
``````powershell
.\helpers.ps1 -Action ScaleWebApp -ResourceGroupName "rg-skycms-dev" -Instances 2
``````

### Swap Deployment Slots (Manual)
``````powershell
.\helpers.ps1 -Action SwapSlot -ResourceGroupName "rg-skycms-dev" -WebAppName "editor-skycms-dev-abc12345"
``````

### List All Resources
``````powershell
.\helpers.ps1 -Action ListResources -ResourceGroupName "rg-skycms-dev"
``````

## Teardown / Cleanup

To delete all resources:

``````powershell
.\destroy-skycms.ps1 -ResourceGroupName "rg-skycms-dev"
``````

⚠️ **WARNING:** This deletes ALL data permanently!

## Security Features

- ✅ **TLS Everywhere** - HTTPS only, SQL requires TLS
- ✅ **Key Vault** - All secrets stored securely, referenced via `@Microsoft.KeyVault()` syntax
- ✅ **Managed Identity** - No passwords for service-to-service authentication
- ✅ **RBAC** - Role-based access control for Key Vault
- ✅ **Health Monitoring** - Continuous health checks ensure availability
- ✅ **Soft Delete** - Key Vault secrets recoverable for 7-90 days

## Troubleshooting

| Problem | Solution |
|---------|----------|
| Deployment fails | Check `az account show` to verify login |
| SQL connection fails | Verify firewall rules and Key Vault secrets |
| App not starting | Check health check status: `az webapp show --name <app> --resource-group <rg>` |
| Key Vault access denied | Verify Managed Identity has "Key Vault Secrets Officer" role |

## Additional Resources

- **Full Documentation:** See README.md for detailed architecture and customization
- **Quick Reference:** See QUICK_START.md for common tasks
- **Azure Portal:** Monitor resources at https://portal.azure.com

## Support

For issues, questions, or contributions:
- GitHub Repository: https://github.com/CWALabs/SkyCMS
- Documentation: https://github.com/CWALabs/SkyCMS/tree/main/Docs

---

**Version:** $version  
**Last Updated:** $(Get-Date -Format "MMMM yyyy")
"@

$installGuide | Out-File -FilePath (Join-Path $packageDir "INSTALL.md") -Encoding UTF8

Write-Host "Creating package structure..." -ForegroundColor Yellow
Write-Host "  ✓ PowerShell scripts" -ForegroundColor Green
Write-Host "  ✓ Bicep templates (main + modules)" -ForegroundColor Green
Write-Host "  ✓ Documentation files" -ForegroundColor Green
Write-Host ""

# Create the ZIP file
Write-Host "Creating ZIP archive..." -ForegroundColor Yellow
$outputPath = Join-Path $scriptDir $outputName

if (Test-Path $outputPath) {
    Remove-Item -Path $outputPath -Force
}

Compress-Archive -Path (Join-Path $tempDir "*") -DestinationPath $outputPath -Force

# Clean up temp directory
Remove-Item -Path $tempDir -Recurse -Force

Write-Host ""
Write-Host "✅ Package created successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "📦 Output: $outputPath" -ForegroundColor Cyan
Write-Host "📏 Size: $([math]::Round((Get-Item $outputPath).Length / 1KB, 2)) KB" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Test the package by extracting and running .\deploy-skycms.ps1" -ForegroundColor White
Write-Host "2. Create a GitHub release with tag v$version" -ForegroundColor White
Write-Host "3. Upload $outputName as a release asset" -ForegroundColor White
Write-Host ""
Write-Host "GitHub CLI quick command:" -ForegroundColor Yellow
Write-Host "  gh release create v$version $outputName --title `"SkyCMS Azure Deployment v$version`" --notes `"Standalone Azure deployment package. See INSTALL.md for instructions.`"" -ForegroundColor Gray
Write-Host ""
