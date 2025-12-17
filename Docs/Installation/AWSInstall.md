---
title: AWS Installation Guide
description: Deploy SkyCMS on AWS using S3 for static website hosting and blob storage
keywords: AWS, installation, S3, deployment, setup
audience: [developers, devops]
---

# AWS Installation Guide

Deploy SkyCMS on AWS using S3 for static website hosting or as your blob storage provider.

## Quick Start

1. **AWS Account** - Ensure you have an active AWS account with S3 access
2. **S3 Bucket** - Create an S3 bucket for your site content
3. **Access Keys** - Generate IAM credentials with S3 permissions
4. **Configuration** - Update SkyCMS settings with your S3 details

See [AWS S3 Access Keys Setup](../Configuration/AWS-S3-AccessKeys.md) for detailed credential creation steps.

## Deployment Options

### Option 1: S3 Static Website Hosting
- Enable static website hosting on your S3 bucket
- Use CloudFront or another CDN for edge distribution
- See [S3 Storage Configuration](../Configuration/Storage-S3.md) for setup details

### Option 2: Dynamic Hosting with S3 Storage
- Deploy SkyCMS application to AWS (EC2, Lightsail, App Runner, etc.)
- Use S3 as your blob storage provider for media and assets
- See [Storage Configuration Reference](../Configuration/Storage-Configuration-Reference.md)

## Related Documentation

- **[Minimum Required Settings](./MinimumRequiredSettings.md)** - Essential configuration for any deployment
- **[AWS S3 Access Keys](../Configuration/AWS-S3-AccessKeys.md)** - Step-by-step credential setup
- **[Storage Configuration Reference](../Configuration/Storage-Configuration-Reference.md)** - Complete S3 integration guide
- **[S3 Storage Overview](../Configuration/Storage-S3.md)** - S3-specific implementation details
