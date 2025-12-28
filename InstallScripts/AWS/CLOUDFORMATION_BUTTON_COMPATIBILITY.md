# AWS CloudFormation Launch Stack Button - Compatibility Assessment

**Assessment Date:** December 26, 2025  
**Overall Compatibility:** ⚠️ **MEDIUM (50%)**  
**Recommendation:** **Requires Refactoring**

---

## Executive Summary

Your AWS deployment uses **AWS CDK (Cloud Development Kit)** to generate CloudFormation templates at deploy time. While this is a great approach for infrastructure-as-code, it creates challenges for CloudFormation Launch Stack buttons, which require:

1. **Pre-existing CloudFormation templates** (YAML/JSON files in the repository)
2. **Public accessibility** to the template URL
3. **Parameter definitions** that match user input requirements

**The Gap:** Your CDK synthesizes templates dynamically at deploy time and stores them in `cdk.out/` (typically gitignored). These are not suitable for public Launch Stack buttons because:
- The synthetic templates are platform/environment-specific
- They're not version-controlled with your source code
- Users would need to understand CDK to modify parameters

---

## Current AWS Deployment Architecture

### What You Have ✅

| Component | Status | Purpose |
|-----------|--------|---------|
| **AWS CDK** | ✅ Excellent | Generates CloudFormation from TypeScript code |
| **cdk-deploy.ps1** | ✅ Interactive | PowerShell wrapper with prompts |
| **CloudFormation Output** | ✅ Available | CDK synthesizes YAML templates |
| **Infrastructure Design** | ✅ Solid | ECS + RDS + CloudFront well-architected |
| **Parameter Validation** | ✅ Good | PowerShell script validates inputs |

### What's Missing for Launch Button ❌

| Requirement | Your Setup | Needed For Button |
|-------------|-----------|-------------------|
| **Template Format** | CDK → CloudFormation | Direct YAML/JSON template |
| **Version Control** | `cdk.out/` (gitignored) | Templates in repository |
| **Public Template URL** | Not applicable | Must be accessible via raw GitHub URL |
| **Parameter Structure** | Dynamic context flags | Static CloudFormation Parameters |
| **Template Updates** | Manual CDK synth + deploy | Automatically updated with commits |
| **User Customization** | Via PowerShell prompts | Via Portal form fields |

---

## Three Implementation Options

### Option 1: Quick Button (No Refactoring) 🟡 EASIEST

**Effort:** 30 minutes  
**Compatibility:** 30% (Limited functionality)

Export the synthesized CloudFormation template from `cdk.out/` and commit it to your repository. Users can then click a Launch Button to use that exact template.

**Pros:**
- Very quick to implement
- No code refactoring needed
- Works immediately

**Cons:**
- Template becomes stale if CDK code changes
- Limited parameter customization (form fields must be hardcoded in template)
- Requires manual template export after each CDK update
- Not ideal for ongoing maintenance

**Steps:**
1. Run `cdk synth` to generate templates in `cdk.out/`
2. Copy the synthesized template to repository (e.g., `aws-cloudformation-template.yaml`)
3. Add Launch Button to README pointing to that template
4. Document: "Run `cdk synth` after CDK changes and commit template"

---

### Option 2: Hybrid Approach (Recommended) 🟢 BALANCED

**Effort:** 2-3 hours  
**Compatibility:** 85% (Full feature parity)

Create a **hand-written CloudFormation template** alongside your CDK code. The template provides all the same functionality but is:
- Version-controlled
- Publicly accessible
- Updatable independently
- Maintainable by both PowerShell and Portal users

**Pros:**
- Launch Button works perfectly
- Both CDK and CloudFormation users are supported
- No template drift issues
- User choice: CDK for advanced users, Button for beginners

**Cons:**
- Requires maintaining two infrastructure-as-code approaches
- Must keep CloudFormation in sync with CDK
- Small overhead per deployment change

