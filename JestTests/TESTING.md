# JavaScript Testing Guide - Sky.Cms.Api.Shared Contact API

## Overview

This guide covers testing the dynamically generated SkyCMS Contact API JavaScript library with a **staged testing approach** for different levels of validation.

## Why Staged Testing?

The Contact API presents unique challenges:
- **Generated JavaScript** with embedded configuration and security tokens
- **Backend dependencies** for validation, email, and CAPTCHA
- **Async operations** that require network calls
- **Different environments** with varying configurations

**Solution**: Use three test stages, each appropriate for different scenarios:

| Stage | Type | Speed | Setup | When to Run |
|-------|------|-------|-------|------------|
| **1** | Unit | ~100ms | None | Constantly (dev, pre-commit, CI) |
| **2** | Integration | ~1-5s | Running API | Pre-deployment, API changes |
| **3** | E2E (Planned) | ~5-10s | Browser + API | Major releases, staging |

## Stage 1: Unit Tests (Pure JavaScript)

### Purpose
Test the SkyCmsContact JavaScript library in isolation with all network calls mocked.

### What Gets Tested
- Configuration defaults and initialization
- Form element selection and validation  
- Field name mapping (default and custom)
- Form submission construction
- Callback execution (onSuccess, onError)
- Error handling paths
- Input sanitization

### Running Unit Tests

```bash
# Fast iteration during development
npm test -- --watch

# Run once
npm test

# Run specific test file
npm test -- skycms-contact.test.js

# Run specific test pattern
npm test -- --testNamePattern="field mapping"

# With coverage report
npm test -- --coverage

# Clear Jest cache if tests seem stale
npm test -- --clearCache
```

### Example Unit Test

```javascript
import { describe, test, expect } from '@jest/globals';

describe('SkyCmsContact Unit Tests', () => {
  test('should initialize form with selector string', () => {
    // Create test DOM
    const form = document.createElement('form');
    form.id = 'contact-form';
    document.body.appendChild(form);

    // Initialize with selector
    SkyCmsContact.init({
      formSelector: '#contact-form',
      submitEndpoint: '/api/contact/submit'
    });

    // Verify form was initialized
    expect(SkyCmsContact.form).toBeDefined();
    expect(SkyCmsContact.form.id).toBe('contact-form');
  });

  test('should map custom field names correctly', () => {
    // Test field name override capability
    const config = {
      formSelector: '#contact-form',
      fieldNames: {
        name: 'fullName',
        email: 'emailAddress',
        message: 'comments'
      }
    };

    SkyCmsContact.init(config);

    expect(SkyCmsContact.config.fieldNames.name).toBe('fullName');
    expect(SkyCmsContact.config.fieldNames.email).toBe('emailAddress');
  });

  test('should call onSuccess callback on successful submission', async () => {
    // Mock fetch
    global.fetch = jest.fn(() =>
      Promise.resolve({
        ok: true,
        json: () => Promise.resolve({ success: true })
      })
    );

    const onSuccess = jest.fn();

    SkyCmsContact.init({
      formSelector: '#contact-form',
      onSuccess: onSuccess
    });

    // Simulate form submission
    await SkyCmsContact.handleSubmit({ preventDefault: () => {} });

    expect(onSuccess).toHaveBeenCalled();
  });
});
```

### Best Practices for Unit Tests

1. **Mock all network calls** - Use `jest.fn()` for fetch
2. **Mock all callbacks** - Verify callbacks are called correctly
3. **Test behavior, not implementation** - Focus on user-facing functionality
4. **Keep tests fast** - Should complete in milliseconds, not seconds
5. **Test error cases** - Invalid input, network errors, missing fields
6. **Use descriptive names** - `should_BEHAVIOR_when_CONDITION` pattern
7. **Set up test DOM** - Create necessary form elements in beforeEach
8. **Clean up after** - Remove DOM elements in afterEach

### Test Files

**Main test files in `tests/unit/api/`:**

1. **skycms-contact.test.js** - Core library tests
   - Configuration and initialization
   - Field name mapping  
   - Form submission
   - Callbacks and error handling
   - 40+ test cases

