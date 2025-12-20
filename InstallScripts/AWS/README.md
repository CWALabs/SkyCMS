# AWS Deployment Scripts for SkyCMS

Complete set of PowerShell scripts for deploying and managing SkyCMS on AWS.

> December 2025 Update
- CloudFront → ALB now forwards `Host`, `CloudFront-Forwarded-Proto`, and `User-Agent` via a custom **Origin Request Policy** to fix setup wizard HTTP 400s (antiforgery).
- Editor application includes middleware that maps `CloudFront-Forwarded-Proto` to `X-Forwarded-Proto` for ASP.NET Core's `UseForwardedHeaders()` middleware.
- **Optional**: Enable end-to-end TLS by providing `-DomainName`, `-HostedZoneId`, `-HostedZoneName` (auto-provisions ACM cert) or `-CertificateArn` (uses existing cert).
- ECS tasks temporarily run with `ASPNETCORE_ENVIRONMENT=Development` for debugging; revert to `Production` after verifying.
- Connection string for the Editor is built dynamically from **RDS endpoint** + **Secrets Manager** credentials; no hardcoded Azure values.
- Apply these changes with a normal redeploy: `./cdk-deploy.ps1` (no teardown needed).

## 📁 Script Overview

### Deployment Scripts

| Script | Purpose | Dependencies |
|--------|---------|--------------|
| **s3-with-cloudfront.yml** | CloudFormation template for static website (S3 + CloudFront) | None |
| **skycms-editor-fargate.yml** | CloudFormation template for SkyCMS Editor (ECS + RDS + CloudFront) | Static site bucket |
| **deploy-skycms-editor.ps1** | Deploy SkyCMS Editor stack | CloudFormation template |
| **sync-editor-to-s3.ps1** | Sync editor build to S3 | Deployed static site |
| **create-s3-access-keys.ps1** | Create IAM credentials for S3 storage access | S3 bucket name |
| **create-cloudfront-purge-user.ps1** | Create IAM credentials for cache invalidation | CloudFront distribution ID |

### Teardown Scripts

| Script | Purpose | What It Deletes |
|--------|---------|-----------------|
| **destroy-skycms-editor.ps1** | Delete Editor stack | ECS, RDS, ALB, CloudFront, VPC, Lambda |
| **destroy-static-site.ps1** | Delete static website stack | S3 bucket, CloudFront, Route 53 records |
| **destroy-all.ps1** | Delete ALL SkyCMS infrastructure | Everything (Editor + Static Site) |

### Other Scripts

| Script | Purpose |
|--------|---------|
| **deploy-rds-mysql.ps1** | Standalone RDS MySQL deployment (alternative approach) |

---

## 🚀 Quick Start Guide

### 1. Deploy Static Website

```powershell
# Deploy S3 bucket with CloudFront (for your public website)
aws cloudformation deploy `
  --template-file ./AWS/s3-with-cloudfront.yml `
  --stack-name skycms-static-site `
  --region us-east-1

# Get bucket name
$bucket = aws cloudformation describe-stacks `
  --stack-name skycms-static-site `
  --query "Stacks[0].Outputs[?OutputKey=='WebsiteBucketName'].OutputValue" `
  --output text
```

### 2. Deploy SkyCMS Editor

```powershell
# Deploy Editor (ECS Fargate + RDS MySQL + CloudFront)
.\InstallScripts\AWS\deploy-skycms-editor.ps1 `
  -StaticSiteBucketName $bucket `
  -DBPassword "YourSecurePassword123"
```

⏱️ Takes 15-20 minutes (RDS creation is slow)

### 3. Create S3 Access Credentials

```powershell
# Create IAM user with S3 permissions (for setup wizard)
.\InstallScripts\AWS\create-s3-access-keys.ps1 `
  -BucketName $bucket
```

📋 Copy the Access Key ID and Secret Access Key shown

### 4. Complete Setup Wizard

1. Get the Setup Wizard URL from deployment outputs
2. Navigate to the URL (wait 2-3 min for ECS tasks to start)
3. Configure:
   - ✅ Database (pre-configured with TLS encryption)
   - 🔑 S3 Storage (use credentials from step 3)
   - 👤 Admin account
   - 🌐 Publisher settings

---

## 🗑️ Teardown Guide

### Delete Everything (Cost Savings)

```powershell
# WARNING: Deletes ALL data and infrastructure
.\InstallScripts\AWS\destroy-all.ps1 `
  -EmptyBucket `
  -DeleteAllIAMUsers
```

### Delete Only Editor (Keep Static Site)

```powershell
.\InstallScripts\AWS\destroy-skycms-editor.ps1 `
  -DeleteIAMUser
```

### Delete Only Static Site (Keep Editor)

```powershell
.\InstallScripts\AWS\destroy-static-site.ps1 `
  -EmptyBucket `
  -DeleteCloudFrontPurgeUser
```

### Safety Features

All teardown scripts include:
- ✅ Confirmation prompts (type "DELETE" to confirm)
- ✅ `-Force` flag to skip prompts (for automation)
- ✅ Warnings about data loss
- ✅ Status reporting

---

## 📋 Common Workflows

