# AWS vs Azure Architecture Comparison

A side-by-side comparison of SkyCMS deployment on AWS and Azure.

## Infrastructure Overview

| Component | AWS | Azure | Notes |
|-----------|-----|-------|-------|
| **Container Platform** | Amazon ECS Fargate | Azure Container Apps | Azure is simpler (no separate ALB/CloudFront) |
| **Load Balancer** | Application Load Balancer | Built into Container Apps | Azure includes this automatically |
| **CDN** | CloudFront | Built into Container Apps | Azure provides HTTPS endpoint by default |
| **Database** | Amazon RDS MySQL | Azure Database for MySQL | Similar capabilities, both support TLS |
| **Secrets** | AWS Secrets Manager | Azure Key Vault | Azure uses RBAC, AWS uses IAM policies |
| **Object Storage** | Amazon S3 | Azure Blob Storage | Both support static website hosting |
| **Networking** | VPC (required) | Virtual Network (optional) | Azure is simpler for basic deployments |
| **Identity** | IAM Roles | Managed Identity + RBAC | Different models, both passwordless |
| **IaC Tool** | AWS CDK (TypeScript) | Bicep | Bicep is Azure-native, similar to Terraform |

---

## Deployment Architecture

### AWS Architecture

```
Internet
   ↓
CloudFront (CDN)
   ↓
Application Load Balancer
   ↓
ECS Fargate (Editor Container)
   ↓
┌─────────────┬──────────────┐
│             │              │
RDS MySQL    S3 Bucket   Secrets Manager
(Database)   (Publisher)   (Credentials)
```

**Resources Created:** 10-12 separate AWS resources

---

### Azure Architecture

```
Internet
   ↓
Container Apps (Built-in HTTPS + Load Balancing)
   ↓
┌─────────────┬──────────────┐
│             │              │
MySQL         Blob Storage   Key Vault
Flexible      (Publisher)    (Secrets)
Server
```

**Resources Created:** 5-7 separate Azure resources

---

## Code Comparison

### Infrastructure as Code

**AWS CDK (TypeScript):**
```typescript
// TypeScript-based, programmatic IaC
const db = new rds.DatabaseInstance(this, 'MySql', {
  engine: rds.DatabaseInstanceEngine.mysql({ 
    version: rds.MysqlEngineVersion.VER_8_0 
  }),
  vpc,
  instanceType: ec2.InstanceType.of(
    ec2.InstanceClass.BURSTABLE4_GRAVITON, 
    ec2.InstanceSize.MICRO
  ),
  // ... more config
});
```

**Azure Bicep:**
```bicep
// Declarative, ARM-based IaC
resource mysqlServer 'Microsoft.DBforMySQL/flexibleServers@2023-12-30' = {
  name: serverName
  location: location
  sku: {
    name: 'Standard_B1ms'
    tier: 'Burstable'
  }
  properties: {
    version: '8.0.21'
    // ... more config
  }
}
```

---

## Networking

### AWS
```
VPC (REQUIRED)
├── Public Subnets (ALB + ECS)
│   └── 0.0.0.0/0 → Internet Gateway
├── Private Isolated Subnets (RDS)
│   └── No internet access
└── Security Groups
    ├── ALB SG → Allow 80/443
    ├── ECS SG → Allow from ALB
    └── RDS SG → Allow from ECS
```

**Complexity:** High (must configure subnets, route tables, security groups)

---

### Azure
```
Container Apps Environment (AUTOMATIC)
├── Public endpoint (HTTPS by default)
├── MySQL Firewall Rules
│   └── Allow Azure services
└── Managed Identity (passwordless)
```

**Complexity:** Low (networking is mostly automatic)

---

## Security

