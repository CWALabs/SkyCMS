---
title: AWS Installation Guide
description: Deploy SkyCMS on AWS using S3 for static website hosting and blob storage
keywords: AWS, installation, S3, deployment, setup
audience: [developers, devops]
---

# AWS Installation Guide

Deploy SkyCMS on AWS using either the **interactive CDK deployment** (recommended) or the **manual S3-only setup**.

## Quick Start

### Recommended: Interactive CDK Deployment (Editor + optional Publisher + optional SES)
1. Prereqs: AWS CLI configured, Node.js 18+, npm, PowerShell, Docker running, permissions for CloudFormation/ECS/RDS/S3/CloudFront/Secrets Manager/ACM.
2. Run the script: `cd InstallScripts/AWS; ./cdk-deploy.ps1` and answer prompts.
3. Choose options:
	- Deploy Publisher (S3 + CloudFront) or not
	- Enable Amazon SES SMTP or not (password stored in Secrets Manager)
	- Custom domains (optional)
4. Review summary and deploy. Outputs include CloudFront URLs, DB info, and secrets ARNs.
5. Next: Run the Editor setup wizard, verify SES sender/recipients if SES was enabled.

Full guide: [InstallScripts/AWS/AWS_CDK_INTERACTIVE_DEPLOYMENT.md](InstallScripts/AWS/AWS_CDK_INTERACTIVE_DEPLOYMENT.md)

### Alternative: Manual S3-Only Setup
1. **AWS Account** - Ensure you have an active AWS account with S3 access
2. **S3 Bucket** - Create an S3 bucket for your site content
3. **Access Keys** - Generate IAM credentials with S3 permissions
4. **Configuration** - Update SkyCMS settings with your S3 details

See [AWS S3 Access Keys Setup](../Configuration/AWS-S3-AccessKeys.md) for detailed credential creation steps.

## Deployment Options

### Option 1: S3 Static Website Hosting (Manual)
- Enable static website hosting on your S3 bucket
- Use CloudFront or another CDN for edge distribution
- See [S3 Storage Configuration](../Configuration/Storage-S3.md) for setup details

### Option 2: Dynamic Hosting with S3 Storage (Manual)
- Deploy SkyCMS application to AWS (EC2, Lightsail, App Runner, etc.)
- Use S3 as your blob storage provider for media and assets
- See [Storage Configuration Reference](../Configuration/Storage-Configuration-Reference.md)

### Option 3: Interactive CDK Stack (Recommended)
- Deploys Editor (ECS + ALB + RDS + CloudFront)
- Optional Publisher (S3 + CloudFront) with storage secret injection
- Optional Amazon SES SMTP (env-driven; password via Secrets Manager)
- Secrets Manager used for DB creds and connection strings
- Single interactive script: [InstallScripts/AWS/cdk-deploy.ps1](../../InstallScripts/AWS/cdk-deploy.ps1)

## Related Documentation

- **[Minimum Required Settings](./MinimumRequiredSettings.md)** - Essential configuration for any deployment
- **[AWS S3 Access Keys](../Configuration/AWS-S3-AccessKeys.md)** - Step-by-step credential setup
- **[Storage Configuration Reference](../Configuration/Storage-Configuration-Reference.md)** - Complete S3 integration guide
- **[S3 Storage Overview](../Configuration/Storage-S3.md)** - S3-specific implementation details
