# SkyCMS Editor AWS CDK - Implementation Summary

## What's Been Built

A **minimalist, single-step AWS CDK deployment** for SkyCMS Editor that focuses on simplicity and getting to a working proof-of-concept quickly.

## Files Created/Modified

### CDK Stack Implementation
- **[cdk/lib/skycms-stack.ts](cdk/lib/skycms-stack.ts)** — TypeScript CDK stack with:
  - VPC (2 AZs, public + isolated subnets, no NAT)
  - RDS MySQL 8.0 (db.t4g.micro, 20 GB GP3, 7-day backups)
  - ECS Fargate (512 CPU / 1 GB, `toiyabe/sky-editor:latest`)
  - Application Load Balancer
  - CloudFront distribution (no caching, HTTPS)
  - S3 bucket IAM access for ECS task role
  - All DB credentials in environment variables (no Secrets Manager)

### Deployment Scripts
- **[cdk-deploy.ps1](cdk-deploy.ps1)** — Main deployment script
  - Installs dependencies (npm ci)
  - Bootstraps CDK (first-time setup)
  - Deploys stack and outputs results
  - Shows next steps
  
- **[cdk-destroy.ps1](cdk-destroy.ps1)** — Cleanup script
  - Tears down all resources to save costs

### Configuration
- **[cdk/package.json](cdk/package.json)** — npm dependencies (aws-cdk-lib v2, TypeScript)
- **[cdk/tsconfig.json](cdk/tsconfig.json)** — TypeScript compiler config
- **[cdk/cdk.json](cdk/cdk.json)** — CDK app entry point and context (image, bucket, dbName, desiredCount)
- **[cdk/bin/skycms.ts](cdk/bin/skycms.ts)** — App initialization (reads context, creates stack)

### Documentation
- **[CDK_DEPLOYMENT_GUIDE.md](CDK_DEPLOYMENT_GUIDE.md)** — Complete step-by-step guide
  - Prerequisites (Node.js, AWS CLI)
  - Deployment steps
  - Setup wizard walkthrough
  - MySQL Workbench access
  - Cost estimates
  - Troubleshooting

## Key Design Decisions (Minimalist Approach)

| Feature | Decision | Rationale |
|---------|----------|-----------|
| **Docker Image** | `toiyabe/sky-editor:latest` | Public Docker Hub image, no private registry complexity |
| **RDS Password** | Fixed string: `SkyCMS2025!Temp` | Dev-only; simplifies output and MySQL Workbench access |
| **DB Credentials** | Plain environment variables | No Secrets Manager; easier to verify database connection |
| **S3 Access** | IAM task role with S3 policy | No explicit keys needed; setup wizard skips S3 auth |
| **RDS Access** | Allow 0.0.0.0/0 to port 3306 | Temporary dev convenience; allows MySQL Workbench from anywhere |
| **NAT Gateway** | None | Saves ~$32/month; Fargate tasks get public IPs |
| **RDS Deletion** | `removalPolicy: DESTROY` | Dev-only; avoids snapshot waits on teardown |
| **Caching** | CACHING_DISABLED (CloudFront) | Ensures setup wizard sees live DB changes |

## Deployment Flow

```
User runs: .\cdk-deploy.ps1 -BucketName "xxx"
    ↓
npm ci (install deps)
    ↓
cdk bootstrap (prepare AWS account)
    ↓
cdk synth (generate CloudFormation)
    ↓
cdk deploy (create resources)
    ↓
Outputs displayed (EditorURL, DB endpoint, password, S3 bucket)
    ↓
User opens EditorURL in browser
    ↓
Setup wizard fills in DB + S3 + admin account
    ↓
✅ SkyCMS Editor ready!
```

## Environment Variables Injected into ECS Task

```
CosmosAllowSetup=true
MultiTenantEditor=false
ASPNETCORE_ENVIRONMENT=Production
BlobServiceProvider=Amazon
AmazonS3BucketName=<your-bucket>
AmazonS3Region=us-east-1
SKYCMS_DB_HOST=<rds-endpoint>
SKYCMS_DB_USER=skycms_admin
SKYCMS_DB_NAME=skycms
SKYCMS_DB_PASSWORD=SkyCMS2025!Temp
```

## Outputs After Deployment

```
EditorURL                 → https://d1234abcd.cloudfront.net
DatabaseEndpoint          → skycms-editor-mysql-xyz.c1234.us-east-1.rds.amazonaws.com
DatabasePort              → 3306
DatabaseUsername          → skycms_admin
DatabasePassword          → SkyCMS2025!Temp
DatabaseName              → skycms
MySqlConnectionString     → Server=...; (ready for MySQL Workbench)
S3BucketName              → your-bucket-name
S3Region                  → us-east-1
```

## Cost & Operational Notes

### Estimated Monthly Cost
- **RDS** (db.t4g.micro): ~$10
- **ECS** (Fargate 512/1024): ~$30
- **ALB**: ~$15
- **CloudFront**: ~$0.085/GB (pay-per-use)
- **Total minimum**: ~$55/month

### Duration of Deployment
- **First run** (~20 min): CDK bootstrap + RDS provision (slow)
- **Subsequent runs** (~15 min): Just stack updates + RDS

### Temporary (Dev-Only) Settings to Tighten Later
- ⚠️ RDS password in env vars (use Secrets Manager in Phase 2)
- ⚠️ RDS allows all IPs (restrict to ECS SG in Phase 2)
- ⚠️ No TLS to RDS (add `require_secure_transport` in Phase 2)
- ⚠️ `removalPolicy: DESTROY` on RDS (add snapshots for prod)

## Next Steps (Phase 2)

Once you confirm this works, enhancements include:

1. **TLS to RDS**: Add `require_secure_transport=1` to RDS parameter group
2. **Secrets Manager**: Store DB password; inject via task secrets
3. **Restrict RDS Access**: Allow only ECS SG, not 0.0.0.0/0
4. **CloudWatch Logs**: Set retention policy, create dashboards
5. **Auto-scaling**: Add Application Auto Scaling for ECS
6. **Monitoring**: CloudWatch alarms for health, CPU, disk

## How to Run

```powershell
# Step 1: Get S3 bucket name
$bucket = aws cloudformation describe-stacks `
  --stack-name skycms-static-site `
  --query "Stacks[0].Outputs[?OutputKey=='WebsiteBucketName'].OutputValue" `
  --output text

# Step 2: Deploy
cd d:\source\SkyCMS\InstallScripts\AWS
.\cdk-deploy.ps1 -BucketName $bucket -Image "toiyabe/sky-editor:latest" -Region "us-east-1"

# Step 3: Wait for outputs, open EditorURL, complete setup wizard

# Step 4: Verify database (optional)
# Use MySqlConnectionString in MySQL Workbench

# Step 5: Cleanup when done
.\cdk-destroy.ps1 -Region "us-east-1"
```

---

**Status**: ✅ Minimalist CDK deployment ready for first-step testing  
**Next Phase**: Security & observability enhancements  
**Last Updated**: December 2025
