# SkyCMS Editor Deployment - Phase 1: Setup Wizard Configuration

## Overview

Phase 1 of the SkyCMS Editor deployment on AWS focuses on:
- ✅ Automated database pre-creation via Lambda
- ✅ Setup wizard enablement for single-tenant configuration
- ✅ S3 storage integration (same bucket as static website)
- ✅ CloudFront distribution with no caching (dynamic content)
- ✅ Environment variables for single-tenant mode

## Prerequisites

1. **Static Website Stack** deployed using `s3-with-cloudfront.yml`
   - You'll need the S3 bucket name from the outputs
2. **AWS CLI v2** installed and configured
3. **Docker image** for SkyCMS Editor (default: `skycms/editor:latest`)

## Deployment Steps

### Step 1: Get Static Site Bucket Name

```powershell
# List outputs from the static site stack
aws cloudformation describe-stacks `
  --stack-name skycms-static-site `
  --region us-east-1 `
  --query "Stacks[0].Outputs[?OutputKey=='WebsiteBucketName'].OutputValue" `
  --output text
```

**Note this bucket name** - you'll need it for the next step.

### Step 2: Deploy SkyCMS Editor Stack

```powershell
./deploy-skycms-editor.ps1 `
  -StackName "skycms-editor" `
  -Region "us-east-1" `
  -ContainerImage "skycms/editor:latest" `
  -StaticSiteBucketName "your-bucket-name-from-step-1" `
  -DBPassword "YourSecurePassword123" `
  -DBInstanceClass "db.t4g.micro" `
  -TaskCPU "512" `
  -TaskMemory "1024"
```

**⏱️ Expected time: 15-20 minutes** (mostly waiting for RDS to provision)

### Step 3: Wait for Deployment to Complete

Watch the CloudFormation stack in AWS Console or run:

```powershell
aws cloudformation describe-stacks `
  --stack-name skycms-editor `
  --region us-east-1 `
  --query "Stacks[0].StackStatus" `
  --output text
```

Status should change from `CREATE_IN_PROGRESS` → `CREATE_COMPLETE`

### Step 4: Access the Setup Wizard

Once deployment completes, CloudFormation outputs will show `SetupWizardURL`:

```powershell
aws cloudformation describe-stacks `
  --stack-name skycms-editor `
  --region us-east-1 `
  --query "Stacks[0].Outputs[?OutputKey=='SetupWizardURL'].OutputValue" `
  --output text
```

**Note:** It may take 2-3 minutes for ECS tasks to fully start and pass health checks. If you get a 502 error, wait a bit and refresh.

### Step 5: Complete Setup Wizard

Navigate to the `SetupWizardURL` and configure:

1. **Database** (pre-configured, just verify):
   - Host: (auto-filled from RDS endpoint)
   - Database: `skycms` (pre-created)
   - Username: `skycms_admin`
   - Password: (as you set during deployment)

2. **Storage** (S3 integration):
   - Provider: Amazon S3
   - Bucket Name: (pre-filled from your static site bucket)
   - Region: (auto-filled)
   - **Access Key ID**: (Get from your AWS account - see below)
   - **Secret Access Key**: (Get from your AWS account - see below)

3. **Administrator Account**:
   - Email: (your admin email)
   - Username: (your admin username)
   - Password: (strong password, min 8 chars)

4. **Publisher Settings** (optional initially):
   - URL: (your domain or leave for later)
   - Email: (support email)

5. **Additional Settings** (optional):
   - Email configuration
   - CDN settings

## Getting AWS S3 Credentials for Setup Wizard

You'll need **IAM Access Keys** to authorize SkyCMS to upload/download files to S3.

### Automated Script (Recommended)

Use the provided script to create credentials with least-privilege access:

```powershell
.\InstallScripts\AWS\create-s3-access-keys.ps1 `
  -BucketName "your-static-site-bucket-name" `
  -Region "us-east-1"
```

This creates:
- IAM user with scoped S3 permissions (read/write only to your bucket)
- Access key credentials
- Clear output with next steps

**Optional**: Save credentials to a file (DO NOT commit):
```powershell
.\InstallScripts\AWS\create-s3-access-keys.ps1 `
  -BucketName "your-bucket-name" `
  -OutputPath "credentials.txt"
```

**⚠️ Store the Access Key ID and Secret Access Key securely** - they're shown only once.

