<![CDATA[#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Build Jekyll documentation and deploy to CloudFlare R2 bucket

.DESCRIPTION
    This script:
    1. Builds the markdown documentation in ./Docs to HTML using Jekyll
    2. Uploads the generated site to a CloudFlare R2 bucket
    3. Optionally purges the CloudFlare cache

.PARAMETER BucketName
    The name of your CloudFlare R2 bucket

.PARAMETER AccountId
    Your CloudFlare account ID

.PARAMETER AccessKeyId
    CloudFlare R2 Access Key ID

.PARAMETER SecretAccessKey
    CloudFlare R2 Secret Access Key

.PARAMETER PurgeCache
    If set, will purge the CloudFlare cache after deployment

.PARAMETER ZoneId
    CloudFlare Zone ID (required if PurgeCache is set)

.PARAMETER ApiToken
    CloudFlare API Token (required if PurgeCache is set)

.EXAMPLE
    .\deploy-docs-to-cloudflare.ps1 -BucketName "skycms-docs" -AccountId "your-account-id"

.EXAMPLE
    .\deploy-docs-to-cloudflare.ps1 -BucketName "skycms-docs" -AccountId "your-account-id" -PurgeCache -ZoneId "zone-id" -ApiToken "token"
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$BucketName,

    [Parameter(Mandatory = $true)]
    [string]$AccountId,

    [Parameter(Mandatory = $false)]
    [string]$AccessKeyId = $env:CLOUDFLARE_R2_ACCESS_KEY_ID,

    [Parameter(Mandatory = $false)]
    [string]$SecretAccessKey = $env:CLOUDFLARE_R2_SECRET_ACCESS_KEY,

    [Parameter(Mandatory = $false)]
    [switch]$PurgeCache,

    [Parameter(Mandatory = $false)]
    [string]$ZoneId = $env:CLOUDFLARE_ZONE_ID,

    [Parameter(Mandatory = $false)]
    [string]$ApiToken = $env:CLOUDFLARE_API_TOKEN,

    [Parameter(Mandatory = $false)]
    [switch]$SkipBuild
)

$ErrorActionPreference = "Stop"

# Define paths
$ScriptDir = $PSScriptRoot
$ProjectRoot = Split-Path $ScriptDir -Parent
$DocsDir = Join-Path $ProjectRoot "Docs"
$SiteDir = Join-Path $DocsDir "_site"

# CloudFlare R2 S3-compatible endpoint
$R2Endpoint = "https://$AccountId.r2.cloudflarestorage.com"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "SkyCMS Documentation Deployment" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Validate prerequisites
function Test-Prerequisites {
    Write-Host "Checking prerequisites..." -ForegroundColor Yellow
    
    # Check if Jekyll is installed
    if (-not $SkipBuild) {
        try {
            $jekyllVersion = jekyll --version 2>&1
            Write-Host "✓ Jekyll found: $jekyllVersion" -ForegroundColor Green
        }
        catch {
            Write-Error "Jekyll is not installed. Install it with: gem install jekyll bundler"
            exit 1
        }

        # Check if Bundler is installed
        try {
            $bundlerVersion = bundle --version 2>&1
            Write-Host "✓ Bundler found: $bundlerVersion" -ForegroundColor Green
        }
        catch {
            Write-Error "Bundler is not installed. Install it with: gem install bundler"
            exit 1
        }
    }

    # Check if AWS CLI is installed (for S3-compatible operations)
    try {
        $awsVersion = aws --version 2>&1
        Write-Host "✓ AWS CLI found: $awsVersion" -ForegroundColor Green
    }
    catch {
        Write-Error "AWS CLI is not installed. Install from: https://aws.amazon.com/cli/"
        exit 1
    }

    # Validate CloudFlare credentials
    if ([string]::IsNullOrEmpty($AccessKeyId) -or [string]::IsNullOrEmpty($SecretAccessKey)) {
        Write-Error "CloudFlare R2 credentials not provided. Set CLOUDFLARE_R2_ACCESS_KEY_ID and CLOUDFLARE_R2_SECRET_ACCESS_KEY environment variables or pass them as parameters."
        exit 1
    }

    # Validate cache purge requirements
    if ($PurgeCache) {
        if ([string]::IsNullOrEmpty($ZoneId) -or [string]::IsNullOrEmpty($ApiToken)) {
            Write-Error "ZoneId and ApiToken are required when PurgeCache is set."
            exit 1
        }
    }

    Write-Host ""
}

# Build the Jekyll site
function Build-JekyllSite {
    if ($SkipBuild) {
        Write-Host "Skipping build (using existing _site directory)..." -ForegroundColor Yellow
        if (-not (Test-Path $SiteDir)) {
            Write-Error "Site directory not found: $SiteDir"
            exit 1
        }
        return
    }

    Write-Host "Building Jekyll site..." -ForegroundColor Yellow
    
    # Navigate to Docs directory
    Push-Location $DocsDir
    try {
        # Install dependencies if Gemfile exists
        if (Test-Path "Gemfile") {
            Write-Host "Installing Ruby dependencies..." -ForegroundColor Gray
            bundle install --quiet
        }

        # Build the site
        Write-Host "Running Jekyll build..." -ForegroundColor Gray
        $env:JEKYLL_ENV = "production"
        bundle exec jekyll build --destination $SiteDir

        if ($LASTEXITCODE -ne 0) {
            throw "Jekyll build failed with exit code $LASTEXITCODE"
        }

        Write-Host "✓ Site built successfully at: $SiteDir" -ForegroundColor Green
        Write-Host ""
    }
    finally {
        Pop-Location
    }
}

# Deploy to CloudFlare R2
function Deploy-ToCloudFlareR2 {
    Write-Host "Deploying to CloudFlare R2..." -ForegroundColor Yellow
    
    # Set AWS CLI environment variables for CloudFlare R2
    $env:AWS_ACCESS_KEY_ID = $AccessKeyId
    $env:AWS_SECRET_ACCESS_KEY = $SecretAccessKey
    $env:AWS_DEFAULT_REGION = "auto"

    try {
        # Sync the site to R2 bucket
        Write-Host "Syncing files to bucket: $BucketName" -ForegroundColor Gray
        
        $syncArgs = @(
            "s3", "sync",
            $SiteDir,
            "s3://$BucketName",
            "--endpoint-url", $R2Endpoint,
            "--delete",  # Remove files that no longer exist
            "--cache-control", "public, max-age=3600",  # 1 hour cache
            "--metadata-directive", "REPLACE"
        )

        # Set proper content types for common file extensions
        Write-Host "Uploading HTML files..." -ForegroundColor Gray
        & aws @syncArgs --exclude "*" --include "*.html" --content-type "text/html; charset=utf-8"
        
        Write-Host "Uploading CSS files..." -ForegroundColor Gray
        & aws @syncArgs --exclude "*" --include "*.css" --content-type "text/css; charset=utf-8"
        
        Write-Host "Uploading JavaScript files..." -ForegroundColor Gray
        & aws @syncArgs --exclude "*" --include "*.js" --content-type "application/javascript; charset=utf-8"
        
        Write-Host "Uploading image files..." -ForegroundColor Gray
        & aws s3 sync $SiteDir "s3://$BucketName" --endpoint-url $R2Endpoint --exclude "*" --include "*.png" --content-type "image/png"
        & aws s3 sync $SiteDir "s3://$BucketName" --endpoint-url $R2Endpoint --exclude "*" --include "*.jpg" --include "*.jpeg" --content-type "image/jpeg"
        & aws s3 sync $SiteDir "s3://$BucketName" --endpoint-url $R2Endpoint --exclude "*" --include "*.svg" --content-type "image/svg+xml"
        & aws s3 sync $SiteDir "s3://$BucketName" --endpoint-url $R2Endpoint --exclude "*" --include "*.gif" --content-type "image/gif"
        & aws s3 sync $SiteDir "s3://$BucketName" --endpoint-url $R2Endpoint --exclude "*" --include "*.webp" --content-type "image/webp"
        
        Write-Host "Uploading remaining files..." -ForegroundColor Gray
        & aws @syncArgs --exclude "*.html" --exclude "*.css" --exclude "*.js" --exclude "*.png" --exclude "*.jpg" --exclude "*.jpeg" --exclude "*.svg" --exclude "*.gif" --exclude "*.webp"

        if ($LASTEXITCODE -ne 0) {
            throw "Failed to sync to CloudFlare R2"
        }

        Write-Host "✓ Files deployed successfully to R2 bucket: $BucketName" -ForegroundColor Green
        Write-Host ""
    }
    finally {
        # Clean up environment variables
        Remove-Item Env:\AWS_ACCESS_KEY_ID -ErrorAction SilentlyContinue
        Remove-Item Env:\AWS_SECRET_ACCESS_KEY -ErrorAction SilentlyContinue
        Remove-Item Env:\AWS_DEFAULT_REGION -ErrorAction SilentlyContinue
    }
}

# Purge CloudFlare cache
function Purge-CloudFlareCache {
    if (-not $PurgeCache) {
        return
    }

    Write-Host "Purging CloudFlare cache..." -ForegroundColor Yellow
    
    $headers = @{
        "Authorization" = "Bearer $ApiToken"
        "Content-Type"  = "application/json"
    }

    $body = @{
        purge_everything = $true
    } | ConvertTo-Json

    try {
        $response = Invoke-RestMethod `
            -Uri "https://api.cloudflare.com/client/v4/zones/$ZoneId/purge_cache" `
            -Method Post `
            -Headers $headers `
            -Body $body

        if ($response.success) {
            Write-Host "✓ CloudFlare cache purged successfully" -ForegroundColor Green
        }
        else {
            Write-Warning "Failed to purge cache: $($response.errors)"
        }
    }
    catch {
        Write-Warning "Failed to purge CloudFlare cache: $_"
    }
    
    Write-Host ""
}

# Main execution
try {
    Test-Prerequisites
    Build-JekyllSite
    Deploy-ToCloudFlareR2
    Purge-CloudFlareCache

    Write-Host "========================================" -ForegroundColor Green
    Write-Host "Deployment completed successfully!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Your documentation is now available on CloudFlare R2." -ForegroundColor Cyan
    Write-Host "Make sure your R2 bucket is configured for public access" -ForegroundColor Cyan
    Write-Host "and connected to a custom domain via CloudFlare." -ForegroundColor Cyan
}
catch {
    Write-Host "========================================" -ForegroundColor Red
    Write-Host "Deployment failed!" -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Red
    Write-Host ""
    Write-Error $_.Exception.Message
    exit 1
}
]]>