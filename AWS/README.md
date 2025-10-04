
# SkyCMS AWS CloudFormation Deployment

This folder contains the CloudFormation template to deploy SkyCMS Editor and Publisher on AWS using ECS Fargate, S3 (blob storage), EFS (SQLite persistence), and an Application Load Balancer (ALB) with host-based routing. It also creates two CloudFront distributions (one per app) that provide automatic TLS-enabled domains that work out-of-the-box without any DNS configuration. The template creates a new VPC (two public subnets, optional private subnets + NAT).

## Features

- **ECS Fargate**: Runs Editor and Publisher containers securely.
- **EFS**: Shared SQLite database using a password you provide via parameter (no Secrets Manager).
- **S3**: Blob storage for media and files.
- **ALB**: Exposes HTTP (80) and optional HTTPS (443) endpoints, supports host-based routing.
- **CloudFront (auto TLS + domains)**: Two distributions are created automatically (Editor/Publisher). Each has a `*.cloudfront.net` domain with TLS enabled that you can use immediately—no DNS or certificates required.
- **DNS guidance (optional)**: You can still point your own domains at the ALB if you prefer custom hostnames.
- **Secure by default**: Security groups restrict ingress to ALB only; EFS uses an Access Point; credentials are passed via NoEcho parameters.
- **Networking built-in (optional)**: Create a new VPC with two public subnets for ALB/ECS/EFS, plus optional private subnets and a NAT Gateway for future databases.

## Key Parameters

| Parameter              | Description                                                                                 |
|------------------------|---------------------------------------------------------------------------------------------|
| `InstallId`            | Required. Short install name used to tag all resources with `skycms-install-id=<InstallId>`. |
| `EditorHostName`       | Required. FQDN for the Editor (e.g., `editor.example.com`).                                 |
| `PublisherHostName`    | Optional. FQDN for the Publisher (e.g., `www.example.com`). If blank, ALB default serves Publisher. |
| `RedirectHttpToHttps`  | `true`/`false`. If true and ACM cert is provided, HTTP requests are 301-redirected to HTTPS. |
| `ACMCertificateArn`    | Optional. ACM certificate ARN for HTTPS (must cover all hostnames).                         |
| `S3BucketName`         | S3 bucket for blob storage.                                                                 |
| `S3Region`             | AWS region for S3 bucket.                                                                   |
| `SqlitePassword`       | SQLite DB password used in the connection string.                                           |
| `AdminEmailAddress`    | Initial admin email.                                                                        |
| `EditorImage`          | Docker image for Editor.                                                                    |
| `PublisherImage`       | Docker image for Publisher.                                                                 |
| `AssignPublicIp`       | ENABLED/DISABLED. Enable if your subnets lack NAT egress so tasks can reach the internet.   |
| `VpcCidr`              | CIDR for the new VPC created by this template.                                              |
| `PublicSubnet1Cidr`    | CIDR for public subnet 1.                                                                    |
| `PublicSubnet2Cidr`    | CIDR for public subnet 2.                                                                    |
| `CreatePrivateSubnets` | `true`/`false`. Also create two private subnets + NAT.                                       |

## Host-Based Routing

- Requests to `EditorHostName` are routed to the Editor container.
- Requests to `PublisherHostName` (if set) are routed to the Publisher container.
- All other requests go to Publisher by default.

## HTTPS Redirect

- If `ACMCertificateArn` is set and `RedirectHttpToHttps` is `true`, all HTTP (80) traffic is redirected to HTTPS (443).

## Optional DNS setup (if you want your own hostnames)

After the stack is created, you can use the CloudFront domains right away:

- `EditorCloudFrontDomain` and `PublisherCloudFrontDomain` outputs give you two TLS-enabled domains that “just work.”

If you want to use your own hostnames, go to your DNS provider and create records pointing to the ALB:

- Create an A/ALIAS (or CNAME if ALIAS is not available) for `EditorHostName` -> the value should be the `LoadBalancerDNSName` output.
- If you set `PublisherHostName`, create another A/ALIAS (or CNAME) for it -> also pointing to `LoadBalancerDNSName`.

Tip: Use ALIAS if your DNS provider supports it to map the root/apex or subdomains directly to the ALB DNS name.

## Secure SQLite

- Provide `SqlitePassword` when deploying; it’s injected into the connection string for `/data/sqlite/skycms.db`.
- Both containers mount EFS at `/data/sqlite` and use `/data/sqlite/skycms.db` for the database.

## Default app environment variables