## What Happens During Deployment

### CloudFormation Resources Created

- **VPC**: Isolated network (10.0.0.0/16)
- **RDS MySQL**: Database instance
  - Pre-created `skycms` database (via Lambda)
  - Storage encrypted at rest
  - 7-day backup retention
- **ECS Fargate Cluster**: Application container runtime
  - 1+ tasks running SkyCMS Editor
  - Behind Application Load Balancer
- **Application Load Balancer**: Internal HTTP endpoint
- **CloudFront Distribution**: Public HTTPS endpoint
  - No caching (TTL=0)
  - All headers/cookies forwarded to origin
  - Default AWS certificate

### Lambda Initialization

The `InitDatabaseFunction` Lambda:
- Runs **after RDS is available**
- Connects as the master user
- Creates the `skycms` database with UTF-8MB4 character set
- Exits gracefully if database already exists

**Why separate from CloudFormation?**
- CloudFormation's RDS resource doesn't support creating databases within an instance
- Lambda provides more flexibility for initialization logic

## Troubleshooting

### "502 Bad Gateway" from CloudFront

- **Cause**: ECS tasks haven't started yet
- **Fix**: Wait 2-3 minutes and refresh

### "Can't connect to database" in setup wizard

- **Cause**: Database initialization Lambda failed
- **Fix**: Check Lambda logs:
  ```powershell
  aws logs tail /aws/lambda/skycms-editor-init-db --follow
  ```

### "Access denied" when uploading files to S3

- **Cause**: IAM credentials are incorrect or don't have S3 permissions
- **Fix**: Verify credentials and ensure user has `AmazonS3FullAccess` policy

### RDS creation taking too long

- **Note**: This is normal - RDS provisioning can take 10-15 minutes
- Check stack events for progress:
  ```powershell
  aws cloudformation describe-stack-events `
    --stack-name skycms-editor `
    --region us-east-1 `
    --query "StackEvents[0:10]"
  ```

## Configuration Files After Setup

After you complete the setup wizard, SkyCMS stores all settings in the MySQL database. The app will no longer use the setup wizard (it's disabled on subsequent runs).

To access configuration later:
- Via web UI: Admin panel in SkyCMS Editor
- Via database: `appsettings` and `appconfig` tables in the `skycms` database

## Cost Considerations

### Ongoing Monthly Costs (Rough Estimates)

- **RDS MySQL** (db.t4g.micro): ~$8-12/month
- **Fargate** (512 CPU, 1GB RAM, 1 task): ~$30-35/month
- **Load Balancer**: ~$15/month
- **CloudFront**: $0.085/GB (pay per GB served)
- **Data transfer**: Variable

**Total minimum**: ~$50-60/month before data transfer

### Cost-Saving Tips

- **Delete when not needed** - Use teardown scripts for dev/test environments
- **Tear down overnight** - Destroy and recreate daily to save ~$2-3/day
- Use RDS Savings Plan for production (save 20-30%)
- Reduce task count if not needed (set `DesiredCount: 0`)
- Consider database backup retention period

### Teardown Scripts (💰 Save Money!)

**Delete everything when not in use:**
```powershell
# Delete all infrastructure (saves ~$50-60/month)
.\InstallScripts\AWS\destroy-all.ps1 -EmptyBucket -DeleteAllIAMUsers

# Redeploy later when needed
.\InstallScripts\AWS\deploy-skycms-editor.ps1 -StaticSiteBucketName $bucket -DBPassword "..."
```

**See [README.md](README.md) for complete teardown documentation.**

## Next Steps (Phase 2)

Phase 2 will address:
- ✅ TLS encryption between ECS and RDS (in-transit encryption)
- ✅ AWS Secrets Manager for password management
- ✅ IAM database authentication
- ✅ Improved monitoring and logging
- ✅ Auto-scaling based on demand

To prepare for Phase 2, save:
1. Your CloudFormation stack name
2. Your RDS endpoint
3. Your S3 bucket name

---

**Documentation Version**: Phase 1 (December 2025)  
**Related Files**:
- `skycms-editor-fargate.yml` - Main CloudFormation template
- `deploy-skycms-editor.ps1` - Deployment script wrapper
- `s3-with-cloudfront.yml` - Static website stack
- `sync-editor-to-s3.ps1` - Content synchronization utility
