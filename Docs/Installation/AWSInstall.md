---
title: AWS Installation Guide
description: Deploy SkyCMS on AWS using S3 for static website hosting and blob storage
keywords: AWS, installation, S3, deployment, setup
audience: [developers, devops]
---

# AWS Installation Guide

Deploy SkyCMS on AWS using either the **interactive CDK deployment** (recommended) or the **manual S3-only setup**.

## Quick Start

### Interactive CDK Deployment (Recommended)

Deploy SkyCMS Editor and optionally Publisher to AWS with a single interactive script.

**Prerequisites:**
- Windows PowerShell 5.1+ or PowerShell Core 7+
- AWS CLI configured (`aws configure`)
- Node.js 18+ and npm installed
- Docker running
- AWS Permissions: CloudFormation, ECS, RDS, S3, CloudFront, IAM, Secrets Manager, ACM, EC2

**Run the deployment:**
```powershell
cd InstallScripts/AWS
./cdk-deploy.ps1
```

The script will interactively prompt you for:
- **Editor Configuration**: Region, Docker image, task count, database name, stack name
- **Custom Domains** (optional): Domain name, Route 53 hosted zone ID, ACM certificate
- **Publisher Deployment** (optional): S3 + CloudFront for static website hosting
- **Amazon SES SMTP** (optional): Email configuration with password stored in Secrets Manager

⏱️ **Deployment time:** 15-20 minutes (RDS provisioning is the longest part)

**What gets deployed:**
- ✅ **Editor**: ECS Fargate cluster + RDS MySQL + Application Load Balancer + CloudFront distribution
- ✅ **Publisher** (optional): S3 bucket + CloudFront distribution + IAM credentials
- ✅ **Secrets Manager**: Database credentials, storage connection string, SMTP password (if enabled)
- ✅ **Custom Domains** (optional): Route 53 DNS records + ACM SSL certificates

---

## Detailed Interactive Deployment Guide

### Step 1: Run the Script

```powershell
cd D:\source\SkyCMS\InstallScripts\AWS
.\cdk-deploy.ps1
```

### Step 2: Answer Interactive Prompts

**Editor Configuration:**
```
AWS Region [us-east-1]: 
Docker Image [toiyabe/sky-editor:latest]: 
Desired Task Count [1]: 
Database Name [skycms]: 
Stack Name [SkyCMS-Stack]: 
Domain Name (optional): 
Hosted Zone ID (optional): 
ACM Certificate ARN (optional): 
```

**Email Configuration (Amazon SES SMTP):**
```
Enable SES SMTP? (yes/no) [no]: 
  If yes:
    Sender Email: admin@example.com
    SES SMTP Username: 
    SES SMTP Password: 
    Secret Name for Password [SkyCms-SmtpPassword]: 
```

**Publisher Configuration:**
```
Deploy Publisher (S3 + CloudFront)? (y/n) [Y]: 
  If yes:
    Publisher Stack Name [skycms-publisher]: 
    Publisher Domain Name (optional): 
    Publisher Hosted Zone ID (optional): 
```

### Step 3: Review Configuration Summary

The script displays a summary before deployment:

```
======================================
DEPLOYMENT CONFIGURATION SUMMARY
======================================

Editor:
  Stack Name: SkyCMS-Stack
  Region: us-east-1
  Image: toiyabe/sky-editor:latest
  Desired Count: 1
  Database: skycms

Email (SES): ENABLED
  Sender: admin@example.com

Publisher: ENABLED
  Stack Name: skycms-publisher
  Domain: publisher.example.com

Continue with deployment? (y/n) [Y]:
```

### Step 4: Deployment Process

The script automatically:

**If Publisher is selected:**
1. Deploys S3 bucket + CloudFront distribution (CloudFormation)
2. Requests/assigns ACM certificate for custom domain (if provided)
3. Creates IAM user `skycms-s3-publisher-user` with S3 permissions
4. Generates access keys
5. Stores connection string in Secrets Manager as `SkyCms-StorageConnectionString`

**For Editor:**
1. Bootstraps AWS CDK (first-time setup)
2. Synthesizes CloudFormation template
3. Deploys VPC, ECS cluster, RDS MySQL, ALB, CloudFront
4. Stores database credentials in Secrets Manager
5. Injects storage connection string from Secrets Manager (if Publisher deployed)
6. Injects SES SMTP settings and password (if enabled)

### Step 5: Review Deployment Output

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
   Storage Secret: arn:aws:secretsmanager:...
   SES SMTP: Enabled (password injected from Secrets Manager)

