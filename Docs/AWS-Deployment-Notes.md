# SkyCMS Editor on AWS (ECS Fargate + ALB + CloudFront + RDS MySQL)

This note documents the concrete changes and steps we used to run the SkyCMS Editor on AWS. It’s intended as a practical reference you can point others to.

## Why CDK (not raw CloudFormation)
- We switched from hand-authored CloudFormation templates to AWS CDK because hand-writing this stack is brittle and time-consuming; CDK synthesizes correct resources and wiring, avoiding many pitfalls.
- CDK advantages for this project:
  - Dependency ordering and cross-service references (ECS ↔ ALB/TargetGroup/Listener, CloudFront ↔ ALB, RDS ↔ Security Groups).
  - Safer defaults and guardrails (least-privilege IAM, log retention, health check configuration).
  - Asset publishing and ECR image parameterization without bespoke packaging glue.
  - Easier environment-specific configuration via context/parameters and code reuse.
  - Faster iteration with diffable changes and logical constructs instead of JSON/YAML.
- Bottom line: CDK removed a long list of subtle CFN gotchas and accelerated our progress.

## App (ASP.NET Core)
- Forwarded headers: enable X-Forwarded-For and X-Forwarded-Proto, clear trusted proxies.
  - File: Editor/Program.cs
  - Why: honors CloudFront/ALB headers so redirects and OAuth use https.
- Forwarded headers middleware: app.UseForwardedHeaders() early in the pipeline.
- Health endpoint: add GET /healthz returning 200 OK for load balancer health checks.
- Cookies/HSTS: secure cookies (SecurePolicy = Always) and HSTS enabled in production.
- OAuth redirect resilience: existing Microsoft OAuth event handler adapts redirect URLs when behind a proxy.

## Container (Docker)
- Listener port: Dockerfile exposes 8080 (dotnet base image default). We align AWS target port to 8080.
  - File: Editor/Dockerfile

## Infrastructure (AWS CDK – Minimal Stack)
- Container port mapping: containerPort: 8080 in task definition so ALB can reach the app.
- ALB target group port: forward to 8080.
- Security groups: allow ALB SG → ECS tasks SG on tcp/8080.
- Health checks: target group checks GET /healthz, codes 200–399, timeout=5s, interval=15s, thresholds 2/2.
- Grace period: ECS service healthCheckGracePeriod: 120s for app warm-up.
- Public egress (no NAT): assignPublicIp: true so tasks can reach AWS APIs (e.g., Secrets Manager).
- Environment and secrets: baseline app env vars and secrets from Secrets Manager (DB password, connection string).
  - Files: InstallScripts/AWS/cdk/lib/skycms-stack-minimal.ts, InstallScripts/AWS/cdk/bin/skycms.ts

## RDS MySQL
- Parameter group: enforce TLS with require_secure_transport=1.
- Networking: RDS SG allows MySQL (3306) from ECS tasks SG (dev-only 0.0.0.0/0 is optional and temporary).
- Outputs: DB endpoint and a dev connection string for validation only.

## CloudFront / HTTPS
- Viewer TLS: CloudFront serves HTTPS with the default CloudFront certificate (no ACM required).
- Edge redirect: viewerProtocolPolicy = REDIRECT_TO_HTTPS.
- Origin: CloudFront → ALB over HTTP by default (optionally HTTPS if you later supply an ACM cert and enable it in CDK).

## Deployment Script (PowerShell)
- Script supports parallel stacks with -StackName so you can deploy a new stack while deleting or waiting on another.
  - File: InstallScripts/AWS/cdk-deploy.ps1

### Typical Deploy Flow
```
# From repo
cd .\InstallScripts\AWS

# Optional: build and push your image (linux/amd64)
# docker build --platform linux/amd64 -t toiyabe/sky-editor:latest -f .\Editor\Dockerfile .
# docker push toiyabe/sky-editor:latest

# Deploy (defaults: us-east-1, image=toiyabe/sky-editor:latest)
# You can supply a custom image tag and/or a custom stack name
.\cdk-deploy.ps1 -Image toiyabe/sky-editor:latest -StackName SkyCmsStack2
```

### Retrieve Outputs and Verify Health
```
# Get outputs (CloudFront URL, ALB DNS, LogGroup, DB info)
aws cloudformation describe-stacks --stack-name <YourStackName> --region us-east-1 --query "Stacks[0].Outputs" --output table

# Resolve ALB → Target Group and check target health
$lbDns = "<ALB_DNS_FROM_OUTPUTS>"
$lbArn = (aws elbv2 describe-load-balancers --region us-east-1 --query "LoadBalancers[?DNSName=='$lbDns'].LoadBalancerArn" --output text)
$tgArn = (aws elbv2 describe-target-groups --load-balancer-arn $lbArn --region us-east-1 --query "TargetGroups[0].TargetGroupArn" --output text)
aws elbv2 describe-target-health --target-group-arn $tgArn --region us-east-1

# Tail app logs (replace with LogGroupName from outputs)
aws logs tail <LogGroupName> --region us-east-1 --since 10m
```

## Common Gotchas We Solved
- Port mismatch: app listens on 8080; ALB/ECS initially targeted 80. Fixed by aligning both to 8080.
- Health checks failing: added /healthz and tuned health check settings + 120s grace period.
- Proxy headers: needed forwarded headers so redirects and cookies behave over CloudFront/ALB (https origin vs viewer).
- No NAT: tasks require assignPublicIp: true in public subnets to reach AWS APIs when there’s no NAT gateway.

## Notes and Next Steps
- ACM (optional): if you want HTTPS from ALB to origin and/or custom domains, add an ACM certificate (us-east-1 for CloudFront) and enable HTTPS origin in CDK.
- Container health checks: consider adding a Docker HEALTHCHECK and ECS container health check for deeper diagnostics.
- CI/CD: integrate cdk synth/deploy and image build/push in a pipeline.

We will keep this document updated as we refine the deployment and add more configuration guidance.
