<#
.SYNOPSIS
    Interactive deployment script for SkyCMS on Azure

.DESCRIPTION
    Deploys SkyCMS infrastructure to Azure using Bicep templates.
    Provisions Container Apps, MySQL, Key Vault, and optional Blob Storage.
    
    Similar to AWS CDK deployment but for Azure.

.EXAMPLE
    .\deploy-skycms.ps1
#>

[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

# ============================================================================
# HELPER FUNCTIONS
# ============================================================================

function Write-Header {
    param([string]$Text)
    Write-Host "`n========================================" -ForegroundColor Cyan
    Write-Host " $Text" -ForegroundColor Cyan
    Write-Host "========================================`n" -ForegroundColor Cyan
}

function Write-Success {
    param([string]$Text)
    Write-Host "✅ $Text" -ForegroundColor Green
}

function Write-Info {
    param([string]$Text)
    Write-Host "ℹ️  $Text" -ForegroundColor Blue
}

function Write-Warning-Custom {
    param([string]$Text)
    Write-Host "⚠️  $Text" -ForegroundColor Yellow
}

function Get-UserInput {
    param(
        [string]$Prompt,
        [string]$Default = '',
        [switch]$Required,
        [switch]$Secure
    )
    
    $displayPrompt = if ($Default) { "$Prompt [$Default]" } else { $Prompt }
    
    if ($Secure) {
        $secureValue = Read-Host -Prompt "$displayPrompt" -AsSecureString
        $BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($secureValue)
        $value = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)
    } else {
        $value = Read-Host -Prompt "$displayPrompt"
    }
    
    if ([string]::IsNullOrWhiteSpace($value) -and $Default) {
        return $Default
    }
    
    if ([string]::IsNullOrWhiteSpace($value) -and $Required) {
        Write-Warning-Custom "This field is required."
        return Get-UserInput -Prompt $Prompt -Default $Default -Required:$Required -Secure:$Secure
    }
    
    return $value
}

function Get-YesNoInput {
    param(
        [string]$Prompt,
        [bool]$Default = $true
    )
    
    $defaultText = if ($Default) { 'Y' } else { 'N' }
    $response = Read-Host -Prompt "$Prompt (y/n) [$defaultText]"
    
    if ([string]::IsNullOrWhiteSpace($response)) {
        return $Default
    }
    
    return $response -match '^[Yy]'
}

function Test-AzureCLI {
    try {
        $null = az version 2>&1
        return $true
    } catch {
        Write-Verbose "Azure CLI test failed: $_"
        return $false
    }
}

function Test-AzureLogin {
    try {
        $account = az account show 2>&1 | ConvertFrom-Json
        return $null -ne $account
    } catch {
        Write-Verbose "Azure login test failed: $_"
        return $false
    }
}

function Test-DockerImage {
    <#
    .SYNOPSIS
    Validates Docker image reference format
    
    .DESCRIPTION
    Checks if image is in valid format: [registry/]repo[:tag]
    Examples: ubuntu, myregistry/myapp:v1.0, docker.io/library/node:latest
    #>
    param([string]$Image)
    
    # Valid formats:
    # - repo (e.g., 'ubuntu')
    # - repo:tag (e.g., 'ubuntu:22.04')
    # - registry/repo (e.g., 'docker.io/ubuntu')
    # - registry/repo:tag (e.g., 'docker.io/ubuntu:22.04')
    
    # Regex: (optional registry/)(required repo)(optional :tag)
    $pattern = '^([a-z0-9\-\.]+(\.[a-z0-9]+)?/)?[a-z0-9\-_]+(/[a-z0-9\-_]+)*(:[a-z0-9\-_\.]+)?$'
    return $Image -match $pattern
}

# ============================================================================
# PRE-FLIGHT CHECKS
# ============================================================================

Write-Header "SkyCMS Azure Deployment"

Write-Info "Checking prerequisites..."

# Check Azure CLI
if (-not (Test-AzureCLI)) {
    Write-Host "❌ Azure CLI is not installed." -ForegroundColor Red
    Write-Host "   Download from: https://aka.ms/installazurecliwindows" -ForegroundColor Yellow
    exit 1
}
Write-Success "Azure CLI is installed"

