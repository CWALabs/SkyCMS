#Requires -Version 7.0

<#
.SYNOPSIS
    Deploys CloudFront test infrastructure for SkyCMS.

.DESCRIPTION
    This script:
    1. Installs npm dependencies
    2. Bootstraps AWS CDK (if needed)
    3. Deploys the CloudFront stack
    4. Configures .NET user secrets automatically

.EXAMPLE
    .\deploy.ps1
#>

[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "SkyCMS CloudFront Test Deployment" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Check prerequisites
Write-Host "Checking prerequisites..." -ForegroundColor Yellow

# Check AWS CLI
try {
    $awsVersion = aws --version 2>$null
    Write-Host "✓ AWS CLI found: $awsVersion" -ForegroundColor Green
} catch {
    Write-Host "✗ AWS CLI not found. Please install: https://aws.amazon.com/cli/" -ForegroundColor Red
    exit 1
}

# Check AWS credentials
try {
    $awsIdentity = aws sts get-caller-identity 2>$null | ConvertFrom-Json
    Write-Host "✓ AWS credentials configured" -ForegroundColor Green
    Write-Host "  Account: $($awsIdentity.Account)" -ForegroundColor Gray
    Write-Host "  User: $($awsIdentity.Arn)" -ForegroundColor Gray
} catch {
    Write-Host "✗ AWS credentials not configured. Run: aws configure" -ForegroundColor Red
    exit 1
}

# Check Node.js
try {
    $nodeVersion = node --version 2>$null
    Write-Host "✓ Node.js found: $nodeVersion" -ForegroundColor Green
} catch {
    Write-Host "✗ Node.js not found. Please install: https://nodejs.org/" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Navigate to script directory
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Push-Location $scriptDir

try {
    # Install dependencies
    Write-Host "Installing dependencies..." -ForegroundColor Yellow
    if (-not (Test-Path "node_modules")) {
        npm install
        if ($LASTEXITCODE -ne 0) {
            throw "npm install failed"
        }
        Write-Host "✓ Dependencies installed" -ForegroundColor Green
    } else {
        Write-Host "✓ Dependencies already installed" -ForegroundColor Green
    }
    Write-Host ""

    # Bootstrap CDK (if needed)
    Write-Host "Checking CDK bootstrap..." -ForegroundColor Yellow
    $region = $env:CDK_DEFAULT_REGION
    if (-not $region) {
        $region = "us-east-1"
    }
    
    # Check if already bootstrapped
    $bootstrapStackName = "CDKToolkit"
    $bootstrapExists = aws cloudformation describe-stacks --stack-name $bootstrapStackName --region $region 2>$null
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ CDK already bootstrapped in $region" -ForegroundColor Green
    } else {
        Write-Host "Bootstrapping CDK in $region..." -ForegroundColor Yellow
        npx cdk bootstrap
        if ($LASTEXITCODE -ne 0) {
            throw "CDK bootstrap failed"
        }
        Write-Host "✓ CDK bootstrapped successfully" -ForegroundColor Green
    }
    Write-Host ""

    # Deploy stack
    Write-Host "Deploying CloudFront stack..." -ForegroundColor Yellow
    Write-Host "This may take 5-10 minutes..." -ForegroundColor Gray
    Write-Host ""
    
    npx cdk deploy --require-approval never --outputs-file outputs.json
    if ($LASTEXITCODE -ne 0) {
        throw "CDK deployment failed"
    }
    
    Write-Host ""
    Write-Host "✓ Stack deployed successfully!" -ForegroundColor Green
    Write-Host ""

    # Parse outputs
    if (Test-Path "outputs.json") {
        $outputs = Get-Content "outputs.json" -Raw | ConvertFrom-Json
        $stackOutputs = $outputs.SkyCmsCloudFrontTest
        
        $distributionId = $stackOutputs.DistributionId
        $accessKeyId = $stackOutputs.AccessKeyId
        $secretAccessKey = $stackOutputs.SecretAccessKey
        
        Write-Host "=====================================" -ForegroundColor Cyan
        Write-Host "Deployment Complete!" -ForegroundColor Cyan
        Write-Host "=====================================" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "Distribution ID: $distributionId" -ForegroundColor White
        Write-Host "Access Key ID: $accessKeyId" -ForegroundColor White
        Write-Host "Secret Key: " -NoNewline -ForegroundColor White
        Write-Host "$($secretAccessKey.Substring(0, 8))..." -ForegroundColor Gray
        Write-Host ""

        # Configure user secrets
        Write-Host "Configuring .NET user secrets..." -ForegroundColor Yellow
        
        $testsDir = Join-Path $scriptDir ".." ".." ".."
        Push-Location $testsDir
        
        try {
            dotnet user-secrets set "AWS:CloudFront:DistributionId" $distributionId
            dotnet user-secrets set "AWS:CloudFront:AccessKeyId" $accessKeyId
            dotnet user-secrets set "AWS:CloudFront:SecretAccessKey" $secretAccessKey
            dotnet user-secrets set "AWS:CloudFront:Region" $region
            
            Write-Host "✓ User secrets configured" -ForegroundColor Green
            Write-Host ""
            
            Write-Host "=====================================" -ForegroundColor Cyan
            Write-Host "Ready to Test!" -ForegroundColor Cyan
            Write-Host "=====================================" -ForegroundColor Cyan
            Write-Host ""
            Write-Host "To run CloudFront tests:" -ForegroundColor White
            Write-Host "  1. Edit Tests\Services\CDN\CloudFrontCdnServiceTests.cs" -ForegroundColor Gray
            Write-Host "  2. Remove the [Ignore] attribute" -ForegroundColor Gray
            Write-Host "  3. Run: " -NoNewline -ForegroundColor Gray
            Write-Host "dotnet test --filter `"TestCategory=CloudFront`"" -ForegroundColor Yellow
            Write-Host ""
            Write-Host "To clean up resources later:" -ForegroundColor White
            Write-Host "  Run: " -NoNewline -ForegroundColor Gray
            Write-Host ".\destroy.ps1" -ForegroundColor Yellow
            Write-Host ""
            
        } finally {
            Pop-Location
        }
        
        # Clean up outputs file
        Remove-Item "outputs.json" -ErrorAction SilentlyContinue
        
    } else {
        Write-Host "⚠ Warning: Could not find outputs.json" -ForegroundColor Yellow
        Write-Host "Please manually configure user secrets from the AWS Console" -ForegroundColor Yellow
    }

} catch {
    Write-Host ""
    Write-Host "✗ Deployment failed: $_" -ForegroundColor Red
    exit 1
} finally {
    Pop-Location
}