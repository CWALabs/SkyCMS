# AWS CloudFormation Launch Stack - Quick Start Guide

**Estimated Setup Time:** 15-20 minutes (RDS creation is slower)  
**Cost Estimate:** $30-50/month for dev environment

---

## 📋 Deployment Timeline

### During Deployment (15-20 minutes)

Your CloudFormation stack is creating resources in order:

| Phase | Duration | What's Happening |
|-------|----------|------------------|
| **VPC & Networking** | 2-3 min | Creating VPC, subnets, security groups, internet gateway |
| **RDS MySQL Setup** | 8-12 min | Creating database instance (this is the slowest part) |
| **ECS Cluster** | 2-3 min | Creating ECS cluster and task definition |
| **Load Balancer** | 2-3 min | Creating ALB and target group |
| **CloudFront** | 1-2 min | Creating CloudFront distribution |
| **Service Launch** | 1 min | Starting ECS tasks |

**Total:** Typically 15-20 minutes (can be faster if you're lucky)

### After Deployment Completes

1. **Get Your Deployment Outputs**
   - Go to CloudFormation Console → Your Stack → Outputs tab
   - Copy `CloudFrontDomainName` (this is your access URL)
   - Copy `DatabaseSecretArn` and `DatabaseEndpoint` for reference

2. **Wait for ECS Tasks to Start**
   - Even after CloudFormation finishes, ECS tasks take 2-3 minutes to pull the Docker image and start
   - Check: CloudFormation → Stack → Resources → ECS Service → View Details
   - Status should change from "PROVISIONING" → "ACTIVE"

3. **Access the Setup Wizard**
   - Navigate to: `https://<CloudFrontDomainName>`
   - You should see the SkyCMS Editor setup wizard
   - If you get a 502 error, wait 1-2 more minutes for ECS tasks to fully start

---

## ⚙️ SkyCMS Editor Configuration

### 1. Complete the Setup Wizard

Navigate to the CloudFront URL and fill in:

**Database Configuration (Pre-configured):**
- Host: From Outputs tab → `DatabaseEndpoint`
- Port: `3306`
- Database: `skycms` (or your custom name)
- Username: `skycms_admin`
- Password: 
  - Go to AWS Secrets Manager → Your Stack Secret
  - Click "Retrieve Secret Value" to copy password
  - Or get from CloudFormation Outputs → `DatabaseSecretArn`

**Admin Account:**
- Email: Your email address
- Password: Create a strong password

**Storage Configuration (Optional):**
- For now, you can skip S3 storage
- Publisher will use local storage temporarily
- Set this up later when ready

**Email Configuration (Optional):**
- Skip for now, configure later in Editor settings
- Supports SES, SendGrid, or SMTP

### 2. Log In

Once wizard completes:
- Use your email and password to log in
- You should see the article/page management interface

---

## 🛠️ Post-Deployment Setup

### Enable Static Website Publishing

**Before Publisher can write to S3:**

1. Create an S3 bucket for Publisher output
   ```bash
   aws s3 mb s3://skycms-publisher-<random> --region us-east-1
   ```

2. Create IAM user with S3 permissions
   ```bash
   aws iam create-user --user-name skycms-publisher
   aws iam attach-user-policy --user-name skycms-publisher \
     --policy-arn arn:aws:iam::aws:policy/AmazonS3FullAccess
   ```

3. Generate access keys
   ```bash
   aws iam create-access-key --user-name skycms-publisher
   ```

4. Configure in Editor:
   - Settings → Storage → Add S3 Bucket
   - Access Key ID and Secret from above
   - Bucket name: `skycms-publisher-<random>`

### Setup SES Email (Optional)

1. Verify sender email in SES
   ```bash
   aws ses verify-email-identity --email-address your-email@example.com
   ```

2. Create SES credentials
   - AWS Console → SES → SMTP Settings
   - Create SMTP credentials for your app

3. Configure in Editor:
   - Settings → Email → SES SMTP
   - Enter credentials from above

---

## 🔍 Monitoring and Troubleshooting

### Check ECS Task Status

```bash
# List running tasks
aws ecs list-tasks --cluster skycms-cluster --region us-east-1

# View task details
aws ecs describe-tasks --cluster skycms-cluster \
  --tasks <task-arn> --region us-east-1

# View task logs
aws logs tail /ecs/skycms --follow
```

### Check RDS Connection

```bash
# Get database endpoint from CloudFormation Outputs
RDS_ENDPOINT=$(aws cloudformation describe-stacks \
  --stack-name skycms \
  --query 'Stacks[0].Outputs[?OutputKey==`DatabaseEndpoint`].OutputValue' \
  --output text)

# Test connection (requires mysql client)
mysql -h $RDS_ENDPOINT -u skycms_admin -p skycms
```

### Common Issues

| Problem | Solution |
|---------|----------|
| **CloudFront shows 502 Bad Gateway** | Wait 2-3 more minutes for ECS tasks to pull image and start. Check ECS Service health in CloudFormation → Resources |
| **Can't connect to database** | Verify security groups. Check: RDS Security Group allows inbound port 3306 from ECS Security Group |
| **Setup wizard won't load** | Check browser console for errors. Verify ALB is healthy (CloudFormation → Resources → ALB → View Details) |
| **ECS tasks keep restarting** | Check CloudWatch logs: `/ecs/skycms`. Database connection string might be wrong. |
| **High AWS costs** | Reduce DesiredTaskCount to 1, or scale down RDS instance class after deployment |

---

## 📊 Accessing Resources

### AWS Console Links

**Your CloudFormation Stack:**
- Console → CloudFormation → Stacks → Filter by `skycms`

**ECS Cluster:**
- Console → ECS → Clusters → `skycms-cluster`
- View services, tasks, logs

**RDS Database:**
- Console → RDS → Databases → `skycms-db`
- Monitor CPU, memory, connections

**CloudFront Distribution:**
- Console → CloudFront → Distributions
- Monitor cache hit ratio, requests

**CloudWatch Logs:**
- Console → CloudWatch → Log Groups → `/ecs/skycms`
- View real-time application logs

---

## 🗑️ Cleanup (Cost Savings)

When you're done testing:

```bash
# Delete the CloudFormation stack (deletes all resources)
aws cloudformation delete-stack --stack-name skycms --region us-east-1

# Monitor deletion
aws cloudformation wait stack-delete-complete --stack-name skycms --region us-east-1

# Verify it's gone
aws cloudformation list-stacks --query 'StackSummaries[?StackName==`skycms`]'
```

**Note:** RDS instance will be snapshotted before deletion (DeletionPolicy: Snapshot). You can restore from snapshot later if needed.

---

## 📈 Scaling After Deployment

### Increase Capacity

```bash
# Update CloudFormation stack with more tasks
aws cloudformation update-stack --stack-name skycms \
  --use-previous-template \
  --parameters ParameterKey=DesiredTaskCount,ParameterValue=3
```

### Change RDS Instance Class

```bash
# Via AWS Console: RDS → Databases → skycms-db → Modify
# Select larger instance class (db.t4g.small, db.t4g.medium)
# Apply immediately or during maintenance window
```

### Enable RDS Read Replicas (Advanced)

```bash
# Create read replica for database scaling
aws rds create-db-instance-read-replica \
  --db-instance-identifier skycms-db-replica \
  --source-db-instance-identifier skycms-db
```

---

## 🔐 Security Best Practices

### After Initial Setup

1. **Rotate RDS Password**
   - AWS Secrets Manager → Select secret
   - Rotate credentials regularly

2. **Enable WAF on CloudFront (Optional)**
   - AWS Console → WAF & Shield → Create WebACL
   - Attach to CloudFront distribution

3. **Set up VPC Flow Logs**
   - VPC → Flow Logs → Create new log group
   - Monitor suspicious traffic

4. **Enable RDS backup retention**
   - Already configured (7-day retention)
   - Backups stored in AWS Backup vault automatically

---

## 💡 Next Steps

1. ✅ Complete setup wizard with database credentials
2. ✅ Create admin account and log in
3. ✅ Create your first page/article
4. ⏭️ Set up Publisher for static site generation (see Publisher documentation)
5. ⏭️ Configure custom domain with Route 53 (see AWS networking guide)
6. ⏭️ Enable SES email for notifications (see AWS SES setup)

---

## 📚 More Resources

- **[SkyCMS Documentation](https://docs-sky-cms.com)** - Complete guides and tutorials
- **[AWS ECS Documentation](https://docs.aws.amazon.com/ecs/)** - Container management
- **[AWS RDS Documentation](https://docs.aws.amazon.com/rds/)** - Database management
- **[CloudFormation Reference](https://docs.aws.amazon.com/cloudformation/)** - Template documentation

---

## 🆘 Need Help?

- **Documentation:** https://docs-sky-cms.com
- **GitHub Issues:** https://github.com/CWALabs/SkyCMS/issues
- **CloudFormation Events:** Check stack Events tab for deployment errors
- **CloudWatch Logs:** `/ecs/skycms` contains application logs