# Check Azure Login
if (-not (Test-AzureLogin)) {
    Write-Warning-Custom "Not logged into Azure. Initiating login..."
    az login
    if (-not (Test-AzureLogin)) {
        Write-Host "❌ Azure login failed." -ForegroundColor Red
        exit 1
    }
}
Write-Success "Logged into Azure"

# Get current subscription
$currentSubscription = az account show | ConvertFrom-Json
Write-Info "Current subscription: $($currentSubscription.name) ($($currentSubscription.id))"

# ============================================================================
# GATHER DEPLOYMENT PARAMETERS
# ============================================================================

Write-Header "Deployment Configuration"

# Resource Group
$resourceGroupName = Get-UserInput -Prompt "Resource Group Name" -Default "rg-skycms-dev" -Required

# Database Configuration (Azure SQL only)
$databaseProvider = 'sql'
Write-Info "Database Provider: Azure SQL"

# Get available regions for SQL deployment
Write-Info "Getting available Azure regions ..."

function Get-AvailableRegionsForDeployment {
    param([string]$DbProvider)
    
    $resourceTypes = @{
        'AppService' = @{ Namespace = 'Microsoft.Web'; ResourceType = 'sites' }
        'Storage' = @{ Namespace = 'Microsoft.Storage'; ResourceType = 'storageAccounts' }
        'KeyVault' = @{ Namespace = 'Microsoft.KeyVault'; ResourceType = 'vaults' }
    }
    
    switch ($DbProvider) {
        'mysql' { $resourceTypes['Database'] = @{ Namespace = 'Microsoft.DBforMySQL'; ResourceType = 'flexibleServers' } }
        'sql' { $resourceTypes['Database'] = @{ Namespace = 'Microsoft.Sql'; ResourceType = 'servers' } }
        'cosmos' { $resourceTypes['Database'] = @{ Namespace = 'Microsoft.DocumentDB'; ResourceType = 'databaseAccounts' } }
    }
    
    $regionsByResource = @{}
    foreach ($key in $resourceTypes.Keys) {
        $resource = $resourceTypes[$key]
        try {
            $query = "resourceTypes[?resourceType=='$($resource.ResourceType)'].locations | `[0`]"
            $locations = az provider show --namespace $resource.Namespace --query $query -o json 2>$null | ConvertFrom-Json
            if ($locations) {
                $normalizedLocations = $locations | ForEach-Object { $_ -replace '\s+', '' | ForEach-Object { $_.ToLower() } }
                $regionsByResource[$key] = $normalizedLocations
            }
        } catch {
            # Ignore errors
        }
    }
    
    # Find intersection
    $commonRegions = $null
    foreach ($key in $regionsByResource.Keys) {
        if ($null -eq $commonRegions) {
            $commonRegions = $regionsByResource[$key]
        } else {
            $commonRegions = $commonRegions | Where-Object { $regionsByResource[$key] -contains $_ }
        }
    }
    
    return $commonRegions | Sort-Object
}

$availableRegions = Get-AvailableRegionsForDeployment -DbProvider 'sql'

