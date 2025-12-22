$ErrorActionPreference = 'Stop'

# Package AWS CDK deployment scripts for distribution
$scriptDir = $PSScriptRoot
$version = "1.0.0"  # Update this for each release
$outputName = "skycms-aws-deployment-v$version.zip"
$tempDir = Join-Path $env:TEMP "skycms-aws-package"

Write-Host "Creating SkyCMS AWS Deployment Package v$version" -ForegroundColor Cyan
Write-Host ""

# Clean up any previous temp directory
if (Test-Path $tempDir) {
    Remove-Item -Path $tempDir -Recurse -Force
}

# Create temp directory structure
New-Item -ItemType Directory -Path $tempDir -Force | Out-Null
$packageDir = Join-Path $tempDir "skycms-aws-deployment"
New-Item -ItemType Directory -Path $packageDir -Force | Out-Null

Write-Host "Copying deployment files..." -ForegroundColor Yellow

# Copy PowerShell scripts
Copy-Item -Path (Join-Path $scriptDir "cdk-deploy.ps1") -Destination $packageDir
Copy-Item -Path (Join-Path $scriptDir "cdk-destroy.ps1") -Destination $packageDir

# Copy CDK directory (excluding node_modules, dist, cdk.out, bin)
$cdkSource = Join-Path $scriptDir "cdk"
$cdkDest = Join-Path $packageDir "cdk"
New-Item -ItemType Directory -Path $cdkDest -Force | Out-Null

$cdkFilesToCopy = @(
    "package.json",
    "package-lock.json",
    "tsconfig.json",
    "cdk.json"
)

foreach ($file in $cdkFilesToCopy) {
    $sourcePath = Join-Path $cdkSource $file
    if (Test-Path $sourcePath) {
        Copy-Item -Path $sourcePath -Destination $cdkDest
    }
}

# Copy lib directory
Copy-Item -Path (Join-Path $cdkSource "lib") -Destination (Join-Path $cdkDest "lib") -Recurse -Force

# Copy README
$readmePath = Join-Path $scriptDir "README.md"
if (Test-Path $readmePath) {
    Copy-Item -Path $readmePath -Destination $packageDir
}

# Create a simple installation guide
$installGuide = @"
# SkyCMS AWS Deployment - Quick Start

## Prerequisites

1. **AWS CLI** - Install from https://aws.amazon.com/cli/
   ``````powershell
   aws --version
   ``````

2. **Node.js 18+** - Install from https://nodejs.org/
   ``````powershell
   node --version
   npm --version
   ``````

3. **Docker** - Your Docker image should be available in Docker Hub or ECR
   - Default: toiyabe/sky-editor:latest

4. **AWS Credentials** - Configure with appropriate permissions
   ``````powershell
   aws configure
   ``````

## Installation Steps

1. **Extract this package** to a directory of your choice

2. **Navigate to the directory**
   ``````powershell
   cd skycms-aws-deployment
   ``````

3. **Run the deployment script**
   ``````powershell
   .\cdk-deploy.ps1
   ``````

4. **Follow the interactive prompts** to configure:
   - AWS Region (default: us-east-1)
   - Docker image name
   - ECS task count
   - Database name
   - Stack name
   - Whether to deploy Publisher (S3 + CloudFront)

## What Gets Deployed

- **VPC** with public/private subnets
- **RDS MySQL** database (db.t3.micro)
- **ECS Fargate** cluster with your Docker container
- **Application Load Balancer**
- **CloudFront** distribution with auto-generated TLS certificate
- **S3 bucket** (if Publisher enabled)
- **CloudWatch** log groups

## Estimated Costs

- RDS MySQL (db.t3.micro): ~\$15-20/month
- ECS Fargate (1 task): ~\$15-20/month
- ALB: ~\$16-20/month
- CloudFront: Pay-as-you-go (minimal for low traffic)
- Total: ~\$50-70/month for minimal deployment

## Cleanup

To remove all resources:
``````powershell
.\cdk-destroy.ps1
``````

## Troubleshooting

- Ensure AWS credentials are configured correctly
- Check that your Docker image is accessible
- Review CloudWatch logs for application issues
- Verify security group rules if you can't connect

## Support

For issues or questions, visit: https://github.com/[your-repo]/issues
"@

Set-Content -Path (Join-Path $packageDir "QUICK_START.md") -Value $installGuide

Write-Host "Creating zip archive..." -ForegroundColor Yellow

# Create zip file
$outputPath = Join-Path $scriptDir $outputName
if (Test-Path $outputPath) {
    Remove-Item -Path $outputPath -Force
}

# Use Compress-Archive
Compress-Archive -Path "$packageDir\*" -DestinationPath $outputPath -Force

# Cleanup temp directory
Remove-Item -Path $tempDir -Recurse -Force

Write-Host ""
Write-Host "✅ Package created successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "📦 Package: $outputPath" -ForegroundColor Cyan
Write-Host "📏 Size: $([math]::Round((Get-Item $outputPath).Length / 1MB, 2)) MB" -ForegroundColor Gray
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Upload this zip file to GitHub Releases" -ForegroundColor White
Write-Host "2. Users download and extract the zip" -ForegroundColor White
Write-Host "3. Users run .\cdk-deploy.ps1" -ForegroundColor White
Write-Host ""
