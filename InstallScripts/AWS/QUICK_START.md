# Quick Start: Interactive Deployment

## TL;DR - Deploy in 3 Steps

### Step 1: Authenticate to AWS
```powershell
aws login
# Browser opens for authentication
```

### Step 2: Bootstrap CDK (one-time per account/region)
```powershell
cd D:\source\SkyCMS\InstallScripts\AWS\cdk
.\node_modules\.bin\cdk.cmd bootstrap aws://873764251532/us-east-1 `
  --qualifier hnb659fds `
  --cloudformation-execution-policies arn:aws:iam::aws:policy/AdministratorAccess
```

### Step 3: Run Deployment Script
```powershell
cd ..
.\cdk-deploy.ps1
```

## What You'll Be Asked

```
📋 EDITOR CONFIGURATION:
   AWS Region [us-east-1]: 
   Docker Image [toiyabe/sky-editor:latest]: 
   Desired Task Count [1]: 
   Database Name [skycms]: 
   Stack Name [skycms-editor]: 
   Domain Name (optional): 
   (optional) Hosted Zone ID: 

❓ PUBLISHER OPTION:
   Deploy Publisher (S3 + CloudFront)? (y/n) [Y]: 
   
   (if yes)
   Stack Name [skycms-publisher]: 
   Domain Name (optional): 
   (optional) Hosted Zone ID: 

✅ CONFIRMATION:
   Continue with deployment? (y/n) [Y]:
```

## What Gets Deployed

### If You Choose Publisher (S3+CloudFront):
- ✅ S3 bucket for website files
- ✅ CloudFront CDN distribution
- ✅ IAM user with S3 access (`skycms-s3-publisher-user`)
- ✅ Access keys stored securely
- ✅ Storage connection string in AWS Secrets Manager

### Always (Editor):
- ✅ ECS cluster + Fargate tasks
- ✅ RDS MySQL database
- ✅ Application Load Balancer
- ✅ CloudFront CDN distribution
- ✅ Auto-generated TLS certificates

## Expected Output

```
✅ DEPLOYMENT COMPLETE!

📦 PUBLISHER (S3 + CloudFront):
   CloudFront URL: https://d123abc.cloudfront.net
   S3 Bucket: skycms-publisher-bucket-xxx
   IAM User: skycms-s3-publisher-user

📝 EDITOR (ECS + RDS + CloudFront):
   CloudFront URL: https://d456def.cloudfront.net
   Database: skycms @ skycms-rds-xxx.us-east-1.rds.amazonaws.com
   Stack: skycms-editor
```

## After Deployment

1. **Wait 1-2 minutes** for CloudFront to propagate
2. **Visit your Editor URL** from output above
3. **Run setup wizard** and configure SkyCMS
4. **Upload Publisher files** (if deployed):
   ```powershell
   aws s3 sync ./website s3://skycms-publisher-bucket-xxx/
   ```

## Troubleshooting

| Problem | Solution |
|---------|----------|
| Script hangs | Check AWS credentials: `aws sts get-caller-identity` |
| CloudFormation fails | Verify IAM permissions for CloudFormation, S3, CloudFront |
| URL not working | Wait 1-2 minutes for CloudFront propagation |
| S3 upload fails | Verify IAM user has S3 policy attached |
| Database not accessible | Run script again to auto-add your IP to security group |

## Key Points

- **All prompts have defaults** — just press Enter to accept
- **No command-line flags** — everything is interactive
- **Publisher is optional** — answer 'n' to skip S3 deployment
- **Custom domains** are optional — CloudFront URLs work without them
- **Everything is automatic** — IAM users, keys, secrets are created for you
- **Storage is connected** — Editor automatically gets S3 access via Secrets Manager

## Files to Review

- `AWS_CDK_INTERACTIVE_DEPLOYMENT.md` — Full documentation
- `cdk-deploy.ps1` — The deployment script (362 lines, well-commented)
- `s3-with-cloudfront.yml` — Publisher CloudFormation template

## Questions?

See `AWS_CDK_INTERACTIVE_DEPLOYMENT.md` for:
- Detailed feature explanations
- Troubleshooting guide
- Security notes
- Environment variable setup