if ($availableRegions -and $availableRegions.Count -gt 0) {
    Write-Success "$($availableRegions.Count) regions support all required resources"
    
    # Get user's existing regions
    $userRegions = @()
    try {
        $userRegions = az group list --query '[].location' -o json 2>$null | ConvertFrom-Json | 
            Select-Object -Unique | 
            ForEach-Object { $_ -replace '\s+', '' | ForEach-Object { $_.ToLower() } }
    } catch { }
    
    # Sort: user's regions first, then others
    $recommendedRegions = $availableRegions | Where-Object { $userRegions -contains $_ }
    $otherRegions = $availableRegions | Where-Object { $userRegions -notcontains $_ }
    
    Write-Host "`nAvailable regions:" -ForegroundColor Cyan
    if ($recommendedRegions) {
        Write-Host "  🌟 RECOMMENDED (you have resources here):" -ForegroundColor Yellow
        $recommendedRegions | ForEach-Object { Write-Host "     $_" -ForegroundColor Green }
    }
    if ($otherRegions.Count -le 10) {
        Write-Host "  Other options:" -ForegroundColor DarkGray
        $otherRegions | ForEach-Object { Write-Host "     $_" -ForegroundColor White }
    } else {
        Write-Host "  Other options (showing first 10):" -ForegroundColor DarkGray
        $otherRegions | Select-Object -First 10 | ForEach-Object { Write-Host "     $_" -ForegroundColor White }
        Write-Host "     ... and $($otherRegions.Count - 10) more" -ForegroundColor DarkGray
    }
    
    $defaultRegion = if ($recommendedRegions) { $recommendedRegions[0] } else { $availableRegions[0] }
    
    do {
        $location = Get-UserInput -Prompt "`nAzure Region" -Default $defaultRegion -Required
        $location = $location.ToLower() -replace '\s+', ''
        
        if ($availableRegions -contains $location) {
            Write-Success "Region '$location' validated"
            break
        } else {
            Write-Warning-Custom "Region '$location' does not support all required resources for $databaseProvider"
            Write-Info "Please choose from the available regions listed above"
            $location = $null
        }
    } while ($true)
} else {
    Write-Warning-Custom "Could not validate region availability. Using manual entry."
    Write-Info "Common Azure regions: eastus, westus2, centralus, westeurope, northeurope, southeastasia"
    $location = Get-UserInput -Prompt "Azure Region" -Default "eastus" -Required
}

# Base Configuration
do {
    $baseName = Get-UserInput -Prompt "Base Name - 3 to 10 chars, lowercase alphanumeric only" -Default "skycms" -Required
    if ($baseName -notmatch '^[a-z0-9]{3,10}$') {
        Write-Warning-Custom "Base name must be 3-10 characters, lowercase letters and numbers only (no hyphens)"
        $baseName = $null
    }
} while ([string]::IsNullOrWhiteSpace($baseName))

$environment = Get-UserInput -Prompt "Environment (dev/staging/prod)" -Default "dev" -Required

# Docker Image
$dockerImage = Get-UserInput -Prompt "Docker Image" -Default "toiyabe/sky-editor:latest" -Required

# Validate docker image format
if (-not (Test-DockerImage $dockerImage)) {
    Write-Warning-Custom "Docker image format does not match expected pattern (e.g., 'myregistry/myapp:v1.0')"
    $retryImage = Get-YesNoInput -Prompt "Continue with '$dockerImage' anyway?" -Default $false
    if (-not $retryImage) {
        Write-Host "Deployment cancelled" -ForegroundColor Yellow
        exit 0
    }
}

$mysqlDatabaseName = Get-UserInput -Prompt "Database Name" -Default "skycms" -Required

# Email Option - Ask first before AdminEmail
$deployEmail = Get-YesNoInput -Prompt "Deploy Azure Communication Services for email?" -Default $false

# AdminEmail - auto-generate if ACS deployed, otherwise ask user
if ($deployEmail) {
    Write-Info "Admin email will be auto-generated from Azure Communication Services (DoNotReply@<domain>)"
    $adminEmail = ""  # Will be set by Bicep from ACS
} else {
    $adminEmail = Get-UserInput -Prompt "Administrator Email (for CMS)" -Default "" -Required
}

# Container Configuration
$minReplicas = Get-UserInput -Prompt "Minimum App Service Instances" -Default "1"

# Publisher Option
$deployPublisher = Get-YesNoInput -Prompt "Deploy Publisher (Blob Storage for static website)?" -Default $true

# Application Insights Option
$deployAppInsights = Get-YesNoInput -Prompt "Deploy Application Insights for monitoring?" -Default $false

# ============================================================================
# CONFIRMATION
# ============================================================================

Write-Header "Deployment Summary"

