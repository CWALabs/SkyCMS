# UploadSecretsToGithubRepo.ps1 Documentation

## Overview

`UploadSecretsToGithubRepo.ps1` is a PowerShell script designed to automate the process of copying secrets from a local secrets file and uploading them as GitHub repository secrets. This is useful for synchronizing sensitive configuration values between your local development environment and your GitHub repository, ensuring that your CI/CD pipelines and GitHub Actions have access to the required secrets.

## How It Works

- The script reads secrets from a specified local secrets file (such as `secrets.json` or `.env`).
- It uses the GitHub CLI (`gh`) to authenticate and interact with the GitHub repository.
- For each secret found in the local file, the script creates or updates the corresponding secret in the target GitHub repository.
- The script may prompt for authentication if you are not already logged in with the GitHub CLI.

## Prerequisites

- **PowerShell**: The script is written for PowerShell and should be run in a PowerShell terminal.
- **GitHub CLI (`gh`)**: You must have the GitHub CLI installed and authenticated. Download from [GitHub CLI](https://cli.github.com/).
- **Access to the target GitHub repository**: You need appropriate permissions to set secrets in the repository.
- **Local secrets file**: Ensure your secrets file is present and formatted as expected by the script.

## Usage

1. **Open PowerShell**
   - Open a PowerShell terminal in the root directory of your project.

2. **Authenticate with GitHub CLI (if not already authenticated)**
   ```powershell
   gh auth login
   ```

3. **Run the Script**
   ```powershell
   .\UploadSecretsToGithubRepo.ps1 -SecretsFilePath <path-to-secrets-file> -Repo <owner/repo>
   ```
   - Replace `<path-to-secrets-file>` with the path to your local secrets file (e.g., `./secrets.json`).
   - Replace `<owner/repo>` with the GitHub repository name (e.g., `MoonriseSoftwareCalifornia/SkyCMS`).

4. **Example**
   ```powershell
   .\UploadSecretsToGithubRepo.ps1 -SecretsFilePath ./secrets.json -Repo MoonriseSoftwareCalifornia/SkyCMS
   ```

## Notes

- The script will overwrite existing secrets in the GitHub repository with the same names.
- Ensure you do not commit your local secrets file to version control.
- For more details, review the script source code in `UploadSecretsToGithubRepo.ps1`.
