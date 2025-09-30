
# SkyCMS AWS CloudFormation Deployment

This folder contains the CloudFormation template to deploy SkyCMS Editor and Publisher on AWS using ECS Fargate, S3 (blob storage), EFS (SQLite persistence), and an Application Load Balancer (ALB) with host-based routing.

## Features
- **ECS Fargate**: Runs Editor and Publisher containers securely.
- **EFS**: Shared SQLite database with secure password (AWS Secrets Manager).
- **S3**: Blob storage for media and files.
- **ALB**: Exposes HTTP (80) and optional HTTPS (443) endpoints, supports host-based routing.
- **Route 53 (optional)**: Automatically creates DNS records for your hostnames.
- **Secure by default**: No public IPs on containers; security groups restrict ingress to ALB only.

## Key Parameters
| Parameter              | Description                                                                                 |
|------------------------|---------------------------------------------------------------------------------------------|
| `EditorHostName`       | Required. FQDN for the Editor (e.g., `editor.example.com`).                                 |
| `PublisherHostName`    | Optional. FQDN for the Publisher (e.g., `www.example.com`). If blank, ALB default serves Publisher. |
| `RedirectHttpToHttps`  | `true`/`false`. If true and ACM cert is provided, HTTP requests are 301-redirected to HTTPS. |
| `ACMCertificateArn`    | Optional. ACM certificate ARN for HTTPS (must cover all hostnames).                         |
| `HostedZoneId`         | Optional. Route 53 Hosted Zone ID to auto-create DNS records for hostnames.                 |
| `S3BucketName`         | S3 bucket for blob storage.                                                                 |
| `S3Region`             | AWS region for S3 bucket.                                                                   |
| `AdminEmailAddress`    | Initial admin email.                                                                        |
| `EditorImage`          | Docker image for Editor.                                                                    |
| `PublisherImage`       | Docker image for Publisher.                                                                 |

## Host-Based Routing
- Requests to `EditorHostName` are routed to the Editor container.
- Requests to `PublisherHostName` (if set) are routed to the Publisher container.
- All other requests go to Publisher by default.

## HTTPS Redirect
- If `ACMCertificateArn` is set and `RedirectHttpToHttps` is `true`, all HTTP (80) traffic is redirected to HTTPS (443).

## DNS Automation (Optional)
- If `HostedZoneId` is provided, Route 53 A-alias records are created for `EditorHostName` and (if set) `PublisherHostName`, pointing to the ALB.

## Secure SQLite
- SQLite password is auto-generated and stored in AWS Secrets Manager.
- Both containers mount EFS at `/skycms` and use `/skycms/skycms.db` for the database.

## Deployment Checklist
1. **ACM Certificate**: Issue a certificate covering all hostnames (Editor/Publisher) in AWS Certificate Manager.
2. **Route 53 Hosted Zone**: Ensure your domain is managed in Route 53 and note the Hosted Zone ID.
3. **S3 Bucket**: Create or select an S3 bucket for blob storage.
4. **VPC/Subnets**: Identify your VPC and at least two public subnets for ALB/ECS/EFS.
5. **Deploy**: Use AWS Console or CLI to launch the stack:
   ```pwsh
   aws cloudformation deploy --template-file AWS/cloudformation-skycms.yaml --stack-name skycms \
     --parameter-overrides EditorHostName=editor.example.com PublisherHostName=www.example.com \
     ACMCertificateArn=arn:aws:acm:... HostedZoneId=Z1234567890 S3BucketName=your-bucket S3Region=us-west-2 \
     AdminEmailAddress=admin@example.com
   ```
6. **DNS**: If you provided `HostedZoneId`, DNS records are created automatically. Otherwise, point your hostnames to the ALB DNS name (see stack outputs).

## Outputs
- `LoadBalancerDNSName`: Public DNS name of the ALB.
- `EditorDNSRecordName` / `PublisherDNSRecordName`: Hostnames for which DNS records were created (if enabled).
- `EditorServiceName` / `PublisherServiceName`: ECS service names.
- `EfsFileSystemId`: EFS filesystem ID.
- `S3Bucket`: S3 bucket name.

## Notes
- Both containers listen on port 80 internally; ALB handles external routing and TLS.
- Security groups restrict traffic to only what is needed.
- For troubleshooting, use AWS Console to inspect ECS tasks, ALB listeners/rules, and Route 53 records.

---
For more details, see comments in `cloudformation-skycms.yaml`.
