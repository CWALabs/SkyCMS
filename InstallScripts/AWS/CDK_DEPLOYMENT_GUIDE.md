# SkyCMS Editor CDK Deployment Guide (Minimalist Edition)

This guide walks you through deploying SkyCMS Editor on AWS using AWS CDK. This is a **first-step, minimal configuration** focusing on getting a working deployment without complex security features yet.

## December 2025 Update (Proxy Headers & Debug Mode)

- CloudFront now forwards critical headers to ALB via a custom **Origin Request Policy** (`Host`, `CloudFront-Forwarded-Proto`, `User-Agent`). This fixes HTTP 400 errors on the setup wizard POST caused by antiforgery validation when the original HTTPS scheme was not preserved.
- The Editor application includes middleware that maps `CloudFront-Forwarded-Proto` to `X-Forwarded-Proto` before `UseForwardedHeaders()` runs, ensuring ASP.NET Core sees the original HTTPS scheme even when CloudFront → ALB uses HTTP.
- **Optional**: For end-to-end TLS, you can provide an ACM certificate ARN or auto-provision one via Route 53 using `-DomainName`, `-HostedZoneId`, and `-HostedZoneName` parameters. This enables HTTPS from CloudFront → ALB and removes the need for the header mapping middleware.
- No teardown required; run the regular deployment to apply this change: `./cdk-deploy.ps1`.
- For troubleshooting, the ECS containers are temporarily configured with `ASPNETCORE_ENVIRONMENT=Development`. Remember to revert to `Production` once debugging is complete.
- The database connection string is now generated dynamically from the **RDS endpoint** and **Secrets Manager** credentials (no Azure endpoints or hardcoded strings).

## What Gets Deployed

- ✅ **Docker Container**: `toiyabe/sky-editor:latest` from Docker Hub
- ✅ **ECS Fargate**: 512 CPU / 1 GB RAM, 1 task running behind an ALB
- ✅ **RDS MySQL 8.0**: `db.t4g.micro`, 20 GB storage, accessible from all IPs (temporary)
- ✅ **CloudFront HTTPS**: Public endpoint with automatic TLS cert
- ✅ **S3 Integration**: ECS task role has full S3 access to your bucket
- ✅ **Single Tenant Setup**: `CosmosAllowSetup=true`, `MultiTenantEditor=false`

## Prerequisites

### 1. Install Node.js
Download and install **Node.js LTS** from https://nodejs.org/

Verify installation:
```powershell
node --version
npm --version
```

### 2. Install AWS CLI v2
Download from https://aws.amazon.com/cli/

Configure your AWS credentials:
```powershell
aws configure
```

You'll be prompted for:
- **AWS Access Key ID**: (from your AWS account)
- **AWS Secret Access Key**: (from your AWS account)
- **Default region**: `us-east-1` (or your preferred region)
- **Output format**: `json`

Verify installation:
```powershell
aws sts get-caller-identity
```

### 3. Get Your S3 Bucket Name
If you've already deployed the static website stack (`s3-with-cloudfront.yml`), get the bucket name:

```powershell
aws cloudformation describe-stacks `
  --stack-name skycms-static-site `
  --region us-east-1 `
  --query "Stacks[0].Outputs[?OutputKey=='WebsiteBucketName'].OutputValue" `
  --output text
```

If you haven't created the S3 bucket yet, first deploy `s3-with-cloudfront.yml`:
```powershell
# Deploy static site first
.\deploy-static-site.ps1 -StackName "skycms-static-site" -Region "us-east-1"
```

## Deployment Steps

### Step 1: Navigate to the CDK Directory
```powershell
cd d:\source\SkyCMS\InstallScripts\AWS
```

### Step 2: Run the Deployment Script
```powershell
.\cdk-deploy.ps1 `
  -BucketName "your-s3-bucket-name-here" `
  -Image "toiyabe/sky-editor:latest" `
  -DbName "skycms" `
  -DesiredCount 1 `
  -Region "us-east-1"
