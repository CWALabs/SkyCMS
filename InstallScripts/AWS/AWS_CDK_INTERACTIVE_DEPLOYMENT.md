# SkyCMS AWS CDK Interactive Deployment Guide

This guide describes the enhanced **interactive deployment script** (`cdk-deploy.ps1`) that deploys SkyCMS Editor and optionally SkyCMS Publisher to AWS using CDK and CloudFormation.

## Overview

The new deployment workflow is fully **interactive** and **user-friendly**:

1. **No command-line flags required** — all configuration is gathered via prompts
2. **Optional Publisher deployment** — choose whether to deploy S3+CloudFront Publisher
3. **Automatic IAM user creation** — access credentials generated automatically
4. **Secrets Manager integration** — storage connection string secured and injected
5. **Unified output summary** — both Publisher and Editor URLs displayed at the end

## Prerequisites

- **Windows PowerShell 5.1+** or **PowerShell Core 7+**
- **AWS CLI** configured with credentials (`aws configure`)
- **Node.js 18+** and **npm** installed
- **Docker** running (for container image validation)
- **AWS Permissions**: CloudFormation, ECS, RDS, S3, CloudFront, IAM, Secrets Manager, ACM, EC2

## Quick Start

### 1. Run the Script

```powershell
cd D:\source\SkyCMS\InstallScripts\AWS
.\cdk-deploy.ps1
```

### 2. Answer the Interactive Prompts

The script will ask you for:

**Editor Configuration:**
- `AWS Region` — defaults to `us-east-1`
- `Docker Image` — defaults to `toiyabe/sky-editor:latest`
- `Desired Task Count` — defaults to `1` (number of ECS tasks)
- `Database Name` — defaults to `skycms`
- `Stack Name` — defaults to `skycms-editor`
- `(Optional) Domain Name` — CloudFront custom domain
- `(Optional) Hosted Zone ID` — Route 53 zone for custom domain
- `(Optional) ACM Certificate ARN` — pre-existing certificate for custom domain

**Publisher Configuration:**
- `Deploy Publisher (S3 + CloudFront)?` — yes/no prompt
  - If yes:
    - `Stack Name` — defaults to `skycms-publisher`
    - `(Optional) Domain Name` — custom domain for Publisher CloudFront
    - `(Optional) Hosted Zone ID` — Route 53 zone for custom domain

### 3. Review Configuration Summary

Before deploying, the script displays a summary:

```
======================================
DEPLOYMENT CONFIGURATION SUMMARY
======================================

Editor:
  Stack Name: skycms-editor
  Region: us-east-1
  Image: toiyabe/sky-editor:latest
  Desired Count: 1
  Database: skycms

Publisher: ENABLED
  Stack Name: skycms-publisher
  Domain: publisher.example.com

Continue with deployment? (y/n) [Y]
```

### 4. Deployment Proceeds

The script will:

1. **Publisher** (if selected):
   - Deploy S3 bucket + CloudFront distribution
   - Request ACM certificate for custom domain (if provided)
   - Create IAM user `skycms-s3-publisher-user`
   - Generate access keys
   - Store S3 connection string in Secrets Manager as `SkyCms-StorageConnectionString`

2. **Editor**:
   - Bootstrap CDK (if needed)
   - Synthesize CloudFormation template
   - Deploy ECS cluster + RDS MySQL + CloudFront
   - Inject storage connection string from Secrets Manager (if Publisher deployed)

### 5. Review Final Output

```
========================================
✅ DEPLOYMENT COMPLETE!
========================================

📦 PUBLISHER (S3 + CloudFront):
   S3 Bucket: skycms-publisher-bucket-xxx
   CloudFront URL: https://d123abc.cloudfront.net
   Custom Domain: https://publisher.example.com
   IAM User: skycms-s3-publisher-user

   📝 To upload website files:
   aws s3 sync ./website s3://skycms-publisher-bucket-xxx/

📝 EDITOR (ECS + RDS + CloudFront):
   Stack Name: skycms-editor
   CloudFront URL: https://d456def.cloudfront.net
   Custom Domain: https://editor.example.com
   Database: skycms @ skycms-rds-xxx.us-east-1.rds.amazonaws.com
   Storage Secret: arn:aws:secretsmanager:us-east-1:123456789:secret:SkyCms-StorageConnectionString-xxx

⏳ CloudFront may take 1-2 minutes to fully propagate.
```

## Feature Details

### Interactive Prompts with Defaults

All prompts support default values shown in `[brackets]`. Press Enter to accept defaults:

```powershell
AWS Region [us-east-1]: 
# (Press Enter to accept us-east-1)

AWS Region [us-east-1]: eu-west-1
# (Type to override with eu-west-1)
```

### Publisher Deployment (Optional)

If you select **yes** for Publisher deployment:

1. **S3 Bucket** created with public access via CloudFront
2. **CloudFront distribution** with Origin Access Identity (OAI) for secure S3 access
3. **ACM Certificate** auto-requested for custom domain (if provided)
4. **IAM User** created: `skycms-s3-publisher-user`
5. **Access Keys** generated for programmatic S3 access
6. **S3 Policy** attached with least-privilege permissions (GetObject, PutObject, DeleteObject, ListBucket)
7. **Secrets Manager Secret** created: `SkyCms-StorageConnectionString`

**Connection String Format:**
```
Bucket=skycms-publisher-bucket-xxx;Region=us-east-1;KeyId=AKIAIOSFODNN7EXAMPLE;Key=wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY;
```

### Storage Integration with Editor

When Publisher is deployed:

1. **Connection string** is automatically created and stored in Secrets Manager
2. **Editor ECS task** receives connection string as environment variable `ConnectionStrings__StorageConnectionString`
3. **Editor application** automatically picks up S3 configuration during startup
4. **File uploads** in Editor UI are stored in S3 bucket

