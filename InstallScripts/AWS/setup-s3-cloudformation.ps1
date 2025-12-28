#Requires -Version 5.1
<#
.SYNOPSIS
Sets up an S3 bucket and hosts the CloudFormation template for one-click deployment.

.DESCRIPTION
Creates a public S3 bucket, uploads the CloudFormation template, and outputs the
template URL for use in the Launch Stack button.

.EXAMPLE
.\setup-s3-cloudformation.ps1

.NOTES
Requires AWS CLI and appropriate AWS credentials configured.
#>

$ErrorActionPreference = "Stop"

Write-Host "`n╔════════════════════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║                    AWS S3 CLOUDFORMATION SETUP                            ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════════════════════════╝`n" -ForegroundColor Cyan

# Configuration
$bucketName = "skycms-cloudformation-templates"
$region = "us-east-1"
$templateFile = Join-Path $PSScriptRoot "skycms-editor-cloudformation.yaml"
$templateKey = "skycms-editor-cloudformation.yaml"

# Verify template exists
if (-not (Test-Path $templateFile)) {
    Write-Host "❌ ERROR: Template file not found at: $templateFile" -ForegroundColor Red
    exit 1
}

try {
    # Create S3 bucket
    Write-Host "📦 Creating S3 bucket: $bucketName" -ForegroundColor Yellow
    aws s3 mb "s3://$bucketName" --region $region
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to create S3 bucket"
    }
    
    # Disable block public access
    Write-Host "🔓 Disabling block public access..." -ForegroundColor Yellow
    aws s3api put-public-access-block `
        --bucket $bucketName `
        --public-access-block-configuration "BlockPublicAcls=false,IgnorePublicAcls=false,BlockPublicPolicy=false,RestrictPublicBuckets=false" `
        --region $region
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to disable block public access"
    }
    
    # Upload template
    Write-Host "📤 Uploading CloudFormation template..." -ForegroundColor Yellow
    aws s3 cp $templateFile "s3://$bucketName/$templateKey" --region $region
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to upload template"
    }
    
    # Make template publicly readable via Bucket Policy (ACLs may be disabled)
    Write-Host "🌐 Applying bucket policy for public read (template only)..." -ForegroundColor Yellow
    $policy = @{
        Version = "2012-10-17"
        Statement = @(
            @{
                Sid      = "PublicReadTemplate"
                Effect   = "Allow"
                Principal= "*"
                Action   = @("s3:GetObject")
                Resource = @("arn:aws:s3:::$bucketName/$templateKey")
            }
        )
    } | ConvertTo-Json -Compress

    aws s3api put-bucket-policy `
        --bucket $bucketName `
        --policy $policy `
        --region $region
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to apply bucket policy"
    }
    
    # Generate the template URL
    $templateUrl = "https://$bucketName.s3.$region.amazonaws.com/$templateKey"
    
    Write-Host "`n✅ SUCCESS! S3 Setup Complete" -ForegroundColor Green
    Write-Host "`n📋 S3 Bucket Details:" -ForegroundColor Cyan
    Write-Host "   Bucket Name: $bucketName" -ForegroundColor White
    Write-Host "   Region:      $region" -ForegroundColor White
    Write-Host "   Template:    $templateKey" -ForegroundColor White
    
    Write-Host "`n🔗 CloudFormation Template URL:" -ForegroundColor Cyan
    Write-Host "   $templateUrl" -ForegroundColor White
    
    Write-Host "`n📝 Launch Stack Button URL:" -ForegroundColor Cyan
    $launchUrl = "https://console.aws.amazon.com/cloudformation/home?region=$region#/stacks/new?stackName=skycms&templateURL=$(([uri]::EscapeDataString($templateUrl)))"
    Write-Host "   $launchUrl" -ForegroundColor White
    
    Write-Host "`n📌 Next Steps:" -ForegroundColor Cyan
    Write-Host "   1. Copy the Launch Stack Button URL above" -ForegroundColor White
    Write-Host "   2. Update README.md with the new URL" -ForegroundColor White
    Write-Host "   3. The bucket will be automatically cleaned up in 30 days if unused" -ForegroundColor White
    Write-Host "   4. To keep the bucket permanently, apply a bucket lifecycle policy" -ForegroundColor White
    
    Write-Host "`n💡 Tip: Set bucket lifecycle policy to delete after X days if you want automatic cleanup" -ForegroundColor Yellow
    Write-Host "   See AWS documentation for details: https://docs.aws.amazon.com/AmazonS3/latest/userguide/object-lifecycle-mgmt.html" -ForegroundColor Yellow
    
    Write-Host "`n"
    
    # Copy to clipboard if possible
    try {
        $templateUrl | Set-Clipboard
        Write-Host "✓ Template URL copied to clipboard!" -ForegroundColor Green
    }
    catch {
        # Clipboard copy failed, but continue
    }
}
catch {
    Write-Host "`n❌ ERROR: $_" -ForegroundColor Red
    Write-Host "`nTroubleshooting:" -ForegroundColor Yellow
    Write-Host "  • Verify AWS credentials: aws sts get-caller-identity" -ForegroundColor White
    Write-Host "  • Check AWS CLI is installed: aws --version" -ForegroundColor White
    Write-Host "  • Ensure you have S3 permissions in your AWS account" -ForegroundColor White
    exit 1
}
