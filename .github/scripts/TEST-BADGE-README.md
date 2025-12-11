# Test Badge for SkyCMS

This badge displays comprehensive test results including:
- ✓ Tests passed
- ✗ Tests failed  
- ⊘ Tests skipped
- % Code coverage

## Badge Display

The badge shows: `[tests] [20✓ 0✗ 1⊘ | 85.3%]`

### Badge Colors

The badge color indicates overall test health:
- **🟢 Bright Green** - All tests passed, coverage ≥ 80%
- **🟡 Yellow** - All tests passed, coverage 60-79%
- **🟠 Orange** - All tests passed, coverage < 60%
- **🔴 Red** - One or more tests failed

## Usage in README

Add this to your README.md to display the badge:

```markdown
![Test Results](./test-badge.svg)
```

## How It Works

1. **Run Tests** - The workflow runs all unit tests with code coverage collection
2. **Generate Coverage Report** - ReportGenerator creates a JSON summary of coverage
3. **Parse Results** - PowerShell script parses the TRX file and coverage JSON
4. **Create Badge** - An SVG badge is generated with all metrics
5. **Commit Badge** - The badge is committed to the main branch (only on main branch runs)

## Files

- **Workflow**: `.github/workflows/sky-tests.yml`
- **Badge Script**: `.github/scripts/generate-test-badge.ps1`
- **Badge Output**: `test-badge.svg` (root of repository)

## Manual Badge Generation

You can also generate the badge locally after running tests:

```powershell
# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults

# Install ReportGenerator if needed
dotnet tool install -g dotnet-reportgenerator-globaltool

# Generate coverage report
reportgenerator `
  -reports:"TestResults/**/coverage.cobertura.xml" `
  -targetdir:"TestResults/CoverageReport" `
  -reporttypes:"Html;JsonSummary"

# Generate badge
.\.github\scripts\generate-test-badge.ps1
```

## Customization

Edit `.github/scripts/generate-test-badge.ps1` to customize:

- **Badge dimensions** - Modify SVG `width` and `height`
- **Color thresholds** - Adjust coverage percentage breakpoints
- **Badge text format** - Change how metrics are displayed
- **Symbols** - Replace ✓, ✗, ⊘ with other characters

## Coverage Thresholds

Current thresholds:
- **< 60%** - Orange
- **60-79%** - Yellow  
- **≥ 80%** - Bright Green

(Red overrides all if any test fails)