| Feature | AWS | Azure |
|---------|-----|-------|
| **Encryption in Transit** | TLS (via parameter group) | TLS Required (default) |
| **Secrets Storage** | Secrets Manager | Key Vault |
| **Service Auth** | IAM Roles | Managed Identity |
| **Database Auth** | Username/Password (from Secret) | Username/Password (from Key Vault) |
| **HTTPS** | CloudFront (manual config) | Container Apps (automatic) |
| **Network Isolation** | Security Groups | Firewall Rules |

---

## Cost Comparison (Development)

### AWS (~$40-60/month)
- **ECS Fargate:** $15-20 (0.5 vCPU, 1GB)
- **Application Load Balancer:** $16-18
- **CloudFront:** $1-5 (minimal traffic)
- **RDS MySQL (db.t4g.micro):** $10-15
- **S3 + CloudFront (Publisher):** $1-5
- **Secrets Manager:** $0.40/secret
- **Data Transfer:** $1-5

### Azure (~$30-40/month)
- **Container Apps:** $15-20 (0.5 vCPU, 1GB)
- **Container Apps Environment:** $5
- **MySQL Flexible (B1ms):** $10-15
- **Blob Storage (Publisher):** $1-5
- **Key Vault:** $0.50
- **Data Transfer:** $0-2

**Azure Advantage:** ~25% cheaper due to:
- No separate ALB cost (built into Container Apps)
- No CloudFront cost (built-in HTTPS)
- Lower networking overhead

---

## Scaling

### AWS
```typescript
// ECS Service Auto Scaling
desiredCount: 1,
cpu: 512,
memoryLimitMiB: 1024,
```

**Manual Configuration:** Must set up CloudWatch alarms + scaling policies

---

### Azure
```bicep
// KEDA-based auto-scaling
scale: {
  minReplicas: 0  // Can scale to zero!
  maxReplicas: 10
  rules: [
    {
      name: 'http-scaling'
      http: { metadata: { concurrentRequests: '10' } }
    }
  ]
}
```

**Automatic:** Built-in HTTP-based scaling, can scale to zero

---

## Deployment Experience

### AWS CDK
```powershell
cd InstallScripts/AWS/cdk
npm install
npx cdk bootstrap
npx cdk deploy --parameters BucketName=xyz
```
- Requires Node.js
- Bootstrap CDK toolkit
- TypeScript compilation step

---

### Azure Bicep
```powershell
cd InstallScripts/Azure
.\deploy-skycms.ps1
```
- No dependencies (Azure CLI only)
- Interactive prompts
- No compilation needed

---

## Key Takeaways

### Choose AWS if:
- ✅ Already invested in AWS ecosystem
- ✅ Need AWS-specific services (Lambda@Edge, etc.)
- ✅ Team prefers programmatic IaC (TypeScript)
- ✅ Multi-cloud strategy with strong AWS presence

### Choose Azure if:
- ✅ Want simpler architecture (fewer resources)
- ✅ Lower monthly costs for dev/staging
- ✅ Prefer declarative IaC (Bicep/ARM)
- ✅ Need scale-to-zero capability
- ✅ Already using Microsoft 365 / Azure AD
- ✅ Want built-in HTTPS without extra config

---

## Migration Path (AWS → Azure)

If you want to migrate from AWS to Azure:

1. **Export AWS data:**
   - Dump RDS MySQL database
   - Download S3 bucket contents

2. **Deploy Azure infrastructure:**
   ```powershell
   cd InstallScripts/Azure
   .\deploy-skycms.ps1
   ```

3. **Migrate data:**
   - Import MySQL dump to Azure Database for MySQL
   - Upload files to Azure Blob Storage

4. **Update DNS:**
   - Point domain to Azure Container Apps or Front Door

5. **Teardown AWS:**
   ```powershell
   cd InstallScripts/AWS
   .\destroy-all.ps1
   ```

---

## Summary

Both platforms work great for SkyCMS. Azure is **simpler and cheaper** for most use cases, while AWS offers **more advanced networking control** if needed.

**Recommendation:** Start with Azure for development, evaluate based on your specific requirements.