- Editor defaults:
  - `CosmosAllowSetup=true`
  - `CosmosStaticWebPages=true`
  - `AzureBlobStorageEndPoint=/`
- Publisher defaults:
  - `CosmosStaticWebPages=true`
  - `AzureBlobStorageEndPoint=/`

## Deployment Checklist

- **ACM Certificate**: Issue a certificate covering all hostnames (Editor/Publisher) in AWS Certificate Manager.
- **S3 Bucket**: Create or select an S3 bucket for blob storage.
- **VPC/Subnets**: This template always creates a new VPC with two public subnets (and optional private subnets + NAT).
- **Outbound access choice**: Either set `AssignPublicIp=ENABLED`, or keep it disabled and provide NAT; see “Outbound internet access and image pulls” below for options.
- **Deploy**: Use AWS Console or CLI to launch the stack.
- **DNS**: After deployment, point your `EditorHostName` (and optional `PublisherHostName`) to the ALB DNS name from the stack output `LoadBalancerDNSName`.

### Example CLI deployment

```pwsh
aws cloudformation deploy --template-file AWS/cloudformation-skycms.yaml --stack-name skycms \
  --parameter-overrides CreatePrivateSubnets=true InstallId=prod-west \
  EditorHostName=editor.example.com PublisherHostName=www.example.com \
  ACMCertificateArn=arn:aws:acm:... S3BucketName=your-bucket S3Region=us-west-2 \
  SqlitePassword='REPLACE_ME_STRONG_PASSWORD' \
  AssignPublicIp=ENABLED AdminEmailAddress=admin@example.com

```

### Deploy with a parameters file (recommended)

Use the sample file and script to avoid typing secrets into your shell:

```pwsh
# Edit the params JSON first
code AWS/examples/skycms-sqlite-params.json

# Deploy
pwsh AWS/examples/deploy-sqlite.ps1 -StackName skycms -Region us-west-2 -ParamsFile AWS/examples/skycms-sqlite-params.json
```

###


## Outputs

- `LoadBalancerDNSName`: Public DNS name of the ALB (useful if mapping your own DNS to the ALB).
- `EditorCloudFrontDomain`: Public TLS-enabled domain for the Editor via CloudFront.
- `PublisherCloudFrontDomain`: Public TLS-enabled domain for the Publisher via CloudFront.
- `EditorServiceName` / `PublisherServiceName`: ECS service names.
- `EfsFileSystemId`: EFS filesystem ID.
- `S3Bucket`: S3 bucket name.

## Notes

- Both containers listen on port 8080 internally; the public ALB listens on 80/443 and forwards to 8080 on the tasks.
- Security groups restrict traffic to only what is needed.
- For troubleshooting, use AWS Console to inspect ECS tasks and ALB listeners/rules.

- Editor’s Publisher URL: If `PublisherHostName` is set, the Editor will automatically use `https://<PublisherHostName>` when an ACM certificate is provided (HTTPS enabled), otherwise `http://<PublisherHostName>`. If `PublisherHostName` is blank, the Editor will automatically point to the Publisher CloudFront domain (TLS-enabled) from the stack output `PublisherCloudFrontDomain`.

### Resource tagging

- All supported resources are tagged with `skycms-install-id=<InstallId>` to make it easy to search and cost-allocate per installation.
- Examples of tagged resources: VPC/subnets/route tables, security groups, ALB and target groups, ECS cluster/services/task defs, EFS (filesystem and access point), NAT/EIP, CloudWatch log group, and IAM roles.

## Outbound internet access and image pulls

ECS tasks need outbound connectivity to pull container images and call external services (Docker Hub, SMTP, SendGrid, etc.). Choose one:

- **Option 1 — Public IPs on tasks**
  - Set `AssignPublicIp=ENABLED` and use public subnets with a route to an Internet Gateway.
  - Easiest way to unblock image pulls from Docker Hub (dev/test).

- **Option 2 — Private subnets + NAT Gateway**
  - For private workloads. With this template: set `CreatePrivateSubnets=true`.
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

  ## SMTP options

  - SMTP is optional. If `SmtpHostName` is blank, SMTP settings are not used.
  - If you set `SmtpHostName`, you may optionally set `SmtpPort` and `SmtpEnableTls`:
    - Leave `SmtpPort` blank to omit it, or set a specific port value.
    - Leave `SmtpEnableTls` blank to omit it, or set to `true`/`false`.

---
For more details, see comments in `cloudformation-skycms.yaml`.
