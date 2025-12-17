---
title: Hosting Static Website on AWS S3
description: Step-by-step guide for hosting a static website using Amazon S3 with CloudFormation templates
keywords: AWS, S3, static-website, CloudFormation, hosting, deployment
audience: [developers, devops]
---

# Hosting a Static Website on AWS S3

This guide provides step-by-step instructions for hosting a static website using Amazon S3 and includes CloudFormation templates for automated deployment.

## Overview

Amazon S3 can host static websites that include HTML, CSS, JavaScript, and other static assets. This setup is cost-effective, scalable, and easy to maintain.

## Prerequisites

- AWS Account
- AWS CLI installed and configured
- Basic understanding of HTML/CSS/JavaScript
- CloudFormation knowledge (optional)

## Manual Setup Steps

### 1. Create an S3 Bucket

1. Open the AWS S3 console
2. Click "Create bucket"
3. Enter a unique bucket name (must match your domain if using custom domain)
4. Choose your preferred region
5. Keep default settings and create the bucket

### 2. Configure Static Website Hosting

1. Select your bucket
2. Go to "Properties" tab
3. Scroll to "Static website hosting"
4. Select "Enable"
5. Set index document: `index.html`
6. Set error document: `error.html` (optional)
7. Save changes

### 3. Set Bucket Policy for Public Access

1. Go to "Permissions" tab
2. Edit "Bucket policy"
3. Add the following policy (replace `your-bucket-name`):

```json
{
    "Version": "2012-10-17",
    "Statement": [
        {
            "Sid": "PublicReadGetObject",
            "Effect": "Allow",
            "Principal": "*",
            "Action": "s3:GetObject",
            "Resource": "arn:aws:s3:::your-bucket-name/*"
        }
    ]
}
```

### 4. Configure SkyCMS for S3 Publishing

1. In your SkyCMS configuration, set the publishing target to S3
2. Configure your AWS credentials and bucket details in SkyCMS settings
3. Set the bucket name and region in the publishing configuration
4. Enable automatic deployment to push content updates directly to S3
5. SkyCMS will handle the upload and synchronization of your static files

For more information on configuring Sky storage, see 
[Storage configuration reference](./Configuration/Storage-Configuration-Reference.md).

## CloudFormation Templates

### Basic S3 Static Website

```yaml
AWSTemplateFormatVersion: '2010-09-09'
Description: 'Static website hosting with S3'

Parameters:
  BucketName:
    Type: String
    Description: Name of the S3 bucket for hosting
    Default: my-static-website-bucket

Resources:
  S3Bucket:
    Type: AWS::S3::Bucket
    Properties:
      BucketName: !Ref BucketName
      WebsiteConfiguration:
        IndexDocument: index.html
        ErrorDocument: error.html
      PublicAccessBlockConfiguration:
        BlockPublicAcls: false
        BlockPublicPolicy: false
        IgnorePublicAcls: false
        RestrictPublicBuckets: false

  S3BucketPolicy:
    Type: AWS::S3::BucketPolicy
    Properties:
      Bucket: !Ref S3Bucket
      PolicyDocument:
        Statement:
          - Sid: PublicReadGetObject
            Effect: Allow
            Principal: '*'
            Action: 's3:GetObject'
            Resource: !Sub '${S3Bucket}/*'

Outputs:
  WebsiteURL:
    Description: URL of the static website
    Value: !GetAtt S3Bucket.WebsiteURL
  BucketName:
    Description: Name of the S3 bucket
    Value: !Ref S3Bucket
```

### Advanced Setup with CloudFront CDN

```yaml
AWSTemplateFormatVersion: '2010-09-09'
Description: 'Static website with S3 and CloudFront CDN'

Parameters:
  BucketName:
    Type: String
    Description: Name of the S3 bucket for hosting
    Default: my-static-website-bucket

Resources:
  S3Bucket:
    Type: AWS::S3::Bucket
    Properties:
      BucketName: !Ref BucketName
      WebsiteConfiguration:
        IndexDocument: index.html
        ErrorDocument: error.html
      PublicAccessBlockConfiguration:
        BlockPublicAcls: false
        BlockPublicPolicy: false
        IgnorePublicAcls: false
        RestrictPublicBuckets: false

  S3BucketPolicy:
    Type: AWS::S3::BucketPolicy
    Properties:
      Bucket: !Ref S3Bucket
      PolicyDocument:
        Statement:
          - Sid: PublicReadGetObject
            Effect: Allow
            Principal: '*'
            Action: 's3:GetObject'
            Resource: !Sub '${S3Bucket}/*'

  CloudFrontDistribution:
    Type: AWS::CloudFront::Distribution
    Properties:
      DistributionConfig:
        Origins:
          - DomainName: !GetAtt S3Bucket.DomainName
            Id: S3Origin
            CustomOriginConfig:
              HTTPPort: 80
              HTTPSPort: 443
              OriginProtocolPolicy: http-only
        Enabled: true
        DefaultRootObject: index.html
        DefaultCacheBehavior:
          TargetOriginId: S3Origin
          ViewerProtocolPolicy: redirect-to-https
          AllowedMethods:
            - GET
            - HEAD
          Compress: true
          ForwardedValues:
            QueryString: false
            Cookies:
              Forward: none
        PriceClass: PriceClass_100
        CustomErrorResponses:
          - ErrorCode: 404
            ResponseCode: 200
            ResponsePagePath: /index.html

Outputs:
  WebsiteURL:
    Description: URL of the static website
    Value: !GetAtt S3Bucket.WebsiteURL
  CloudFrontURL:
    Description: CloudFront distribution URL
    Value: !Sub 'https://${CloudFrontDistribution.DomainName}'
  BucketName:
    Description: Name of the S3 bucket
    Value: !Ref S3Bucket
```

## Deployment Commands

### Deploy Basic Template

```bash
aws cloudformation create-stack \
  --stack-name static-website-stack \
  --template-body file://basic-s3-website.yaml \
  --parameters ParameterKey=BucketName,ParameterValue=your-unique-bucket-name
```

### Deploy Advanced Template

```bash
aws cloudformation create-stack \
  --stack-name static-website-with-cdn-stack \
  --template-body file://advanced-s3-website.yaml \
  --parameters ParameterKey=BucketName,ParameterValue=your-unique-bucket-name
```

## Best Practices

1. **Use CloudFront** for better performance and caching
2. **Enable versioning** on your S3 bucket for backup
3. **Use HTTPS** always (CloudFront provides free SSL certificates)
4. **Optimize images** and assets for web delivery
5. **Set appropriate cache headers** for static assets
6. **Monitor costs** using AWS Cost Explorer
7. **Use Route 53** for custom domain management

## Security Considerations

- Only grant public read access to necessary objects
- Use CloudFront for additional security features
- Consider AWS WAF for advanced protection
- Regularly review bucket policies and permissions

## Cost Optimization

- Use CloudFront to reduce S3 data transfer costs
- Implement lifecycle policies for old versions
- Choose appropriate S3 storage classes
- Monitor usage with AWS Cost Explorer
