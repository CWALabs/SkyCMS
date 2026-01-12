# Quick Start: Running JavaScript Tests

## TL;DR - Just Run Tests

```bash
# Navigate to test directory
cd JestTests

# Run unit tests (fast, no setup required)
npm test

# Run integration tests (requires running API)
npm run test:integration

# Run everything (sequential)
npm run test:all
```

## Three-Minute Setup

### Prerequisites
- Node.js 16+ installed
- npm available in terminal
- (Optional) C# API running on localhost:5000 for integration tests

### Step 1: Install Dependencies
```bash
cd JestTests
npm install
```

### Step 2: Run Unit Tests
```bash
npm test
# Tests should pass within 1 second
```

### Step 3: (Optional) Run Integration Tests

**Important:** Sky.Cms.Api.Shared is a **class library**, not a runnable application. The Contact API endpoints are hosted by the **Editor** application.

**Terminal 1 - Start the Editor (which hosts the API):**
```bash
cd Editor
dotnet run
# Wait for "Now listening on" message
# The Contact API will be available at http://localhost:5000/_api/contact/
```

**Terminal 2 - Run tests:**
```bash
cd JestTests
npm run test:integration
# Tests will make real HTTP calls to API
```

## Common Scenarios

### Scenario 1: Developing JavaScript Locally

```bash
cd JestTests
npm test -- --watch

# Tests re-run automatically when files change
# Press 'q' to quit watch mode
```

### Scenario 2: Before Committing Code

```bash
cd JestTests
npm test                          # Run all unit tests
npm test -- --coverage            # See coverage report
```

### Scenario 3: Before Deploying

```bash
# Terminal 1: Start API in Release mode
cd Sky.Cms.Api.Shared
dotnet run --configuration Release

# Terminal 2: Run complete test suite
cd JestTests
npm run test:all
# Both unit and integration tests run sequentially
```

## Understanding Test Output

### ✅ Unit Tests Pass
```
PASS  tests/unit/api/skycms-contact.test.js
  SkyCmsContact Unit Tests
    ✓ should initialize form with selector string
    ✓ should initialize form with element reference
    ... (more tests)

Test Suites: 2 passed, 2 total
Tests:       45 passed, 45 total
Time:        0.892 s
```
**Result**: Everything works! Continue to integration tests if needed.

### ⚠️ Integration Tests Skip (API Not Running)
```
PASS  tests/integration/api/contact-api.integration.test.js
  ⚠️  API not available at http://localhost:5000
  Skipping integration tests. Start the API with:
    cd Sky.Cms.Api.Shared
    dotnet run

  Contact API Integration Tests
    ⊘ should return JavaScript library (skipped)
    ⊘ should submit form (skipped)
    ... (more skipped)

Tests: 12 skipped, 12 total
Time:  0.234 s
```
**Result**: Expected. Start API and re-run to execute integration tests.

### ❌ Unit Tests Fail
```
FAIL  tests/unit/api/skycms-contact.test.js
  SkyCmsContact Unit Tests
    ✓ should initialize form with selector string
    ✗ should initialize form with element reference
      Expected: true
      Received: false
      at Object.<anonymous> (tests/unit/api/skycms-contact.test.js:25:10)

Test Suites: 1 failed, 1 passed
Tests:       1 failed, 45 passed
Time:        0.892 s
```
**Action**: Fix the failing test or the code. See troubleshooting section below.

### ❌ Integration Tests Fail (API Running)
```
FAIL  tests/integration/api/contact-api.integration.test.js
  Contact API Integration Tests
    GET /_api/contact/skycms-contact.js
      ✗ should return JavaScript library
        TypeError: fetch failed (ECONNREFUSED - Connection refused)
```
**Action**: Verify API is running on the correct port. Check API_URL environment variable.

## Troubleshooting

### Problem: "Cannot find module '@jest/globals'"

**Solution**:
```bash
cd JestTests
npm install
```

### Problem: Tests timeout or hang