### Custom Domain Names

Both Publisher and Editor support custom domains via Route 53 + ACM:

**For Publisher:**
```
Domain Name: publisher.example.com
Hosted Zone ID: Z1234567890ABC
# (ACM certificate auto-requested or selected)
```

**For Editor:**
```
Domain Name: editor.example.com
Hosted Zone ID: Z1234567890ABC
# (ACM certificate auto-requested or selected)
```

If no custom domain is specified, CloudFront-provided domain is used (e.g., `d123abc.cloudfront.net`).

## Workflow Summary

### Full Deployment (Publisher + Editor)

```
User runs script
    ↓
Interactive prompts (Editor config)
    ↓
Interactive prompts (Publisher option + custom domains)
    ↓
Configuration summary + confirmation
    ↓
Deploy Publisher (CloudFormation)
  ├─ Create S3 bucket
  ├─ Create CloudFront distribution
  ├─ Create IAM user
  ├─ Create access keys
  └─ Store connection string in Secrets Manager
    ↓
Deploy Editor (CDK)
  ├─ Bootstrap CDK
  ├─ Synthesize CloudFormation
  ├─ Deploy ECS + RDS + CloudFront
  └─ Inject storage secret from Secrets Manager
    ↓
Final output summary (URLs + next steps)
```

### Editor-Only Deployment (No Publisher)

```
User runs script
    ↓
Interactive prompts (Editor config)
    ↓
Skip Publisher (user selects 'n')
    ↓
Configuration summary + confirmation
    ↓
Deploy Editor (CDK)
  ├─ Bootstrap CDK
  ├─ Synthesize CloudFormation
  ├─ Deploy ECS + RDS + CloudFront
  └─ No storage secret injected
    ↓
Final output summary (Editor URL only)
```

## Environment Variables

The script uses these AWS-related environment variables:

- `AWS_REGION` — AWS region (overridden by interactive prompt)
- `AWS_ACCESS_KEY_ID` — AWS credentials
- `AWS_SECRET_ACCESS_KEY` — AWS credentials
- `AWS_PROFILE` — Named AWS profile (optional)

Configure via:
```powershell
$env:AWS_REGION = "us-east-1"
$env:AWS_ACCESS_KEY_ID = "AKIAIOSFODNN7EXAMPLE"
$env:AWS_SECRET_ACCESS_KEY = "wJalrXUt..."
```

Or use `aws configure`:
```powershell
aws configure
```

## Troubleshooting

### Script hangs at npm install
- Check Node.js/npm installation: `npm --version`
- Try clearing npm cache: `npm cache clean --force`

### CloudFormation deploy fails
- Check AWS credentials: `aws sts get-caller-identity`
- Verify IAM permissions for CloudFormation, S3, CloudFront, etc.
- Check CloudFormation events in AWS Console

### ACM certificate validation fails
- Certificate must be validated via DNS before CloudFront can use it
- Add DNS records as shown in ACM console
- Wait 5-10 minutes for validation to complete

### RDS database not accessible
- Script automatically adds your public IP to security group
- If still failing, manually check RDS security group in AWS Console
- Verify firewall allows port 3306 outbound

### S3 access keys not showing
- Check Secrets Manager in AWS Console: `SkyCms-StorageConnectionString`
- Verify IAM user was created: `skycms-s3-publisher-user`
- Check IAM user has S3 policy attached

### CloudFront URL not working
- Wait 1-2 minutes for CloudFront propagation
- Check CloudFront distribution status in AWS Console
- Verify origin (S3 bucket or ALB) is accessible

## Next Steps

### After Deployment

1. **Wait for CloudFront propagation** (~1-2 minutes)
2. **Visit Editor URL** and run setup wizard
3. **Upload Publisher files** (if deployed):
   ```powershell
   aws s3 sync ./website s3://skycms-publisher-bucket-xxx/
   ```
4. **Test S3 file uploads** in Editor media manager
5. **Configure custom DNS records** (if using custom domains)

### Managing Deployments

**Update Editor configuration:**
```powershell
# Re-run the script with new parameters
.\cdk-deploy.ps1
```

**Delete Editor stack:**
```powershell
aws cloudformation delete-stack --stack-name skycms-editor --region us-east-1
```

**Delete Publisher stack:**
```powershell
aws cloudformation delete-stack --stack-name skycms-publisher --region us-east-1
aws s3 rm s3://skycms-publisher-bucket-xxx --recursive  # Delete bucket contents first
```

## Security Notes

- **IAM credentials** are generated with least-privilege S3 scoping
- **Storage connection string** is encrypted in AWS Secrets Manager
- **ECS task** receives credentials via Secrets Manager injection (not in task definition)
- **CloudFront distributions** use HTTPS only
- **Custom domains** require ACM certificates
- **S3 bucket** is not publicly accessible (CloudFront OAI provides access)

## Related Files

- [cdk-deploy.ps1](./cdk-deploy.ps1) — Main deployment script
- [s3-with-cloudfront.yml](./s3-with-cloudfront.yml) — Publisher CloudFormation template
- [cdk/lib/skycms-stack-minimal.ts](./cdk/lib/skycms-stack-minimal.ts) — Editor CDK stack definition
- [PUBLISHER_DEPLOYMENT.md](./PUBLISHER_DEPLOYMENT.md) — Manual Publisher deployment guide

## Support

For issues or questions:
1. Check the troubleshooting section above
2. Review AWS CloudFormation events in AWS Console
3. Check CloudWatch logs for ECS/RDS issues
4. Run `aws sts get-caller-identity` to verify credentials