**Steps:**
1. Export synthesized template from `cdk.out/` as reference
2. Create hand-written `skycms-cloudformation.yaml` (copy and clean up)
3. Define CloudFormation Parameters:
   - Editor Docker Image
   - Database Name
   - Desired ECS Count
   - Optional: Domain name, TLS certificate
4. Test template works standalone
5. Commit to repository
6. Add Launch Button to README

**Best For:** Your use case. Gives users choice between CLI and Portal.

---

### Option 3: Full CDK Integration (Advanced) 🔴 MOST COMPLEX

**Effort:** 4-6 hours  
**Compatibility:** 95% (Full feature parity with complexity)

Use AWS SAM (Serverless Application Model) or CDK's `@aws-cdk/cloudformation-include` to publish CDK outputs as static templates.

**Pros:**
- Single source of truth (CDK code)
- CI/CD pipeline auto-publishes templates
- No drift between CDK and CloudFormation

**Cons:**
- Requires CI/CD pipeline setup (GitHub Actions)
- More complex to maintain
- Overkill if not deploying frequently

**Steps:**
1. Set up GitHub Actions workflow
2. On commit to main: run `cdk synth`, upload template to S3
3. Add Launch Button pointing to S3 URL
4. Configure S3 to serve public templates

**Best For:** Large teams with frequent deployments and CI/CD pipelines.

---

## Current Setup Analysis

### CDK Stack Definition

Your `skycms-stack.ts` includes:

```typescript
✅ VPC Configuration         (subnets, security groups)
✅ ECS Cluster               (Fargate with Docker image)
✅ RDS MySQL Database        (TLS enforcement, auto-credentials)
✅ CloudFront Distribution   (CDN with ALB origin)
✅ Secrets Manager           (auto-generated DB passwords)
✅ Security Groups           (ALB → ECS → RDS)
✅ Optional TLS/Route 53     (custom domain support)
```

**Current Parameters (via PowerShell):**
- `--context image=<docker-image>`
- `--context desiredCount=<1-10>`
- `--context dbName=<database-name>`
- `--context stackName=<stack-name>`
- `--context editorCacheEnabled=<true|false>`
- `--context publisherCacheEnabled=<true|false>`

**CloudFormation Can Support All Of These** ✅

---

## Recommendation: Option 2 (Hybrid)

I recommend **Option 2 - Hybrid Approach** because:

1. **Serves Both Audiences**
   - Advanced users: Use CDK (`cdk-deploy.ps1`) for maximum control
   - Beginners: Use Launch Button for simplicity

2. **Low Maintenance**
   - CloudFormation template is a cleaned-up export of CDK synth output
   - Both approaches deploy identical infrastructure
   - Syncing is straightforward (compare outputs after CDK changes)

3. **Best User Experience**
   - Users don't need to understand CDK
   - No tool installation required for Portal deployment
   - Same 15-20 minute deployment time

4. **Professional Presentation**
   - Consistent with Azure deployment approach
   - Shows multi-cloud support clearly
   - Impressive one-click deployment capability

---

## Implementation Checklist for Option 2

