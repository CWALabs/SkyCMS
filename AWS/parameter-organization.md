# CloudFormation Parameter Organization

When you deploy the `skycms-mysql.yaml` template through the AWS CloudFormation Console, parameters are now organized into logical groups for better user experience.

## Parameter Group Organization

### 1. Basic Configuration
- **Installation ID**: A short name to tag all resources
- **Stack Name Prefix**: Logical name prefix for created resources  
- **Administrator Email**: Email for initial account and notifications

### 2. S3 Storage Configuration
- **S3 Bucket Name**: Globally unique bucket name for blob storage
- **S3 Region**: AWS region of the S3 bucket
- **S3 Access Key ID**: Access key for S3 bucket access
- **S3 Secret Access Key**: Secret key for S3 bucket access

*Note: S3 credentials now appear together for easier configuration*

### 3. Email Provider - SendGrid (option 1)
- **API Key**: SendGrid API key (leave blank if not using SendGrid)

### 4. Email Provider - SMTP (option 2)  
- **Host Name**: SMTP server hostname (e.g., smtp.gmail.com)
- **Port Number**: SMTP port (e.g., 587)
- **Enable SSL/TLS**: Secure connection setting (true/false)
- **Username**: SMTP authentication username
- **Password**: SMTP authentication password

### 5. Email Provider - Azure Communications (option 3)
- **Connection String**: Azure Communications connection string (leave blank if not using Azure Communications)

*Note: Choose only ONE email provider. Configure parameters for your chosen provider and leave others blank*

### 6. Application Settings
- **Editor Container Image**: Docker image for Editor service
- **Publisher Container Image**: Docker image for Publisher service
- **Desired Task Count**: Number of ECS tasks per service
- **Assign Public IP to Tasks**: Enable for Docker Hub image pulls

### 5. Custom Domain & TLS (optional)
- **Editor Host Name**: Custom domain for Editor (optional)
- **Publisher Host Name**: Custom domain for Publisher (optional)
- **ACM Certificate ARN**: SSL certificate for custom domains
- **Redirect HTTP to HTTPS**: Force HTTPS redirects

### 6. Database Configuration
- **Database Instance Class**: RDS instance size
- **Database Storage (GB)**: Allocated storage amount
- **Backup Retention Days**: Database backup retention
- **Database Name**: MySQL database name
- **Database Username**: MySQL admin username
- **Database Password**: MySQL admin password

### 7. Network Configuration (advanced)
- **VPC CIDR Block**: Main VPC address range
- **Public Subnet 1 CIDR**: First public subnet range
- **Public Subnet 2 CIDR**: Second public subnet range  
- **Private Subnet 1 CIDR**: First private subnet range
- **Private Subnet 2 CIDR**: Second private subnet range

## Benefits

1. **Logical Grouping**: Related parameters appear together
2. **Clear Labels**: User-friendly parameter names
3. **Guided Configuration**: Groups guide users through setup process
4. **Reduced Errors**: S3 credentials appear together, email options are clearly grouped
5. **Progressive Disclosure**: Basic settings first, advanced settings last

## CLI Deployment

Parameter organization only affects the AWS Console interface. CLI deployments use the same parameter names as before:

```bash
aws cloudformation deploy \
  --template-file skycms-mysql.yaml \
  --stack-name my-skycms \
  --parameter-overrides \
    InstallId=prod \
    AdminEmailAddress=admin@example.com \
    S3BucketName=my-unique-bucket \
    S3AccessKeyId=AKIA... \
    S3SecretAccessKey=... \
    # ... other parameters
```

The parameter organization improves the AWS Console experience without affecting CLI or programmatic deployments.