### Development/Testing Cycle

```powershell
# Deploy for testing
.\InstallScripts\AWS\deploy-skycms-editor.ps1 -StaticSiteBucketName $bucket -DBPassword "Test123"

# ... test your changes ...

# Tear down to save costs
.\InstallScripts\AWS\destroy-skycms-editor.ps1 -Force -DeleteIAMUser
```

### Production Deployment

```powershell
# 1. Deploy static site with custom domain
aws cloudformation deploy `
  --template-file ./AWS/s3-with-cloudfront.yml `
  --stack-name prod-skycms-site `
  --parameter-overrides DomainName=www.example.com HostedZoneId=Z1234 ACMCertificateArn=arn:...

# 2. Deploy editor
.\InstallScripts\AWS\deploy-skycms-editor.ps1 `
  -StackName prod-skycms-editor `
  -StaticSiteBucketName $bucket `
  -DBPassword $securePassword `
  -DBInstanceClass db.t4g.small `
  -TaskCPU 1024 `
  -TaskMemory 2048

# 3. Create credentials
.\InstallScripts\AWS\create-s3-access-keys.ps1 -BucketName $bucket -UserName prod-skycms-s3
```

### Backup Before Teardown

```powershell
# Export RDS snapshot before deletion
$dbEndpoint = aws cloudformation describe-stacks `
  --stack-name skycms-editor-fargate `
  --query "Stacks[0].Outputs[?OutputKey=='DatabaseEndpoint'].OutputValue" `
  --output text

aws rds create-db-snapshot `
  --db-instance-identifier skycms-editor-fargate-mysql `
  --db-snapshot-identifier skycms-backup-$(Get-Date -Format yyyyMMdd)

# Then proceed with teardown
.\InstallScripts\AWS\destroy-skycms-editor.ps1
```

---

## 🔧 Script Parameters

### destroy-skycms-editor.ps1

```powershell
-StackName           # CloudFormation stack name (default: skycms-editor-fargate)
-Region              # AWS region (default: us-east-1)
-Force               # Skip confirmation prompt
-DeleteIAMUser       # Also delete S3 access IAM user
-IAMUserName         # IAM user to delete (default: skycms-s3-access)
```

### destroy-static-site.ps1

```powershell
-StackName                    # CloudFormation stack name (default: skycms-static-site)
-Region                       # AWS region (default: us-east-1)
-Force                        # Skip confirmation prompt
-EmptyBucket                  # Empty S3 bucket before deletion (required)
-DeleteCloudFrontPurgeUser    # Also delete CloudFront purge IAM user
-CloudFrontPurgeUserName      # IAM user to delete (default: skycms-purge-user)
```

### destroy-all.ps1

```powershell
-EditorStackName      # Editor stack name (default: skycms-editor-fargate)
-StaticSiteStackName  # Static site stack name (default: skycms-static-site)
-Region               # AWS region (default: us-east-1)
-Force                # Skip confirmation prompt
-EmptyBucket          # Empty S3 bucket before deletion
-DeleteAllIAMUsers    # Delete all SkyCMS IAM users
```

---

## 📖 Documentation

- **[PHASE1_SETUP_WIZARD.md](PHASE1_SETUP_WIZARD.md)** - Complete setup wizard guide
- **[AWS S3 Access Keys](../Docs/AWS-S3-AccessKeys.md)** - IAM credential setup details
- **[AWSInstall.md](../Docs/Installation/AWSInstall.md)** - General AWS installation guide

---

## 💰 Cost Management Tips

1. **Delete when not in use** - Run `destroy-all.ps1` nightly for dev environments
2. **Use smaller instances** - db.t4g.micro for development
3. **Reduce task count** - Set `DesiredCount: 0` to stop ECS without deleting
4. **Enable auto-scaling** - Only pay for what you use (Phase 2 feature)

---

## ⚠️ Important Notes

- **S3 buckets must be empty** before CloudFormation can delete them
  - Use `-EmptyBucket` flag with teardown scripts
  - Or manually empty: `aws s3 rm s3://bucket-name --recursive`

- **CloudFront deletion is slow** (15-20 minutes)
  - This is normal AWS behavior
  - Deletion continues even if you cancel the wait

- **RDS deletion is permanent**
  - Consider snapshots for production databases
  - Automated backups are kept per retention policy

- **IAM users** are NOT deleted by default
  - Use `-DeleteIAMUser` or `-DeleteAllIAMUsers` flags explicitly
  - Prevents accidental credential deletion

---

## 🐛 Troubleshooting

### "Stack deletion failed - S3 bucket not empty"
```powershell
# Empty the bucket first
aws s3 rm s3://your-bucket-name --recursive

# Or use the flag
.\destroy-static-site.ps1 -EmptyBucket
```

### "Cannot delete stack - resource in use"
This usually means CloudFront distribution is still deploying. Wait 10 minutes and retry.

### "Access denied" errors
Ensure your AWS credentials have sufficient permissions:
- CloudFormation full access
- EC2, ECS, RDS, S3, CloudFront, IAM permissions

---

**Last Updated**: December 2025  
**Tested With**: AWS CLI v2, PowerShell 7+
