#!/usr/bin/env pwsh
# Validate CloudFormation template using AWS CLI
# Based on aws-samples/ecs-refarch-cloudformation validation approach

$ErrorCount = 0

Write-Host "Validating AWS CloudFormation template..." -ForegroundColor Cyan

$Template = "skycms-editor-fargate.yml"

Write-Host "`nValidating: $Template" -ForegroundColor Yellow

try {
    # Run AWS CloudFormation validation
    $Result = aws cloudformation validate-template --template-body "file://$Template" 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "[PASS] $Template" -ForegroundColor Green
        
        # Parse and display template details
        $TemplateInfo = $Result | ConvertFrom-Json
        Write-Host "`nTemplate Details:" -ForegroundColor Cyan
        Write-Host "  Description: $($TemplateInfo.Description)"
        Write-Host "  Parameters: $($TemplateInfo.Parameters.Count)"
        
        if ($TemplateInfo.Parameters.Count -gt 0) {
            Write-Host "`n  Parameter List:"
            foreach ($param in $TemplateInfo.Parameters) {
                Write-Host "    - $($param.ParameterKey): $($param.Description)"
            }
        }
    }
    else {
        $ErrorCount++
        Write-Host "[FAIL] $Template" -ForegroundColor Red
        Write-Host "Error: $Result" -ForegroundColor Red
    }
}
catch {
    $ErrorCount++
    Write-Host "[FAIL] $Template" -ForegroundColor Red
    Write-Host "Exception: $_" -ForegroundColor Red
}

Write-Host "`n$ErrorCount template validation error(s)" -ForegroundColor $(if ($ErrorCount -gt 0) { 'Red' } else { 'Green' })

if ($ErrorCount -gt 0) {
    exit 1
}

Write-Host "`n✓ CloudFormation template is valid!" -ForegroundColor Green
exit 0
