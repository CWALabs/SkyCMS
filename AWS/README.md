
# SkyCMS AWS CloudFormation Deployment

This folder contains the CloudFormation template to deploy SkyCMS Editor and Publisher on AWS using ECS Fargate, S3 (blob storage), EFS (SQLite persistence), and an Application Load Balancer (ALB) with host-based routing. The template can also create a new VPC for you (two public subnets, optional private subnets + NAT).

## Features

- **ECS Fargate**: Runs Editor and Publisher containers securely.
- **EFS**: Shared SQLite database with secure password (AWS Secrets Manager).
- **S3**: Blob storage for media and files.
- **ALB**: Exposes HTTP (80) and optional HTTPS (443) endpoints, supports host-based routing.
- **Route 53 (optional)**: Automatically creates DNS records for your hostnames.
- **Secure by default**: Security groups restrict ingress to ALB only; EFS uses an Access Point; IAM/Secrets Manager for credentials.
- **Networking built-in (optional)**: Create a new VPC with two public subnets for ALB/ECS/EFS, plus optional private subnets and a NAT Gateway for future databases.

## Key Parameters

| Parameter              | Description                                                                                 |
|------------------------|---------------------------------------------------------------------------------------------|
| `InstallId`            | Required. Short install name used to tag all resources with `skycms-install-id=<InstallId>`. |
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
| `AssignPublicIp`       | ENABLED/DISABLED. Enable if your subnets lack NAT egress so tasks can reach the internet.   |
| `CreateNewVPC`         | `true`/`false`. Create and use a new VPC with two public subnets.                           |
| `VpcCidr`              | CIDR for the new VPC (when `CreateNewVPC=true`).                                            |
| `PublicSubnet1Cidr`    | CIDR for public subnet 1 (when `CreateNewVPC=true`).                                        |
| `PublicSubnet2Cidr`    | CIDR for public subnet 2 (when `CreateNewVPC=true`).                                        |
| `CreatePrivateSubnets` | `true`/`false`. Also create two private subnets + NAT (when `CreateNewVPC=true`).           |

## Host-Based Routing

- Requests to `EditorHostName` are routed to the Editor container.
- Requests to `PublisherHostName` (if set) are routed to the Publisher container.
- All other requests go to Publisher by default.

## HTTPS Redirect

- If `ACMCertificateArn` is set and `RedirectHttpToHttps` is `true`, all HTTP (80) traffic is redirected to HTTPS (443).

## DNS Automation (Optional)

- If `HostedZoneId` is provided, Route 53 A-alias records are created for `EditorHostName` and (if set) `PublisherHostName`, pointing to the ALB.
- If your DNS is hosted outside Route 53, you can leave `HostedZoneId` blank and create A/ALIAS records in your external DNS provider, targeting the ALB DNS name from the stack output (`LoadBalancerDNSName`). This achieves the same result: your hostnames point to the ALB.

## Secure SQLite

- SQLite password is auto-generated and stored in AWS Secrets Manager.
- Both containers mount EFS at `/data/sqlite` and use `/data/sqlite/skycms.db` for the database.

## Deployment Checklist

- **ACM Certificate**: Issue a certificate covering all hostnames (Editor/Publisher) in AWS Certificate Manager.
- **Route 53 Hosted Zone**: Ensure your domain is managed in Route 53 and note the Hosted Zone ID.
- **S3 Bucket**: Create or select an S3 bucket for blob storage.
- **VPC/Subnets**: Either let the template create the VPC (set `CreateNewVPC=true`) or provide your own `VpcId` and two public subnets.
- **Outbound access choice**: Either set `AssignPublicIp=ENABLED`, or keep it disabled and provide NAT; see “Outbound internet access and image pulls” below for options.
- **Deploy**: Use AWS Console or CLI to launch the stack.
- **DNS**: If you provided `HostedZoneId`, DNS records are created automatically. Otherwise, point your hostnames to the ALB DNS name (see stack outputs).

### Example CLI deployment (new VPC)

```pwsh
aws cloudformation deploy --template-file AWS/cloudformation-skycms.yaml --stack-name skycms \
  --parameter-overrides CreateNewVPC=true CreatePrivateSubnets=true InstallId=prod-west \
  EditorHostName=editor.example.com PublisherHostName=www.example.com \
  ACMCertificateArn=arn:aws:acm:... HostedZoneId=Z1234567890 S3BucketName=your-bucket S3Region=us-west-2 \
  AssignPublicIp=ENABLED AdminEmailAddress=admin@example.com

```

### Example CLI deployment (existing VPC)

```pwsh
aws cloudformation deploy --template-file AWS/cloudformation-skycms.yaml --stack-name skycms \
  --parameter-overrides CreateNewVPC=false InstallId=staging1 \
  VpcId=vpc-0123456789abcdef0 PublicSubnets='subnet-aaa,subnet-bbb' \
  EditorHostName=editor.example.com PublisherHostName=www.example.com \
  ACMCertificateArn=arn:aws:acm:... HostedZoneId=Z1234567890 S3BucketName=your-bucket S3Region=us-west-2 \
  AssignPublicIp=ENABLED AdminEmailAddress=admin@example.com
```


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

### Resource tagging

- All supported resources are tagged with `skycms-install-id=<InstallId>` to make it easy to search and cost-allocate per installation.
- Examples of tagged resources: VPC/subnets/route tables, security groups, ALB and target groups, ECS cluster/services/task defs, EFS (filesystem and access point), NAT/EIP, CloudWatch log group, IAM roles, and Secrets Manager secret.

## Outbound internet access and image pulls

ECS tasks need outbound connectivity to pull container images and call external services (Docker Hub, SMTP, SendGrid, etc.). Choose one:

- **Option 1 — Public IPs on tasks**
  - Set `AssignPublicIp=ENABLED` and use public subnets with a route to an Internet Gateway.
  - Easiest way to unblock image pulls from Docker Hub (dev/test).

- **Option 2 — Private subnets + NAT Gateway**
  - For private workloads. With this template: set `CreateNewVPC=true` and `CreatePrivateSubnets=true`.
  - Keep `AssignPublicIp=DISABLED` for private tasks and ensure they run in the private subnets.
  - Public-facing ALB/ECS tasks (Editor/Publisher) can remain in public subnets.

- **Option 3 — VPC endpoints (no NAT)**
  - Push images to Amazon ECR and add VPC Interface Endpoints for `com.amazonaws.<region>.ecr.api` and `com.amazonaws.<region>.ecr.dkr`.
  - Add a Gateway VPC endpoint for S3 (`com.amazonaws.<region>.s3`).
  - Lets tasks pull from ECR and access S3 without public internet.

## Troubleshooting

- Error: `CannotPullContainerError: ... failed to resolve ref docker.io/...: i/o timeout`
  - Cause: No outbound internet from tasks to reach Docker Hub.
  - Fix: Set `AssignPublicIp=ENABLED` or provide NAT egress; alternatively, move images to ECR and add VPC endpoints.

---
For more details, see comments in `cloudformation-skycms.yaml`.