Write-Host "Resource Group:    $resourceGroupName" -ForegroundColor White
Write-Host "Location:          $location" -ForegroundColor White
Write-Host "Base Name:         $baseName" -ForegroundColor White
Write-Host "Environment:       $environment" -ForegroundColor White
Write-Host "Docker Image:      $dockerImage" -ForegroundColor White
Write-Host "Database Provider: Azure SQL (default)" -ForegroundColor White
Write-Host "Database Name:     $mysqlDatabaseName" -ForegroundColor White
Write-Host "Admin Username:    Auto-generated" -ForegroundColor White
Write-Host "Admin Password:    Auto-generated" -ForegroundColor White
Write-Host "Deploy Email (ACS): $deployEmail" -ForegroundColor White
Write-Host "Admin Email:       $(if ($deployEmail) { 'Auto-generated from ACS' } else { $adminEmail })" -ForegroundColor White
Write-Host "Deploy Publisher:  $deployPublisher" -ForegroundColor White
Write-Host "Deploy App Insights: $deployAppInsights" -ForegroundColor White
Write-Host "Min Instances:     $minReplicas" -ForegroundColor White

$confirm = Get-YesNoInput -Prompt "`nProceed with deployment?" -Default $true
if (-not $confirm) {
    Write-Host "Deployment cancelled." -ForegroundColor Yellow
    exit 0
}

# ============================================================================
# CREATE RESOURCE GROUP
# ============================================================================

Write-Header "Creating Resource Group"

$null = az group show --name $resourceGroupName 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Info "Resource group '$resourceGroupName' already exists"
} else {
    Write-Info "Creating resource group '$resourceGroupName' in '$location'..."
    az group create --name $resourceGroupName --location $location | Out-Null
    if ($LASTEXITCODE -eq 0) {
        Write-Success "Resource group created"
    } else {
        Write-Host "❌ Failed to create resource group" -ForegroundColor Red
        exit 1
    }
}

# ============================================================================
# DEPLOY BICEP TEMPLATE
# ============================================================================

Write-Header "Deploying Azure Infrastructure"

$bicepFile = Join-Path $PSScriptRoot "bicep\main.bicep"

if (-not (Test-Path $bicepFile)) {
    Write-Host "❌ Bicep template not found at: $bicepFile" -ForegroundColor Red
    exit 1
}

Write-Info "Starting Bicep deployment (this may take 10-15 minutes)..."
Write-Info "Resources: Container Apps, MySQL Flexible Server, Key Vault$(if ($deployPublisher) {', Blob Storage'})"

$deploymentName = "skycms-deployment-$(Get-Date -Format 'yyyyMMdd-HHmmss')"

# ============================================================================
# CREATE TEMPORARY PARAMETER FILE (avoids exposing password in logs)
# ============================================================================

Write-Info "Preparing deployment parameters..."

$paramFile = New-TemporaryFile
$paramObject = @{
    "schema"         = "https://schema.management.azure.com/schemas/2019-04-01/deploymentParameters.json#"
    "contentVersion" = "1.0.0.0"
    "parameters"     = @{
        "location"                  = @{ "value" = $location }
        "baseName"                  = @{ "value" = $baseName }
        "environment"               = @{ "value" = $environment }
        "deployPublisher"           = @{ "value" = $deployPublisher }
        "deployEmail"               = @{ "value" = $deployEmail }
        "deployAppInsights"         = @{ "value" = $deployAppInsights }
        "dockerImage"               = @{ "value" = $dockerImage }
        "minReplicas"               = @{ "value" = [int]$minReplicas }
        "adminEmail"                = @{ "value" = $adminEmail }
    }
}

$paramObject | ConvertTo-Json -Depth 10 | Set-Content $paramFile

# ============================================================================
# DEPLOY WITH RETRY LOGIC
# ============================================================================$maxRetries = 3
$retryCount = 0
$deployed = $false
$deploymentResult = $null

