<#
.SYNOPSIS
    Teardown script for SkyCMS Azure infrastructure

.DESCRIPTION
    Deletes the SkyCMS resource group and all associated Azure resources.
    WARNING: This will delete ALL data including databases and storage.

.PARAMETER ResourceGroupName
    Name of the resource group to delete (optional, will prompt if not provided)

.PARAMETER Force
    Skip confirmation prompts

.EXAMPLE
    .\destroy-skycms.ps1
    
.EXAMPLE
    .\destroy-skycms.ps1 -ResourceGroupName "rg-skycms-dev" -Force
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory=$false)]
    [string]$ResourceGroupName,
    
    [Parameter(Mandatory=$false)]
    [switch]$Force
)

$ErrorActionPreference = 'Stop'

# ============================================================================
# HELPER FUNCTIONS
# ============================================================================

function Write-Header {
    param([string]$Text)
    Write-Host "`n========================================" -ForegroundColor Red
    Write-Host " $Text" -ForegroundColor Red
    Write-Host "========================================`n" -ForegroundColor Red
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

function Get-YesNoInput {
    param(
        [string]$Prompt,
        [bool]$Default = $false
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
        return $false
    }
}

function Test-AzureLogin {
    try {
        $account = az account show 2>&1 | ConvertFrom-Json
        return $null -ne $account
    } catch {
        return $false
    }
}

# ============================================================================
# PRE-FLIGHT CHECKS
# ============================================================================

Write-Header "SkyCMS Azure Teardown"

Write-Warning-Custom "This will DELETE ALL SkyCMS resources in the specified resource group!"
Write-Warning-Custom "This action CANNOT be undone. All data will be permanently lost."
Write-Host ""

# Check Azure CLI
if (-not (Test-AzureCLI)) {
    Write-Host "❌ Azure CLI is not installed." -ForegroundColor Red
    exit 1
}

# Check Azure Login
if (-not (Test-AzureLogin)) {
    Write-Warning-Custom "Not logged into Azure. Initiating login..."
    az login
    if (-not (Test-AzureLogin)) {
        Write-Host "❌ Azure login failed." -ForegroundColor Red
        exit 1
    }
}

# Get current subscription
$currentSubscription = az account show | ConvertFrom-Json
Write-Info "Current subscription: $($currentSubscription.name)"

# ============================================================================
# GET RESOURCE GROUP
# ============================================================================

if ([string]::IsNullOrWhiteSpace($ResourceGroupName)) {
    # List available resource groups
    Write-Host "`nAvailable Resource Groups:" -ForegroundColor Cyan
    $rgs = az group list --query "[].{Name:name, Location:location}" --output json | ConvertFrom-Json
    $rgs | ForEach-Object { Write-Host "  - $($_.Name) ($($_.Location))" -ForegroundColor White }
    Write-Host ""
    
    $ResourceGroupName = Read-Host "Enter Resource Group Name to delete"
}

if ([string]::IsNullOrWhiteSpace($ResourceGroupName)) {
    Write-Host "❌ Resource group name is required" -ForegroundColor Red
    exit 1
}

# ============================================================================
# VERIFY RESOURCE GROUP EXISTS
# ============================================================================

Write-Info "Checking resource group '$ResourceGroupName'..."

$rgExists = az group exists --name $ResourceGroupName
if ($rgExists -eq 'false') {
    Write-Host "❌ Resource group '$ResourceGroupName' does not exist" -ForegroundColor Red
    exit 1
}

Write-Success "Found resource group '$ResourceGroupName'"

# ============================================================================
# SHOW RESOURCES TO BE DELETED
# ============================================================================

Write-Host "`nResources in '$ResourceGroupName':" -ForegroundColor Yellow
$resources = az resource list --resource-group $ResourceGroupName --query "[].{Name:name, Type:type}" --output json | ConvertFrom-Json

if ($resources.Count -eq 0) {
    Write-Info "No resources found in this resource group"
} else {
    $resources | ForEach-Object {
        Write-Host "  🗑️  $($_.Name) ($($_.Type))" -ForegroundColor White
    }
}

Write-Host ""
Write-Host "Total resources: $($resources.Count)" -ForegroundColor Yellow
Write-Host ""

# ============================================================================
# CONFIRMATION
# ============================================================================

if (-not $Force) {
    Write-Host "⚠️  WARNING: This will permanently delete:" -ForegroundColor Red
    Write-Host "   • All Container Apps and Container App Environments" -ForegroundColor Red
    Write-Host "   • MySQL database server and ALL databases" -ForegroundColor Red
    Write-Host "   • Key Vault and ALL secrets" -ForegroundColor Red
    Write-Host "   • Storage accounts and ALL blobs/files" -ForegroundColor Red
    Write-Host "   • Managed identities and role assignments" -ForegroundColor Red
    Write-Host "   • All other resources in the resource group" -ForegroundColor Red
    Write-Host ""
    
    $confirmText = Read-Host "Type the resource group name '$ResourceGroupName' to confirm deletion"
    
    if ($confirmText -ne $ResourceGroupName) {
        Write-Host "❌ Resource group name does not match. Deletion cancelled." -ForegroundColor Yellow
        exit 0
    }
    
    $finalConfirm = Get-YesNoInput -Prompt "Are you absolutely sure you want to delete '$ResourceGroupName'?" -Default $false
    
    if (-not $finalConfirm) {
        Write-Host "Deletion cancelled." -ForegroundColor Yellow
        exit 0
    }
}

# ============================================================================
# DELETE RESOURCE GROUP
# ============================================================================

Write-Header "Deleting Resources"

Write-Info "Deleting resource group '$ResourceGroupName'..."
Write-Info "This may take 5-10 minutes..."

az group delete --name $ResourceGroupName --yes --no-wait

if ($LASTEXITCODE -eq 0) {
    Write-Success "Resource group deletion initiated"
    Write-Info "Deletion is running in the background"
    Write-Info "Monitor progress in Azure Portal or run:"
    Write-Host "   az group show --name $ResourceGroupName" -ForegroundColor Cyan
} else {
    Write-Host "❌ Failed to delete resource group" -ForegroundColor Red
    exit 1
}

# ============================================================================
# COMPLETION
# ============================================================================

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host " 🗑️  TEARDOWN INITIATED" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Success "Resource group '$ResourceGroupName' is being deleted"
Write-Info "All SkyCMS resources will be removed shortly"
Write-Host ""
Write-Info "You can check deletion status with:"
Write-Host "   az group list --query \"[?name=='$ResourceGroupName']\"" -ForegroundColor Cyan
Write-Host ""