```
Preparation Phase
  [ ] Document current CDK context parameters
  [ ] List all CloudFormation outputs needed
  [ ] Identify all required user inputs

Template Creation Phase
  [ ] Run: cdk synth (generates templates in cdk.out/)
  [ ] Copy synthesized template as reference
  [ ] Create aws-cloudformation.yaml (hand-written version)
  [ ] Map CDK constructs → CloudFormation resources:
      [ ] VPC (subnets, route tables, security groups)
      [ ] ECS Cluster & Service (Fargate)
      [ ] RDS MySQL Instance
      [ ] CloudFront Distribution & ALB
      [ ] Secrets Manager (DB credentials)
      [ ] IAM Roles for ECS task execution
      [ ] CloudWatch Log Groups
      [ ] Optional: Route 53 & ACM (TLS)

Parameter Definition Phase
  [ ] Define CloudFormation Parameters:
      [ ] EditorImageUri (default: toiyabe/sky-editor:latest)
      [ ] DesiredTaskCount (default: 1, allowed: 1-10)
      [ ] DatabaseName (default: skycms)
      [ ] StackName (alphanumeric, 3-32 chars)
      [ ] EditorCacheEnabled (yes/no)
      [ ] PublisherCacheEnabled (yes/no)
      [ ] (Optional) DomainName (for custom domain)
      [ ] (Optional) HostedZoneId (Route 53 zone)
      [ ] (Optional) CertificateArn (existing ACM cert)

Testing Phase
  [ ] Deploy template via CloudFormation console manually
  [ ] Verify all resources created correctly
  [ ] Test Editor application loads
  [ ] Verify database connectivity
  [ ] Confirm CloudFront distribution works
  [ ] Check all Outputs displayed correctly

Documentation Phase
  [ ] Create QUICKSTART_LAUNCH_BUTTON_AWS.md
  [ ] Document post-deployment steps (same as current)
  [ ] Add troubleshooting for common issues
  [ ] Link from main README

Integration Phase
  [ ] Add metadata.json for AWS Launch Button customization
  [ ] Update main README with Launch Button
  [ ] Test button links resolve correctly
  [ ] Verify parameter form fields populate properly

Maintenance Phase
  [ ] After CDK code changes:
      [ ] Run cdk synth
      [ ] Compare outputs with current template
      [ ] Update CloudFormation template if needed
      [ ] Document what changed
      [ ] Test template still works
```

---

## File Structure (After Implementation)

```
InstallScripts/AWS/
├── README.md                          (← Add Launch Button here)
├── QUICKSTART_LAUNCH_BUTTON_AWS.md   (← New post-deploy guide)
├── aws-cloudformation.yaml            (← New hand-written template)
├── metadata.json                      (← New: button customization)
├── cdk-deploy.ps1                     (← Keep: for advanced users)
├── cdk-destroy.ps1                    (← Keep unchanged)
├── cdk/                               (← Keep: CDK source)
├── TEMPLATE_COMPARISON.md             (← New: CDK vs CloudFormation)
└── AWS_CDK_INTERACTIVE_DEPLOYMENT.md (← Keep unchanged)
```

---

## Launch Button URLs (Once Ready)

**Main README Button:**
```markdown
[![Launch Stack](https://s3.amazonaws.com/cloudformation-examples/cloudformation-launch-stack.png)](https://console.aws.amazon.com/cloudformation/home?region=us-east-1#/stacks/new?stackName=skycms&templateURL=https://raw.githubusercontent.com/YOUR_USERNAME/SkyCMS/main/InstallScripts/AWS/aws-cloudformation.yaml)
```

**Parameters Pre-filled (Optional):**
- `stackName=skycms` - Default stack name
- Region defaults to `us-east-1`

---

## Key Differences: CDK vs CloudFormation Button

| Aspect | CDK (PowerShell) | CloudFormation Button |
|--------|-----------------|----------------------|
| **User Skill** | Intermediate (CLI tools) | Beginner (web browser) |
| **Setup Time** | 20 min (includes tool install) | 15 min (no tools) |
| **Infrastructure** | Same | Same |
| **Customization** | Full control via parameters | Basic via form fields |
| **Cost** | Identical | Identical |
| **Support** | PowerShell script docs | AWS Portal + guide |

---

## Cost Implications

**Both deployment methods create identical infrastructure, so costs are the same:**

| Component | Est/Month |
|-----------|-----------|
| ECS Fargate (1 task, 0.5 vCPU, 1GB RAM) | $15-25 |
| RDS MySQL (db.t4g.micro, burstable) | $10-15 |
| CloudFront (10GB/month) | $2-5 |
| Other (Secrets, logs, etc.) | $2-3 |
| **Total** | **~$30-50/month** |

---

## Next Steps

Would you like me to:

1. **Start Implementation** → Create the hand-written CloudFormation template (Option 2)
2. **Just Export** → Export current CDK synth as template (Option 1, quick)
3. **Get More Info** → Detailed template structure or specific parameter definitions
4. **Compare Approaches** → See detailed pros/cons of each option

Let me know which direction you'd prefer!