⏳ CloudFront may take 1-2 minutes to fully propagate.
```

---

## Post-Deployment Steps

1. **Wait for CloudFront propagation** (~1-2 minutes)
2. **Visit the Editor URL** from the deployment output
3. **Run the Setup Wizard** to configure remaining settings (see [Setup Wizard Guide](./SetupWizard.md))
4. **Verify email** (if SES enabled): Verify sender identity and recipients if in sandbox mode
5. **Upload Publisher files** (if deployed):
   ```powershell
   aws s3 sync ./website s3://your-bucket-name/
   ```

---

## Feature Details

### Publisher Deployment (S3 + CloudFront)

When you select Publisher deployment:

- **S3 Bucket** created for static website hosting
- **CloudFront Distribution** with Origin Access Identity (OAI) for secure S3 access
- **IAM User** `skycms-s3-publisher-user` with least-privilege S3 permissions
- **Access Keys** generated for programmatic S3 access
- **Secrets Manager Secret** `SkyCms-StorageConnectionString` with connection string

**Connection String Format:**
```
Bucket=bucket-name;Region=us-east-1;KeyId=AKIA...;Key=secret-key;
```

The Editor automatically receives this connection string via Secrets Manager injection, enabling seamless file uploads to S3.

### Amazon SES SMTP Configuration

When you enable SES SMTP:

- **Sender Email** must be a verified SES identity (single email in sandbox is fine)
- **SMTP Credentials** stored in Secrets Manager
- **Environment Variables** automatically configured:
  - Host: `email-smtp.<region>.amazonaws.com`
  - Port: `587` (STARTTLS)
  - Username: Your SES SMTP username
  - Password: Injected from Secrets Manager
  - UseSSL: `false` (STARTTLS is used)

**Note**: In sandbox mode, recipients must also be verified. Request production access to send to any email.

### Custom Domain Names

Both Publisher and Editor support custom domains via Route 53 + ACM:

**Requirements:**
- Domain hosted in Route 53 with Hosted Zone ID
- ACM certificate in the same region (auto-requested if not provided)

**Configuration:**
```
Domain Name: editor.example.com
Hosted Zone ID: Z1234567890ABC
```

If no custom domain is specified, CloudFront-provided domain is used (e.g., `d123abc.cloudfront.net`).

---

## Troubleshooting

### Script Issues

| Problem | Solution |
|---------|----------|
| Script hangs at npm install | Check Node.js: `npm --version`; Clear cache: `npm cache clean --force` |
| AWS credentials error | Verify: `aws sts get-caller-identity` |
| CloudFormation deploy fails | Check IAM permissions for CloudFormation, S3, CloudFront, ECS, RDS |

### Deployment Issues

| Problem | Solution |
|---------|----------|
| ACM certificate validation fails | Add DNS records shown in ACM console; Wait 5-10 minutes |
| RDS not accessible | Script adds your IP to security group; Verify firewall allows port 3306 |
| CloudFront URL not working | Wait 1-2 minutes for propagation; Check distribution status in console |
| S3 access keys not showing | Check Secrets Manager: `SkyCms-StorageConnectionString` |

### SES Email Issues

| Problem | Solution |
|---------|----------|
| Email not sending | Verify sender identity in SES console |
| Recipient not receiving | In sandbox mode, verify recipient email in SES |
| SMTP authentication fails | Check SMTP credentials in Secrets Manager |

---

## Managing Deployments

### Update Configuration

Re-run the deployment script with new parameters:
```powershell
.\cdk-deploy.ps1
```

### Delete Resources

**Delete Editor stack:**
```powershell
cdk destroy --all
# Or manually:
aws cloudformation delete-stack --stack-name skycms-editor --region us-east-1
```

**Delete Publisher stack:**
```powershell
# Empty bucket first
aws s3 rm s3://skycms-publisher-bucket-xxx --recursive
# Delete stack
aws cloudformation delete-stack --stack-name skycms-publisher --region us-east-1
```

---

## Alternative: Manual S3-Only Setup
1. **AWS Account** - Ensure you have an active AWS account with S3 access
2. **S3 Bucket** - Create an S3 bucket for your site content
3. **Access Keys** - Generate IAM credentials with S3 permissions
4. **Configuration** - Update SkyCMS settings with your S3 details

See [AWS S3 Access Keys Setup](../Configuration/AWS-S3-AccessKeys.md) for detailed credential creation steps.

For users who only need S3 storage without the full Editor deployment:

1. **AWS Account** - Ensure you have an active AWS account with S3 access
2. **S3 Bucket** - Create an S3 bucket for your site content
3. **Access Keys** - Generate IAM credentials with S3 permissions
4. **Configuration** - Update SkyCMS settings with your S3 details

See [AWS S3 Access Keys Setup](../Configuration/AWS-S3-AccessKeys.md) for detailed credential creation steps.

### Manual Deployment Options

**Option 1: S3 Static Website Hosting**
- Enable static website hosting on your S3 bucket
- Use CloudFront or another CDN for edge distribution
- See [S3 Storage Configuration](../Configuration/Storage-S3.md) for setup details

**Option 2: Dynamic Hosting with S3 Storage**
- Deploy SkyCMS application to AWS (EC2, Lightsail, App Runner, etc.)
- Use S3 as your blob storage provider for media and assets
- See [Storage Configuration Reference](../Configuration/Storage-Configuration-Reference.md)

## Related Documentation

- **[Minimum Required Settings](./MinimumRequiredSettings.md)** - Essential configuration for any deployment
- **[AWS S3 Access Keys](../Configuration/AWS-S3-AccessKeys.md)** - Step-by-step credential setup
- **[Storage Configuration Reference](../Configuration/Storage-Configuration-Reference.md)** - Complete S3 integration guide
- **[S3 Storage Overview](../Configuration/Storage-S3.md)** - S3-specific implementation details
