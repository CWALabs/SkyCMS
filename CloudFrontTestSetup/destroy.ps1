#Requires -Version 7.0

<#
.SYNOPSIS
    Destroys CloudFront test infrastructure for SkyCMS.

.DESCRIPTION
    This script:
    1. Destroys the CloudFront CDK stack
    2. Removes all AWS resources
    3. Optionally clears user secrets

.PARAMETER ClearSecrets
    If specified, also removes AWS CloudFront secrets from .NET user secrets

.EXAMPLE
    .\destroy.ps1
    
.EXAMPLE
    .\destroy.ps1 -ClearSecrets
#>

[CmdletBinding()]
param(
    [switch]$ClearSecrets
)

$ErrorActionPreference = "Stop"

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "SkyCMS CloudFront Test Cleanup" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Confirmation
Write-Host "This will destroy all CloudFront test resources." -ForegroundColor Yellow
Write-Host ""
$confirmation = Read-Host "Are you sure you want to continue? (yes/no)"

if ($confirmation -ne "yes") {
    Write-Host "Cancelled." -ForegroundColor Gray
    exit 0
}

Write-Host ""

# Navigate to script directory
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Push-Location $scriptDir

try {
    # Check if stack exists
    Write-Host "Checking for existing stack..." -ForegroundColor Yellow
    $stackName = "SkyCmsCloudFrontTest"
    $region = $env:CDK_DEFAULT_REGION
    if (-not $region) {
        $region = "us-east-1"
    }
    
    $stackExists = aws cloudformation describe-stacks --stack-name $stackName --region $region 2>$null
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "✓ Stack does not exist or already deleted" -ForegroundColor Green
    } else {
        Write-Host "Found stack. Destroying..." -ForegroundColor Yellow
        Write-Host "This may take 5-10 minutes..." -ForegroundColor Gray
        Write-Host ""
        
        npx cdk destroy --force
        if ($LASTEXITCODE -ne 0) {
            throw "CDK destroy failed"
        }
        
        Write-Host ""
        Write-Host "✓ Stack destroyed successfully!" -ForegroundColor Green
    }
    
    Write-Host ""

    # Clear user secrets if requested
    if ($ClearSecrets) {
        Write-Host "Clearing user secrets..." -ForegroundColor Yellow
        
        $testsDir = Join-Path $scriptDir ".." ".." ".."
        Push-Location $testsDir
        
        try {
            dotnet user-secrets remove "AWS:CloudFront:DistributionId" 2>$null
            dotnet user-secrets remove "AWS:CloudFront:AccessKeyId" 2>$null
            dotnet user-secrets remove "AWS:CloudFront:SecretAccessKey" 2>$null
            dotnet user-secrets remove "AWS:CloudFront:Region" 2>$null
            
            Write-Host "✓ User secrets cleared" -ForegroundColor Green
        } finally {
            Pop-Location
        }
    } else {
        Write-Host "User secrets were not cleared." -ForegroundColor Gray
        Write-Host "To clear them manually, run: " -NoNewline -ForegroundColor Gray
        Write-Host ".\destroy.ps1 -ClearSecrets" -ForegroundColor Yellow
    }
    
    Write-Host ""
    Write-Host "=====================================" -ForegroundColor Cyan
    Write-Host "Cleanup Complete!" -ForegroundColor Cyan
    Write-Host "=====================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "All CloudFront test resources have been removed." -ForegroundColor White
    Write-Host ""

} catch {
    Write-Host ""
    Write-Host "✗ Cleanup failed: $_" -ForegroundColor Red
    exit 1
} finally {
    Pop-Location
}