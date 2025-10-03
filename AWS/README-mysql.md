# SkyCMS AWS CloudFormation (MySQL)

This template (`skycms-mysql.yaml`) deploys SkyCMS Editor and Publisher on AWS using:

- ECS Fargate (two services: Editor/Publisher)
- RDS MySQL (database name: `skycms`, in private subnets)
- S3 bucket for blob storage
- ALB with host-based routing and optional TLS termination
- New VPC with two public subnets (for ALB/ECS) and two private subnets (for RDS)

Internal container port is 8080; the public ALB listens on 80/443 and forwards to 8080.

## Parameters (key)

- InstallId: Tag value applied to resources as `skycms-install-id`.
- EditorHostName: FQDN for the Editor (e.g., `editor.example.com`).
- PublisherHostName: Optional FQDN for the Publisher (e.g., `www.example.com`). If blank, ALB default goes to Publisher.
- RedirectHttpToHttps: `true`/`false`. If `true` and ACMCertificateArn is set, HTTP → HTTPS 301 redirect.
- ACMCertificateArn: Optional. ACM cert for HTTPS on the ALB (must cover your hostnames).
- S3BucketName / S3Region / S3AccessKeyId / S3SecretAccessKey: Bucket and credentials used by SkyCMS.
- DesiredCount: ECS desired tasks per service (default 1).
- AssignPublicIp: `ENABLED`/`DISABLED`. Enable to allow ECS tasks to pull images from Docker Hub.
- DBInstanceClass / DBAllocatedStorage / DBBackupRetentionDays: RDS sizing controls.
- DbUsername / DbPassword: MySQL admin credentials used for the RDS instance and app connection strings (DbPassword is NoEcho in CloudFormation).

Networking CIDRs are configurable via VpcCidr/PublicSubnet1Cidr/PublicSubnet2Cidr/PrivateSubnet1Cidr/PrivateSubnet2Cidr.

## What gets created

- VPC + 2 public subnets + 2 private subnets
- Internet Gateway, route tables, and associations
- Security groups for ALB, ECS services, and MySQL
- RDS MySQL 8.0 in private subnets (DeletionPolicy: Snapshot)
- S3 bucket (public access blocked; DeletionPolicy: Retain)
- ECS cluster, roles, task definitions, and services
- ALB (HTTP and optional HTTPS), listeners, and host-based routing rules

## Deploy

Use AWS Console or CLI. Example PowerShell (replace values):

```pwsh
aws cloudformation deploy `
  --template-file AWS/skycms-mysql.yaml `
  --stack-name skycms-mysql `
  --parameter-overrides `
    InstallId=prod-west `
    DbUsername=skycmsadmin `
    DbPassword='P@ssw0rd-ChangeMe' `
    S3BucketName=your-unique-bucket `
    S3Region=us-west-2 `
    S3AccessKeyId=AKIA... `
    S3SecretAccessKey=... `
    EditorHostName=editor.example.com `
    PublisherHostName=www.example.com `
    ACMCertificateArn=arn:aws:acm:us-west-2:123456789012:certificate/xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx `
    RedirectHttpToHttps=true `
    AssignPublicIp=ENABLED `
    DesiredCount=1
```

Note: Intrinsic function warnings like `!Ref`, `!Sub` in generic YAML linters are normal. For validation, prefer the CloudFormation validator or cfn-lint.

```pwsh
aws cloudformation validate-template --template-body file://AWS/skycms-mysql.yaml
```

### Deploy with a parameters file (recommended)

Store secrets in a params file and use the helper script:

```pwsh
# Edit the params JSON first
code AWS/examples/skycms-mysql-params.json

# Deploy
pwsh AWS/examples/deploy-mysql.ps1 -StackName skycms-mysql -Region us-west-2 -ParamsFile AWS/examples/skycms-mysql-params.json
```

## DNS setup

After the stack completes, configure DNS with your provider:

- Create an A/ALIAS (or CNAME) for `EditorHostName` → the stack output `LoadBalancerDNSName`.
- If you set `PublisherHostName`, create another A/ALIAS (or CNAME) → `LoadBalancerDNSName`.
- Use ALIAS for apex/root if supported. CNAME is fine for subdomains.

If `ACMCertificateArn` is set and covers your hostnames, browsing via HTTPS will be valid. If you enabled `RedirectHttpToHttps`, HTTP will 301 redirect to HTTPS.

## App configuration (env)

- Editor defaults:
  - `CosmosAllowSetup=true`
  - `CosmosStaticWebPages=true`
  - `AzureBlobStorageEndPoint=/`
- Publisher defaults:
  - `CosmosStaticWebPages=true`
  - `AzureBlobStorageEndPoint=/`

- Internal listener: `ASPNETCORE_URLS=http://+:8080`
- Editor `CosmosPublisherUrl` is derived from `PublisherHostName` (https if ACM is set, else http). Left empty if `PublisherHostName` is blank.
- Storage: `ConnectionStrings__StorageConnectionString="Bucket=<S3BucketName>;Region=<S3Region>;KeyId=<S3AccessKeyId>;Key=<S3SecretAccessKey>;"`
- Database: `ConnectionStrings__ApplicationDbContextConnection="Server=<RdsEndpoint>;Port=3306;Database=skycms;User Id=<secret Username>;Password=<secret Password>;"`
  - The template injects the `DbUsername` and `DbPassword` parameter values directly.

## Outputs

- LoadBalancerDNSName: Public DNS of the ALB (use this for DNS records).
- EditorServiceName / PublisherServiceName: ECS service names.
- DbEndpoint: RDS endpoint address.
- VpcId, Subnet IDs, and S3Bucket.

## Security notes

- ALB exposes 80 and optionally 443. ECS tasks listen on 8080.
- RDS is deployed in private subnets, reachable only from the ECS service security groups on port 3306.
- S3 bucket blocks public access.
- DbPassword is a NoEcho parameter. CloudFormation masks it in console output, but be mindful of where you store deploy scripts and CI logs.

## Troubleshooting

- Target Groups: Check Health for editor/publisher target groups in EC2 → Target Groups.
- ECS Services/Tasks: Check desired vs. running count and container logs.
- Database connectivity: Ensure DB endpoint is in Outputs and SG allows 3306 from ECS SGs.
- Certificate warnings: If hitting the ALB DNS while TLS is enabled for your own domain, you’ll see a warning; use your real hostnames with DNS configured.

## Cleanup

- The DB instance is Snapshotted on stack delete (DeletionPolicy: Snapshot).
- The S3 bucket is retained (DeletionPolicy: Retain). Empty/delete manually if you want to remove it.

---
For details, see comments in `AWS/skycms-mysql.yaml`.
