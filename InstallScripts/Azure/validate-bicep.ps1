<#
.SYNOPSIS
    Validate Bicep templates for SkyCMS Azure deployment

.DESCRIPTION
    Runs bicep build and what-if analysis to validate templates without deploying.
    Useful for CI/CD pipelines and pre-deployment validation.

.PARAMETER ResourceGroupName
    Name of the resource group to validate against (optional)

.EXAMPLE
    .\validate-bicep.ps1
    
.EXAMPLE
    .\validate-bicep.ps1 -ResourceGroupName "rg-skycms-dev"
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory=$false)]
    [string]$ResourceGroupName
)

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

function Write-Error-Custom {
    param([string]$Text)
    Write-Host "❌ $Text" -ForegroundColor Red
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

# ============================================================================
# PRE-FLIGHT CHECKS
# ============================================================================

Write-Header "Bicep Template Validation"

Write-Info "Checking prerequisites..."

# Check Azure CLI
if (-not (Test-AzureCLI)) {
    Write-Error-Custom "Azure CLI is not installed"
    exit 1
}
Write-Success "Azure CLI is installed"

# Check Bicep CLI
try {
    $bicepVersion = az bicep version 2>&1
    Write-Success "Bicep CLI is installed: $bicepVersion"
} catch {
    Write-Info "Bicep CLI not found, installing..."
    az bicep install
}

# ============================================================================
# VALIDATE BICEP TEMPLATES
# ============================================================================

Write-Header "Building Bicep Templates"

$bicepDir = Join-Path $PSScriptRoot "bicep"
$bicepFiles = @()

# Add main bicep file
$mainBicep = Join-Path $bicepDir "main.bicep"
if (Test-Path $mainBicep) {
    $bicepFiles += $mainBicep
}

# Discover module bicep files dynamically
$modulesDir = Join-Path $bicepDir "modules"
if (Test-Path $modulesDir) {
    $bicepFiles += Get-ChildItem -Path $modulesDir -Filter "*.bicep" -Recurse | ForEach-Object { $_.FullName }
}

if ($bicepFiles.Count -eq 0) {
    Write-Error-Custom "No Bicep files found in $bicepDir"
    exit 1
}

$allValid = $true

foreach ($file in $bicepFiles) {
    if (-not (Test-Path $file)) {
        Write-Error-Custom "File not found: $file"
        $allValid = $false
        continue
    }
    
    $relativePath = $file -replace [regex]::Escape((Join-Path $PSScriptRoot "")), ""
    Write-Info "Validating $relativePath..."
    
    try {
        az bicep build --file $file --stdout | Out-Null
        if ($LASTEXITCODE -eq 0) {
            Write-Success "$relativePath is valid"
        } else {
            Write-Error-Custom "$relativePath has errors"
            $allValid = $false
        }
    } catch {
        Write-Error-Custom "$relativePath validation failed: $_"
        $allValid = $false
    }
}

# ============================================================================
# WHAT-IF ANALYSIS (Optional)
# ============================================================================

if ($ResourceGroupName) {
    Write-Header "What-If Analysis"
    
    Write-Info "Running what-if analysis for resource group: $ResourceGroupName"
    Write-Info "This shows what changes would be made without actually deploying"
    Write-Host ""
    
    $mainBicep = Join-Path $PSScriptRoot "bicep\main.bicep"
    
    # Generate a random password for validation (not used, just for template validation)
    $testPassword = -join ((65..90) + (97..122) + (48..57) | Get-Random -Count 16 | ForEach-Object {[char]$_})
    
    try {
        az deployment group what-if `
            --resource-group $ResourceGroupName `
            --template-file $mainBicep `
            --parameters `
                baseName="skycms" `
                environment="dev" `
                mysqlAdminPassword=$testPassword `
                deployPublisher=$true
        
        if ($LASTEXITCODE -eq 0) {
            Write-Success "What-if analysis completed"
        } else {
            Write-Error-Custom "What-if analysis failed"
            $allValid = $false
        }
    } catch {
        Write-Error-Custom "What-if analysis error: $_"
        $allValid = $false
    }
}

# ============================================================================
# SUMMARY
# ============================================================================

Write-Header "Validation Summary"

if ($allValid) {
    Write-Success "All Bicep templates are valid!"
    Write-Info "Templates are ready for deployment"
    exit 0
} else {
    Write-Error-Custom "Some templates have validation errors"
    Write-Info "Please fix the errors before deploying"
    exit 1
}