2. **generated-script.test.js** - Generated script validation
   - Global SkyCmsContact object structure
   - Required methods presence
   - Embedded configuration

## Stage 2: Integration Tests (JavaScript + Real API)

### Purpose
Test the SkyCmsContact JavaScript library against a real running C# API.

### What Gets Tested
- Generated JavaScript library retrieval and structure
- Antiforgery token presence and validity
- Form submission with real validation  
- Invalid email rejection
- Missing field rejection
- Message length enforcement
- Rate limiting enforcement
- CAPTCHA integration verification
- Custom field name handling in real scenarios
- HTTP status codes and response format

### Prerequisites

1. **Editor application running** - The Contact API is hosted by the Editor app, not Sky.Cms.Api.Shared (which is a class library)
2. **Configuration** - API URL (defaults to http://localhost:5000)
3. **Network connectivity** - Test machine can reach API

### Starting the API

**Important:** Sky.Cms.Api.Shared is a class library and cannot be run directly. The Contact API endpoints are registered in the Editor application.

```bash
# Terminal 1: Start Editor application (which hosts the Contact API)
cd Editor
dotnet run

# Editor will start on https://localhost:5001 or http://localhost:5000
# Watch for "Now listening on:" message
# The Contact API endpoints will be available at /_api/contact/*

# Verify API is running
curl http://localhost:5000/_api/contact/skycms-contact.js
# Should return JavaScript code
```

### Running Integration Tests

```bash
# Terminal 2: Run integration tests
cd JestTests

# Run all integration tests
npm run test:integration

# Run specific test file
npm run test:integration -- tests/integration/api/contact-api.integration.test.js

# Run specific test
npm run test:integration -- --testNamePattern="should accept valid"

# With verbose output
npm run test:integration -- --verbose
```

### API Unavailable Behavior

If the API isn't running, integration tests:
- **Do NOT fail** - They skip gracefully with a warning message
- **Show message** explaining how to start API
- **Allow CI/CD to pass** - No blocking failures for optional integration tests

Example output:
```
⚠️  API not available at http://localhost:5000
Skipping integration tests. Start the API with:
  cd Sky.Cms.Api.Shared
  dotnet run
```

### Example Integration Test

```javascript
import { describe, test, expect, beforeAll } from '@jest/globals';
import { getApiUrl, isApiAvailable } from '../setup/api-config';

describe('Contact API Integration Tests', () => {
  let apiUrl;
  let apiAvailable = false;

  beforeAll(async () => {
    apiUrl = getApiUrl();
    apiAvailable = await isApiAvailable(apiUrl);

    if (!apiAvailable) {
      console.warn(`⚠️  API not available at ${apiUrl}`);
      console.warn('Skipping integration tests. Start the API with:');
      console.warn('  cd Sky.Cms.Api.Shared');
      console.warn('  dotnet run');
    }
  });

  test('should fetch generated JavaScript from API', async () => {
    if (!apiAvailable) return; // Skip if API not running

    const response = await fetch(`${apiUrl}/_api/contact/skycms-contact.js`);
    
    expect(response.ok).toBe(true);
    expect(response.headers.get('content-type')).toContain('application/javascript');
    
    const script = await response.text();
    expect(script).toContain('SkyCmsContact');
  });

  test('should submit valid form to real API', async () => {
    if (!apiAvailable) return;

    const response = await fetch(`${apiUrl}/_api/contact/submit`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        name: 'Integration Test',
        email: 'integration@test.com',
        message: 'This is a real API integration test'
      })
    });

    const result = await response.json();
    expect(result.success).toBe(true);
  });

  test('should reject invalid email on real API', async () => {
    if (!apiAvailable) return;

    const response = await fetch(`${apiUrl}/_api/contact/submit`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        name: 'Test',
        email: 'invalid-email',  // Invalid format
        message: 'Should fail validation'
      })
    });

    const result = await response.json();
    expect(result.success).toBe(false);
    expect(result.error).toBeTruthy();
  });
});
```

### Best Practices for Integration Tests

1. **Test the real API contract** - Don't mock API endpoints
2. **Handle API unavailability gracefully** - Skip tests, don't fail
3. **Use realistic test data** - Match production scenarios
4. **Clean up test data** - Don't leave junk in backend (if applicable)
5. **Test error responses** - Verify API validation works
6. **Document setup requirements** - Make it clear what needs running
7. **Include proper waits** - Network calls may be slower than unit tests
8. **Verify response structure** - Check all required fields present

### Configuration

```bash
# Set API URL (optional, defaults to http://localhost:5000)
export API_URL=http://localhost:5000

# Or on Windows:
set API_URL=http://localhost:5000

# Or in PowerShell:
$env:API_URL="http://localhost:5000"

# Or create .env file in JestTests directory:
# API_URL=http://localhost:5000
```

### Test Files

**Main test files in `tests/integration/api/`:**

1. **contact-api.integration.test.js** - Real API interaction
   - Script retrieval and validation
   - Form submission success/failure paths
   - Input validation enforcement
   - Rate limiting verification
   - CAPTCHA configuration checks
   - Custom field name support
   - ~15 integration test cases

## Running All Tests Together

```bash
# Run unit tests first, then integration tests
npm run test:all

# Equivalent to:
# 1. npm test (unit tests)
# 2. npm run test:integration (integration tests)

# Useful for:
# - Final validation before commit
# - Pre-deployment checks
# - CI/CD pipelines
```

### Test Execution Flow

```
npm run test:all
├── Unit Tests
│   ├── skycms-contact.test.js ✓
│   └── generated-script.test.js ✓
│
└── Integration Tests (if API running)
    ├── contact-api.integration.test.js
    │   ├── GET /_api/contact/skycms-contact.js ✓
    │   ├── POST /_api/contact/submit ✓
    │   ├── Validation enforcement ✓
    │   ├── Rate limiting ✓
    │   └── Field name mapping ✓
```

## Stage 3: End-to-End Tests (Planned)

### Purpose
Test complete user experience with browser automation using Playwright.

### What Will Be Tested
- Form rendering in real browsers
- User input handling and validation feedback
- Real CAPTCHA interaction
- Form submission with visual feedback
- Success/error message display
- Accessibility compliance
- Different browsers (Chrome, Firefox, Safari)

### Status
Not yet implemented. Planned for future enhancement.

### Recommended Setup (when implemented)

```bash
npm install -D @playwright/test

# Run E2E tests
npm run test:e2e

# Run with browser visible
npm run test:e2e -- --headed

# Run in specific browser
npm run test:e2e -- --project=firefox
```

## Development Workflow

### During Active Development

```bash
# Keep unit tests running in watch mode
npm test -- --watch

# In another terminal, start Editor application (hosts the Contact API)
cd ../Editor
dotnet run

# In another terminal, optionally run integration tests
cd JestTests
npm run test:integration
```

### Before Committing Code

```bash
# Run all unit tests
npm test

# Check for test coverage regressions
npm test -- --coverage

# If you modified API integration points, test with real API
npm run test:integration
```

### Before Deploying to Production

```bash
# 1. Run all unit tests
npm test

# 2. Start Editor application in staging/production mode
cd Editor
dotnet run --configuration Release

# 3. Run full test suite
cd JestTests
npm run test:all

# 4. Review coverage report
npm test -- --coverage
```

## Troubleshooting

### Unit Tests Fail

**Symptom**: Tests pass locally but fail in CI/CD

**Diagnosis**:
```bash
npm test -- --showConfig
npm test -- --verbose
node --version
npm --version
```

**Solutions**:
- Ensure Node version is consistent (use `.nvmrc` file)
- Install dependencies with `npm ci` instead of `npm install`
- Clear Jest cache: `npm test -- --clearCache`
- Check for hardcoded paths or environment assumptions

### Integration Tests Timeout

**Symptom**: Integration tests hang or timeout

**Diagnosis**:
```bash
# Check if API is running
curl http://localhost:5000/_api/contact/skycms-contact.js

# Check connectivity
ping localhost

# Verify API_URL setting
echo $API_URL
```

**Solutions**:
- Start the API: `cd Sky.Cms.Api.Shared && dotnet run`
- Check firewall/network connectivity
- Increase timeout: Add to integration test file:
  ```javascript
  jest.setTimeout(30000); // 30 seconds
  ```
- Verify API_URL environment variable

### CAPTCHA Tests Fail

**Symptom**: Tests fail with CAPTCHA validation errors

**Solutions**:
- Disable CAPTCHA in test configuration
- Use test API keys from CAPTCHA provider (if available)
- Mock CAPTCHA in unit tests only
- Configure test environment to skip CAPTCHA validation

### Tests Run Out of Memory

**Symptom**: "JavaScript heap out of memory" error

**Solutions**:
```bash
# Run tests serially instead of in parallel
npm test -- --maxWorkers=1

# Or increase Node heap size
NODE_OPTIONS=--max-old-space-size=4096 npm test

# Or split tests into separate runs
npm test -- tests/unit/api/skycms-contact.test.js
npm test -- tests/unit/api/generated-script.test.js
```

## CI/CD Integration

### GitHub Actions Example

```yaml
name: JavaScript Tests

on: [push, pull_request]

jobs:
  unit-tests:
    runs-on: ubuntu-latest
    
    steps:
      - uses: actions/checkout@v3
      
      - uses: actions/setup-node@v3
        with:
          node-version: '18'
      
      - name: Install dependencies
        run: cd JestTests && npm ci
      
      - name: Run unit tests
        run: cd JestTests && npm test
      
      - name: Upload coverage
        uses: codecov/codecov-action@v3
        with:
          files: ./JestTests/coverage/lcov.info

  integration-tests:
    runs-on: ubuntu-latest
    needs: unit-tests
    
    steps:
      - uses: actions/checkout@v3
      
      - uses: actions/setup-node@v3
        with:
          node-version: '18'
      
      - uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '9.0.x'
      
      - name: Install dependencies
        run: cd JestTests && npm ci
      
      - name: Start API
        run: |
          cd Sky.Cms.Api.Shared
          dotnet run &
          sleep 10
        timeout-minutes: 5
      
      - name: Run integration tests
        run: cd JestTests && npm run test:integration
```

## Coverage Goals

### Unit Test Coverage

Target **>80% code coverage** for JavaScript files:

```bash
npm test -- --coverage

# View coverage report
open coverage/lcov-report/index.html
```

### Integration Test Coverage

Cannot measure code coverage for integration tests (they call compiled C# code), but verify:

- All HTTP endpoints are called
- All validation rules are tested
- All response codes are handled
- All error cases are covered

## Best Practices Summary

### General

✅ Test behavior, not implementation  
✅ Keep test names descriptive  
✅ Use beforeEach/afterEach for setup/cleanup  
✅ Make tests independent (no execution order dependency)  
✅ Review and update tests with code changes  

### Unit Tests

✅ Mock all network calls  
✅ Run frequently during development  
✅ Keep tests fast (milliseconds, not seconds)  
✅ Test error paths and edge cases  
✅ Use 100% deterministic data  

### Integration Tests

✅ Test actual API contract  
✅ Handle API unavailability gracefully  
✅ Use realistic (but safe) test data  
✅ Test error responses from API  
✅ Document any prerequisites  

## References

- [Jest Documentation](https://jestjs.io/)
- [Jest API Reference](https://jestjs.io/docs/api)
- [Testing Library](https://testing-library.com/docs/)
- [Playwright (Future E2E)](https://playwright.dev/)
- [Contact API Documentation](../Docs/Api/ContactForm.md)
- [API Architecture](../Docs/Api/ARCHITECTURE.md)
- [Configuration Guide](../Docs/Api/Configuration.md)

## Support

For questions or issues with testing:

1. Check Jest logs: `npm test -- --verbose`
2. Review test file examples in `tests/unit/api/` and `tests/integration/api/`
3. Consult Jest documentation for specific features
4. Check API logs in `Sky.Cms.Api.Shared` for integration test issues
