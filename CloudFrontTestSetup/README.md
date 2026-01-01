# CloudFront Test Infrastructure

Simple AWS CDK setup for testing CloudFront integration.

## Prerequisites

- **AWS CLI** configured with credentials: `aws configure`
- **Node.js** 18+ installed
- **PowerShell 7+** (already installed on Windows 11)

## Quick Start

### Deploy CloudFront

cd Tests/Infrastructure/CloudFront .\deploy.ps1

This script will:
1. ✅ Check prerequisites
2. ✅ Install npm dependencies
3. ✅ Bootstrap AWS CDK (if needed)
4. ✅ Deploy CloudFront distribution
5. ✅ Configure .NET user secrets automatically

### Run Tests

After deployment completes:

1. Edit `Tests\Services\CDN\CloudFrontCdnServiceTests.cs`
2. Remove the `[Ignore]` attribute from the test class
3. Run:

cd ......  # Back to Tests folder dotnet test --filter "TestCategory=CloudFront"

## Clean Up

When done testing:

cd Tests/Infrastructure/CloudFront .\destroy.ps1

Or to also clear user secrets:

cd ..\..\..\..\SecretsManager .\destroy.ps1

## What Gets Deployed

- **CloudFront Distribution** - CDN for testing invalidations
- **S3 Bucket** - Origin storage
- **IAM User** - With permissions for cache invalidation
- **Access Keys** - Automatically configured in user secrets

## Estimated Cost

~$1-2/month while active (mostly within AWS free tier)

