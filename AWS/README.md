# SkyCMS on AWS (CloudFormation + ECS Fargate)

**NOTE: This is a draft document and is being tested.**

This guide deploys the SkyCMS Editor and Publisher containers on AWS using:

- ECS Fargate for compute
- An Application Load Balancer (ALB)
  - Port 80 -> Editor
  - Port 8080 -> Publisher
- S3 for blob storage
- EFS for persistent SQLite file storage

It mirrors the Azure ARM deployment (App Service + Storage) with equivalent AWS building blocks.

## What this stack creates

- ECS cluster, task definitions, and services for Editor and Publisher
- Public ALB with two listeners (80 and 8080) and target groups
- EFS filesystem with mount targets (for `/data/sqlite`)
- IAM roles (execution + task) with scoped S3 permissions to your bucket
- CloudWatch Logs log group for container logs

Bring or create:

- An S3 bucket for website content (provide the name and region)
- AWS credentials for the app (AccessKeyId/Secret) or consider using task role only (see notes)
- An existing VPC and two public subnets

## Parameters

Required

- VpcId: Existing VPC ID
- PublicSubnets: Two or more public subnets (different AZs)
- S3BucketName: Name of your S3 bucket
- S3Region: Region of your S3 bucket (e.g., us-west-2)
- S3AccessKeyId: Access key ID with least-privilege to S3 bucket
- S3SecretAccessKey: Secret key for the above
- AdminEmailAddress: Initial admin email address

Optional

- EditorImage (default toiyabe/sky-editor:latest)
- PublisherImage (default toiyabe/sky-publisher:latest)
- DesiredCount (default 1)
- SMTP or SendGrid options

## How storage is configured

SkyCMS detects S3 when `ConnectionStrings:StorageConnectionString` uses the S3 format:

Bucket={bucket};Region={region};KeyId={access-key-id};Key={secret};

The template injects this into both apps so they write/read content directly from your S3 bucket.

For SQLite, both apps mount EFS at `/data/sqlite` and use `Data Source=/data/sqlite/skycms.db`.

## Outputs

- EditorUrl: http://`alb-dns`/
- PublisherUrl: http://`alb-dns`:8080/

In the Editor app settings, the template sets `CosmosPublisherUrl` to the PublisherUrl so publishing works out of the box.

## Deploy

You can launch from the console (CloudFormation) and supply parameters, or package/deploy via CLI. Ensure your subnets have internet routing for Fargate tasks.

Post-deploy:

1. Visit EditorUrl, complete initial setup.
2. Configure database and email if needed.
3. Publish a test page; confirm it appears at PublisherUrl.

## Security notes

- The task role policy grants read/write/delete on the specific S3 bucket only. Tighten further by adding explicit prefixes if desired.
- EFS SG only allows NFS from the ECS service SGs; services accept traffic only from the ALB SG.
- Consider moving from AccessKeyId/Secret to task-role-only access by updating SkyCMS to support AWS SDK default credentials (future improvement).

## Known differences vs Azure

- Azure ARM mounts Azure Files automatically; on AWS we use EFS.
- HTTPS/ACM not configured by default; add an HTTPS listener and ACM certificate on the ALB for production.
- If you want separate hostnames for Editor/Publisher, add path or host-based routing rules to the ALB, or create two ALBs.

## Clean up

Delete the CloudFormation stack to remove all resources. EFS contents are retained unless you delete the filesystem manually.
