# How to Create a GitHub Release

## Step 1: Create the Package

Run the packaging script:
```powershell
cd InstallScripts\AWS
.\create-release-package.ps1
```

This creates `skycms-aws-deployment-v1.0.0.zip` (update version in script as needed).

## Step 2: Create GitHub Release

### Via GitHub Web Interface (Easiest):

1. **Go to your repository** on GitHub.com

2. **Click "Releases"** (on the right sidebar)

3. **Click "Draft a new release"**

4. **Fill in the release form**:
   - **Tag version**: `v1.0.0` (create new tag)
   - **Release title**: `SkyCMS AWS Deployment v1.0.0`
   - **Description**: 
     ```markdown
     ## SkyCMS AWS Deployment Package
     
     Deploy SkyCMS to AWS using CDK with a single PowerShell script.
     
     ### What's Included
     - Interactive deployment script (`cdk-deploy.ps1`)
     - Cleanup script (`cdk-destroy.ps1`)
     - CDK infrastructure code
     - Quick start guide
     
     ### Prerequisites
     - AWS CLI configured
     - Node.js 18+
     - Docker image available
     
     ### Quick Start
     1. Download and extract `skycms-aws-deployment-v1.0.0.zip`
     2. Run `.\cdk-deploy.ps1`
     3. Follow interactive prompts
     
     ### What Gets Deployed
     - VPC with public/private subnets
     - RDS MySQL database
     - ECS Fargate cluster
     - Application Load Balancer
     - CloudFront distribution
     - S3 bucket (optional)
     
     See QUICK_START.md inside the package for full details.
     ```

5. **Attach the zip file**:
   - Drag and drop `skycms-aws-deployment-v1.0.0.zip` into the release assets area
   - Or click "Attach binaries" and select the file

6. **Publish release**
   - For production: Click "Publish release"
   - For testing: Check "Set as a pre-release" first

## Step 3: Users Download

Users can now:
1. Go to your repository's Releases page
2. Download `skycms-aws-deployment-v1.0.0.zip`
3. Extract it anywhere on their machine
4. Run `.\cdk-deploy.ps1`

**No need to clone the entire repository!**

## Alternative: Using GitHub CLI

If you have GitHub CLI installed:

```powershell
# Create the package first
.\create-release-package.ps1

# Create release and upload
gh release create v1.0.0 `
  skycms-aws-deployment-v1.0.0.zip `
  --title "SkyCMS AWS Deployment v1.0.0" `
  --notes "Standalone AWS deployment package. See QUICK_START.md for instructions."
```

## Updating for New Versions

1. Update version in `create-release-package.ps1`
2. Run `.\create-release-package.ps1`
3. Create new GitHub release with new tag (e.g., `v1.1.0`)
4. Upload new zip file

## Best Practices

- **Tag naming**: Use semantic versioning (v1.0.0, v1.1.0, v2.0.0)
- **Changelog**: List what changed in each version
- **Pre-releases**: Use for beta/testing versions
- **Asset naming**: Include version in zip filename
- **Documentation**: Keep QUICK_START.md updated

## What Users Get

When users download your release package, they get:
```
skycms-aws-deployment/
├── QUICK_START.md          ← Installation instructions
├── README.md               ← Full documentation
├── cdk-deploy.ps1          ← Deployment script
├── cdk-destroy.ps1         ← Cleanup script
└── cdk/                    ← CDK infrastructure
    ├── package.json
    ├── tsconfig.json
    ├── cdk.json
    └── lib/
        └── skycms-stack.ts
```

They do NOT need:
- The entire SkyCMS source code
- Your .NET projects
- Other installation scripts
- Git history

This makes it lightweight and focused!