```

**Replace `your-s3-bucket-name-here` with the actual bucket name from Step 3 above.**

### Step 3: Wait for Deployment
⏱️ **Expected time: 15–20 minutes** (RDS provisioning is the longest part)

The script will:
1. Install npm dependencies
2. Bootstrap CDK (first time only)
3. Synthesize the CloudFormation template
4. Deploy to AWS

When complete, you'll see output like:
```
=====================================
✅ Deployment Complete!
=====================================

Outputs:
EditorURL = https://d1234abcd.cloudfront.net
DatabaseEndpoint = skycms-editor-mysql-xyz.c1234.us-east-1.rds.amazonaws.com
DatabasePort = 3306
DatabaseUsername = skycms_admin
DatabasePassword = SkyCMS2025!Temp
DatabaseName = skycms
MySqlConnectionString = Server=skycms-editor-mysql-xyz.c1234.us-east-1.rds.amazonaws.com;Port=3306;Database=skycms;Uid=skycms_admin;Pwd=SkyCMS2025!Temp;
S3BucketName = your-s3-bucket-name
S3Region = us-east-1
```

**Save these outputs!** You'll need them for the setup wizard.

## Access the Editor Setup Wizard

### 1. Open the Editor URL
Copy the `EditorURL` from the deployment outputs and open it in your browser:
```
https://d1234abcd.cloudfront.net
```

If you get a **502 Bad Gateway**, wait 2–3 minutes for the ECS task to fully start, then refresh.

### 2. Complete the Setup Wizard

Fill in each section:

#### Database Section
- **Host**: (copy from `DatabaseEndpoint` output)
- **Port**: `3306`
- **Database**: `skycms` (or the `DatabaseName` from outputs)
- **Username**: `skycms_admin` (or `DatabaseUsername` from outputs)
- **Password**: `SkyCMS2025!Temp` (or `DatabasePassword` from outputs)

#### S3 Storage Section
- **Provider**: `Amazon S3`
- **Bucket Name**: (copy from `S3BucketName` output)
- **Region**: `us-east-1` (or `S3Region` from outputs)
- **Access Key ID**: (leave blank—IAM role is used)
- **Secret Access Key**: (leave blank—IAM role is used)

#### Administrator Account
- **Email**: (your email)
- **Username**: (your admin username)
- **Password**: (strong password, min 8 chars)

#### Publisher Settings (optional)
- **URL**: (your domain or empty for now)
- **Email**: (support email)

Click **Save** to complete setup.

## Verify Database Access (MySQL Workbench)

You can connect to the database using MySQL Workbench or another client:

1. Download and install **MySQL Workbench** from https://www.mysql.com/products/workbench/
2. Create a new connection:
   - **Connection Name**: SkyCMS Editor
   - **Hostname**: (copy from `DatabaseEndpoint` output)
   - **Port**: `3306`
   - **Username**: `skycms_admin`
   - **Password**: `SkyCMS2025!Temp`
3. Click **Test Connection** to verify
4. Query the `skycms` database to see tables created by the wizard

Or use the **MySqlConnectionString** directly:
```
Server=skycms-editor-mysql-xyz.c1234.us-east-1.rds.amazonaws.com;Port=3306;Database=skycms;Uid=skycms_admin;Pwd=SkyCMS2025!Temp;
```

## Cost Monitoring

### Estimated Monthly Costs
- **RDS MySQL (db.t4g.micro)**: ~$8–12
- **ECS Fargate (512/1024)**: ~$30–35
- **ALB**: ~$15
- **CloudFront**: ~$0.085/GB (usage-based)
- **Data Transfer**: Variable

**Total minimum**: ~$50–60/month (not counting data transfer)

### Cost-Saving Tips
- **Delete when not in use**: Run `cdk-destroy.ps1` to save ~$50–60/month
- **Tear down overnight**: Schedule deletion if only needed during work hours
- **Reduce task count**: Set `DesiredCount 0` to pause without deleting

## Destroy/Cleanup

To delete all resources and stop incurring costs:

```powershell
.\cdk-destroy.ps1 -Region "us-east-1"
```

**Warning**: This will delete the RDS database, ECS cluster, ALB, and CloudFront distribution. Data in S3 will remain.

## Troubleshooting

### "502 Bad Gateway" from CloudFront
- **Cause**: ECS task hasn't started yet
- **Fix**: Wait 2–3 minutes and refresh the browser

### "Can't connect to database" in setup wizard
1. Verify RDS is fully provisioned:
   ```powershell
   aws rds describe-db-instances --query "DBInstances[0].DBInstanceStatus"
   ```
   Should show `available`

2. Check database was created:
   ```powershell
   aws rds describe-db-instances --query "DBInstances[0].DBName"
   ```
   Should show `skycms`

### RDS Security Group Not Allowing Access
The stack allows all IPs (0.0.0.0/0) to port 3306 for dev convenience. If you can't connect:

1. Check the security group was created:
   ```powershell
   aws ec2 describe-security-groups --filters "Name=tag:Name,Values=*DbSg*"
   ```

2. Verify the ingress rule:
   ```powershell
   aws ec2 describe-security-group-rules --filters "Name=group-id,Values=sg-xxxxx"
   ```

### Docker Image Pull Fails
- Verify the image exists on Docker Hub: `docker pull toiyabe/sky-editor:latest`
- Check ECS task logs in CloudWatch:
  ```powershell
  aws logs tail /ecs/SkyCmsEditorStack-EditorService --follow
  ```

### Deployment Hangs
### HTTP 400 on Setup Wizard (POST)
- **Cause**: ASP.NET Core antiforgery sees a scheme mismatch if `X-Forwarded-Proto` does not reflect the original HTTPS when traffic flows CloudFront (HTTPS) → ALB (HTTP) → ECS.
- **Fix (already applied)**: 
  - CDK configures CloudFront with an **Origin Request Policy** that forwards `Host`, `CloudFront-Forwarded-Proto`, and `User-Agent` to ALB.
  - The Editor application includes middleware that copies `CloudFront-Forwarded-Proto` (set by CloudFront when viewer uses HTTPS) to `X-Forwarded-Proto` before `UseForwardedHeaders()` middleware runs.
  - This ensures ASP.NET Core's antiforgery validation and OAuth redirects see the original HTTPS scheme.
- **Optional end-to-end TLS**: Deploy with an ACM certificate to enable HTTPS from CloudFront → ALB, eliminating the need for header mapping:
  ```powershell
  ./cdk-deploy.ps1 -DomainName "editor.example.com" -HostedZoneName "example.com" -HostedZoneId "Z123456ABCDEFG"
  ```
  Or use an existing certificate:
  ```powershell
  ./cdk-deploy.ps1 -CertificateArn "arn:aws:acm:us-east-1:ACCOUNT:certificate/XXXXXXXX"
  ```
- **Action**: Redeploy with `./cdk-deploy.ps1`. No stack destroy is needed.
- **References**: [AWS CloudFront Custom Headers](https://docs.aws.amazon.com/AmazonCloudFront/latest/DeveloperGuide/add-origin-custom-headers.html), [Origin Request Policies](https://docs.aws.amazon.com/AmazonCloudFront/latest/DeveloperGuide/origin-request-policies.html)
- CDK can take a while on first deployment. Check CloudFormation events:
  ```powershell
  aws cloudformation describe-stack-events `
    --stack-name SkyCmsEditorStack `
    --query "StackEvents[0:10]"
  ```

## Next Steps (Phase 2 Enhancements)

Once you confirm the minimalist setup works, we can add:

1. ✅ **TLS to RDS**: Encrypted in-transit connection (RDS require_secure_transport)
2. ✅ **Secrets Manager**: Store DB password securely (not in env vars)
3. ✅ **IAM Database Auth**: Temporary credentials instead of static password
4. ✅ **Better Monitoring**: CloudWatch dashboards, alarms
5. ✅ **Auto-scaling**: Scale ECS tasks based on CPU/memory demand

For now, focus on verifying this basic deployment works!

---

**Questions?** Check the [AWS CDK Documentation](https://docs.aws.amazon.com/cdk/latest/guide/) or review the TypeScript code in `cdk/lib/skycms-stack.ts`.

**Last Updated**: December 2025
