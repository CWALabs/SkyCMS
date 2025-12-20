# SkyCMS Publisher Deployment (S3 + CloudFront)

This guide explains how to optionally deploy an S3 bucket with CloudFront for serving static website content (Publisher) alongside your SkyCMS Editor.

## Quick Start

### Step 1: Deploy Publisher Stack (One-Time Setup)

```powershell
cd InstallScripts/AWS
$Region = "us-east-1"
$StackName = "SkyCmsPublisherStack"

aws cloudformation deploy `
  --stack-name $StackName `
  --template-file s3-with-cloudfront.yml `
  --capabilities CAPABILITY_IAM `
  --region $Region
```

### Step 2: Create IAM User for S3 Access

Get the S3 bucket name from the stack outputs:

```powershell
$BucketName = aws cloudformation describe-stacks `
  --stack-name $StackName `
  --region $Region `
  --query "Stacks[0].Outputs[?OutputKey=='WebsiteBucketName'].OutputValue" `
  --output text

Write-Host "S3 Bucket: $BucketName"
```

Create IAM user `skycms-s3-publisher-user`:

```powershell
$IamUser = "skycms-s3-publisher-user"

# Create user
aws iam create-user --user-name $IamUser

# Create policy document
$PolicyDocument = @"
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": [
        "s3:GetObject",
        "s3:PutObject",
        "s3:DeleteObject",
        "s3:ListBucket"
      ],
      "Resource": [
        "arn:aws:s3:::$BucketName",
        "arn:aws:s3:::$BucketName/*"
      ]
    }
  ]
}
"@

# Save to file
$PolicyDocument | Out-File -FilePath "$env:TEMP\s3-policy.json" -Encoding utf8

# Attach policy
aws iam put-user-policy `
  --user-name $IamUser `
  --policy-name "S3PublisherAccess" `
  --policy-document "file://$env:TEMP\s3-policy.json"

# Create access key
$AccessKey = aws iam create-access-key --user-name $IamUser --output json | ConvertFrom-Json
$AccessKeyId = $AccessKey.AccessKey.AccessKeyId
$SecretAccessKey = $AccessKey.AccessKey.SecretAccessKey

Write-Host "✅ IAM User Created"
Write-Host "Access Key ID: $AccessKeyId"
Write-Host "Secret Access Key: $SecretAccessKey (save this securely!)"
```

### Step 3: Store Connection String in Secrets Manager

```powershell
$StorageConnectionString = "Bucket=$BucketName;Region=$Region;KeyId=$AccessKeyId;Key=$SecretAccessKey;"
$SecretName = "SkyCms-StorageConnectionString"

# Create or update secret
aws secretsmanager create-secret `
  --name $SecretName `
  --description "S3 storage connection string for SkyCMS Publisher" `
  --secret-string $StorageConnectionString `
  --region $Region

# Get secret ARN
$StorageSecretArn = aws secretsmanager describe-secret `
  --secret-id $SecretName `
  --region $Region `
  --query ARN `
  --output text

Write-Host "✅ Secret Created"
Write-Host "Secret ARN: $StorageSecretArn"
```

### Step 4: Deploy Editor with Storage Connection

When deploying the Editor, pass the storage secret ARN as a context variable:

```powershell
cd InstallScripts/AWS/cdk

$Node = (Join-Path (Get-Location) "node_modules\aws-cdk\bin\cdk")
$AccountId = aws sts get-caller-identity --query Account --output text
$EditorStackName = "SkyCmsMinimalStack"

# Deploy with storage context
node $Node deploy $EditorStackName `
  --require-approval never `
  --context image="toiyabe/sky-editor:latest" `
  --context desiredCount=1 `
  --context dbName="skycms" `
  --context stackName=$EditorStackName `
  --context storageSecretArn=$StorageSecretArn `
  --region $Region
```

## Workflow Summary

```
┌─────────────────────────────────────────────┐
│ 1. Deploy Publisher Stack (S3 + CloudFront) │
│    aws cloudformation deploy ...             │
└─────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────┐
│ 2. Create IAM User for S3 Access            │
│    aws iam create-user ...                   │
│    aws iam create-access-key ...             │
└─────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────┐
│ 3. Store Connection String in Secrets Mgr   │
│    aws secretsmanager create-secret ...      │
└─────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────┐
│ 4. Deploy Editor with Storage Context       │
│    cdk deploy ... --context storageSecretArn=... │
└─────────────────────────────────────────────┘
```

## Architecture

With Publisher enabled:

```
                    ┌──────────────────┐
                    │   S3 Bucket      │
                    │  (Static Files)  │
                    └────────┬─────────┘
                             │
                    ┌────────▼──────────┐
                    │    CloudFront     │
                    │  (Publisher CDN)  │
                    └────────┬──────────┘
                             │
                    https://publisher.cloudfront.net
                             
                    ┌──────────────────┐
                    │   CloudFront     │
                    │  (Editor CDN)    │
                    └────────┬─────────┘
                             │
                    ┌────────▼──────────┐
                    │      ALB          │
                    └────────┬──────────┘
                             │
                    ┌────────▼──────────┐
                    │  ECS Fargate      │
                    │  (SkyCMS Editor)  │
                    └────────┬──────────┘
                             │
                    ┌────────▼──────────┐
                    │  RDS MySQL        │
                    │  (Database)       │
                    └───────────────────┘
```

## Editor Configuration

The Editor automatically reads `ConnectionStrings__StorageConnectionString` from the Secrets Manager secret and uses it to configure S3 storage.

In the Editor setup wizard, you'll see the S3 connection already configured via the environment variable injected from Secrets Manager.

## Managing S3 Access Keys

### Rotate Keys

```powershell
# 1. Create new access key
$NewAccessKey = aws iam create-access-key --user-name skycms-s3-publisher-user --output json | ConvertFrom-Json

# 2. Update secret with new credentials
$NewConnectionString = "Bucket=$BucketName;Region=$Region;KeyId=$($NewAccessKey.AccessKey.AccessKeyId);Key=$($NewAccessKey.AccessKey.SecretAccessKey);"

aws secretsmanager put-secret-value `
  --secret-id SkyCms-StorageConnectionString `
  --secret-string $NewConnectionString `
  --region $Region

# 3. Restart ECS task (will pick up new secret)
aws ecs update-service `
  --cluster SkyCmsMinimalStack-Cluster... `
  --service SkyCmsMinimalStack-Service... `
  --force-new-deployment `
  --region $Region

# 4. Delete old access key
aws iam delete-access-key `
  --user-name skycms-s3-publisher-user `
  --access-key-id OLD_KEY_ID
```

## Optional: Custom Domain for Publisher

To use a custom domain like `www.example.com` for the Publisher CloudFront distribution:

```powershell
# Redeploy Publisher stack with custom domain parameters
aws cloudformation deploy `
  --stack-name SkyCmsPublisherStack `
  --template-file s3-with-cloudfront.yml `
  --capabilities CAPABILITY_IAM `
  --region $Region `
  --parameter-overrides `
    DomainName="www.example.com" `
    HostedZoneId="Z1234567890ABC" `
    ACMCertificateArn="arn:aws:acm:us-east-1:123456789012:certificate/12345678-1234-1234-1234-123456789012"
```

**Requirements:**
- Route 53 hosted zone for your domain
- ACM certificate in `us-east-1` region (CloudFront requirement)

## Troubleshooting

### S3 Access Denied

Verify IAM policy is correct:
```powershell
aws iam get-user-policy `
  --user-name skycms-s3-publisher-user `
  --policy-name S3PublisherAccess
```

### CloudFront Not Propagating

CloudFront distributions typically take 1-5 minutes to fully propagate. Check status:

```powershell
$DistributionId = aws cloudformation describe-stacks `
  --stack-name SkyCmsPublisherStack `
  --region $Region `
  --query "Stacks[0].Outputs[?OutputKey=='CloudFrontDistributionId'].OutputValue" `
  --output text

aws cloudfront get-distribution-config --id $DistributionId
```

### Editor Not Reading Storage Connection

Verify the secret exists and is accessible:

```powershell
aws secretsmanager get-secret-value `
  --secret-id SkyCms-StorageConnectionString `
  --region $Region

# Check ECS task environment
aws ecs describe-task-definition `
  --task-definition SkyCmsMinimalStack-TaskDef... `
  --query "taskDefinition.containerDefinitions[0].secrets"
```

## Cleanup

To remove Publisher deployment:

```powershell
# Delete CloudFormation stack
aws cloudformation delete-stack --stack-name SkyCmsPublisherStack --region $Region

# Delete IAM user and access keys
aws iam list-access-keys --user-name skycms-s3-publisher-user
aws iam delete-access-key --user-name skycms-s3-publisher-user --access-key-id KEY_ID
aws iam delete-user-policy --user-name skycms-s3-publisher-user --policy-name S3PublisherAccess
aws iam delete-user --user-name skycms-s3-publisher-user

# Delete Secrets Manager secret
aws secretsmanager delete-secret `
  --secret-id SkyCms-StorageConnectionString `
  --force-delete-without-recovery `
  --region $Region
```

## Next Steps

- [Storage-S3.md](../../Docs/Configuration/Storage-S3.md) — Detailed S3 configuration guide
- [CDK_DEPLOYMENT_GUIDE.md](./CDK_DEPLOYMENT_GUIDE.md) — Editor deployment details
- [AWS S3 Access Keys Guide](../../Docs/Configuration/AWS-S3-AccessKeys.md) — Manual IAM setup
