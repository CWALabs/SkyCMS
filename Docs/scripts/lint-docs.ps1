# Run link and anchor checks against the Docs site using lychee (via Docker) to avoid local installs.
# Requires Docker Desktop. Example: ./scripts/lint-docs.ps1

param(
    [string]$Target = "https://cwalabs.github.io/SkyCMS/Docs/",
    [string]$Config = ""
)

$cmd = @(
    "docker", "run", "--rm",
    "ghcr.io/lycheeverse/lychee:latest",
    "--no-progress",
    "--accept", "https://localhost,https://127.0.0.1",
    "--max-redirects", "5",
    "--user-agent", "SkyCMS-Docs-Lychee",
    "--verbose"
)

if ($Config -ne "") {
    $cmd += @("--config", "/workspace/lychee.toml")
}

$cmd += $Target

Write-Host "Running: $($cmd -join ' ')" -ForegroundColor Cyan
& $cmd

if ($LASTEXITCODE -ne 0) {
    Write-Error "Link check failed with code $LASTEXITCODE"
    exit $LASTEXITCODE
}

Write-Host "Link check passed" -ForegroundColor Green