while (-not $deployed -and $retryCount -lt $maxRetries) {
    try {
        if ($retryCount -gt 0) {
            $delaySeconds = [Math]::Min(30 * [Math]::Pow(2, $retryCount - 1), 300)
            Write-Warning-Custom "Deployment attempt $($retryCount + 1) of $maxRetries. Retrying in $delaySeconds seconds..."
            Start-Sleep -Seconds $delaySeconds
        }
        
        $deploymentResult = az deployment group create `
            --name $deploymentName `
            --resource-group $resourceGroupName `
            --template-file $bicepFile `
            --parameters "@$paramFile" `
            --output json

        if ($LASTEXITCODE -eq 0) {
            $deployed = $true
            Write-Success "Deployment succeeded"
        } else {
            $retryCount++
            if ($retryCount -lt $maxRetries) {
                Write-Warning-Custom "Deployment failed with exit code $LASTEXITCODE, retrying..."
            }
        }
    } catch {
        $retryCount++
        if ($retryCount -lt $maxRetries) {
            Write-Warning-Custom "Deployment error: $_ - Retrying..."
        } else {
            Write-Host "❌ Deployment failed after $maxRetries attempts" -ForegroundColor Red
            Write-Host "Last error: $_" -ForegroundColor Red
        }
    }
}

try {
    # Clean up parameter file (contains sensitive data)
    if (Test-Path $paramFile) {
        Remove-Item $paramFile -Force -ErrorAction SilentlyContinue
        Write-Verbose "Cleaned up temporary parameter file"
    }
} catch {
    Write-Verbose "Failed to clean up parameter file: $_"
}

if (-not $deployed) {
    Write-Host "❌ Deployment failed after $maxRetries attempts" -ForegroundColor Red
    exit 1
}

$deployment = $deploymentResult | ConvertFrom-Json

# ============================================================================
# VERIFY DEPLOYMENT SUCCESS
# ============================================================================

if ($null -eq $deployment.properties) {
    Write-Host "❌ Deployment returned invalid response" -ForegroundColor Red
    Write-Host "Deployment ID: $deploymentName" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "📋 TROUBLESHOOTING:" -ForegroundColor Yellow
    Write-Host "  1. Check Azure Portal for deployment details:" -ForegroundColor White
    Write-Host "     https://portal.azure.com/ > $resourceGroupName > Deployments > $deploymentName" -ForegroundColor Cyan
    Write-Host "  2. Review error messages in deployment logs" -ForegroundColor White
    Write-Host "  3. Check resource constraints and quotas" -ForegroundColor White
    Write-Host ""
    Write-Host "🔧 RECOVERY OPTIONS:" -ForegroundColor Yellow
    Write-Host "  • Fix the issue and re-run this script" -ForegroundColor White
    Write-Host "  • View logs: az deployment group show --name $deploymentName --resource-group $resourceGroupName" -ForegroundColor Cyan
    Write-Host "  • Clean up: .\destroy-skycms.ps1 -ResourceGroupName '$resourceGroupName' -Force" -ForegroundColor Cyan
    exit 1
}

# ============================================================================
# DISPLAY RESULTS
# ============================================================================

Write-Header "Deployment Complete!"

$outputs = $deployment.properties.outputs

Write-Success "SkyCMS infrastructure deployed successfully!"
Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host " 🚀 ACCESS INFORMATION" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "📝 EDITOR APPLICATION:" -ForegroundColor Cyan
Write-Host "   URL:        $($outputs.editorUrl.value)" -ForegroundColor White
Write-Host "   FQDN:       $($outputs.editorFqdn.value)" -ForegroundColor White
Write-Host ""
Write-Host "🗄️  DATABASE:" -ForegroundColor Cyan
Write-Host "   Provider:   Azure SQL Database" -ForegroundColor White
Write-Host ""
if ($deployPublisher) {
    Write-Host "📦 PUBLISHER (Static Website):" -ForegroundColor Cyan
    Write-Host "   Storage:    $($outputs.storageAccountName.value)" -ForegroundColor White
    Write-Host "   URL:        $($outputs.staticWebsiteUrl.value)" -ForegroundColor White
    Write-Host ""
}

Write-Host "========================================" -ForegroundColor Green
Write-Host " 📋 NEXT STEPS" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "1. Wait 1-2 minutes for Container App to fully start" -ForegroundColor Yellow
Write-Host "2. Visit the Editor URL above" -ForegroundColor Yellow
Write-Host "3. Complete the SkyCMS setup wizard" -ForegroundColor Yellow
if ($deployPublisher) {
    Write-Host "4. Upload publisher files to blob storage" -ForegroundColor Yellow
}
Write-Host ""
Write-Info "Deployment logs saved to Azure Portal"
Write-Info "Resource Group: $resourceGroupName"
Write-Host ""