**Solution**:
```bash
# Check if Node process is stuck
ps aux | grep node

# Kill any stuck processes
pkill node

# Clear Jest cache
npm test -- --clearCache

# Try again
npm test
```

### Problem: Integration tests timeout

**Symptoms**: 
```
FAIL tests/integration/api/contact-api.integration.test.js
  Timeout - Async callback was not invoked within the 10000 ms timeout
```

**Solution**:
```bash
# Check if API is running
curl http://localhost:5000/_api/contact/skycms-contact.js
# Should return JavaScript code

# If API is not running, start the Editor application (NOT Sky.Cms.Api.Shared)
cd ../Editor
dotnet run

# If API is running but tests still timeout, increase timeout
# Edit tests/integration/api/contact-api.integration.test.js
# Add at top: jest.setTimeout(30000);
```

### Problem: "EADDRINUSE: address already in use"

**Cause**: API is already running on port 5000

**Solution**:
```bash
# Check what's using port 5000
netstat -ano | findstr :5000

# Kill the process (replace PID with actual process ID)
taskkill /PID <PID> /F

# Or use different port
set API_PORT=5001
```

### Problem: Tests pass locally but fail in CI/CD

**Checklist**:
- [ ] Node version matches (check package.json engines)
- [ ] Dependencies installed with `npm ci` not `npm install`
- [ ] Cache cleared: `npm test -- --clearCache`
- [ ] Environment variables set correctly
- [ ] C# API running for integration tests

**Solution**:
```bash
npm ci                    # Clean install
npm test -- --clearCache  # Clear cache
npm test -- --verbose     # Run with verbose output
```

### Problem: Coverage report shows 0%

**Solution**:
```bash
npm test -- --clearCache
npm test -- --coverage
```

## Available npm Commands

```bash
npm test                    # Run unit tests once
npm test -- --watch         # Run unit tests in watch mode
npm test -- --coverage      # Run unit tests with coverage report
npm test -- --verbose       # Run with verbose output
npm test -- --clearCache    # Clear Jest cache

npm run test:integration    # Run integration tests
npm run test:all            # Run unit tests, then integration tests

npm run lint                # Check code style (if configured)
npm run format              # Format code (if configured)
```

## Environment Variables

For integration tests, you can configure:

```bash
# Unix/Linux/macOS
export API_URL=http://localhost:5000
export API_PORT=5000

# Windows Command Prompt
set API_URL=http://localhost:5000
set API_PORT=5000

# Windows PowerShell
$env:API_URL="http://localhost:5000"
$env:API_PORT="5000"
```

Or create a `.env` file in the JestTests directory:
```
API_URL=http://localhost:5000
API_PORT=5000
```

## What's Being Tested

### Unit Tests (tests/unit/api/)
- SkyCmsContact JavaScript library
- Form initialization and configuration
- Field name mapping
- Form submission logic
- Error handling
- Callback execution
- **No network calls** (all mocked)

### Integration Tests (tests/integration/api/)
- Real API endpoint: `GET /_api/contact/skycms-contact.js`
- Real API endpoint: `POST /_api/contact/submit`
- Real validation rules
- Real rate limiting
- Real CAPTCHA configuration
- Real field name support
- **Real network calls** (requires running API)

## Next Steps

1. **Run unit tests** → `npm test`
2. **Read results** → Check output for failures
3. **Fix any issues** → Edit code or tests
4. **Commit changes** → Tests should pass
5. **Before deploying** → Run `npm run test:all`

## Still Stuck?

1. Check [TESTING.md](./TESTING.md) for detailed documentation
2. Review example tests in `tests/unit/api/` and `tests/integration/api/`
3. Check [Jest documentation](https://jestjs.io/)
4. Verify C# API is running correctly for integration tests
5. Check console output for detailed error messages

## Key Files

- `package.json` - npm scripts and Jest configuration
- `tests/unit/api/` - Unit test files
- `tests/integration/api/` - Integration test files
- `tests/setup/setup.js` - Jest global setup
- `tests/setup/api-config.js` - API configuration for integration tests
- `TESTING.md` - Comprehensive testing guide
- `README.md` - Jest setup guide
