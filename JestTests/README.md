# JestTests

JavaScript unit testing for SkyCMS using Jest.

## Setup

```powershell
# Navigate to JestTests directory
cd JestTests

# Install dependencies (first time only)
npm install
```

## Running Tests

```powershell
# Run all tests
npm test

# Run tests in watch mode (auto-rerun on changes)
npm run test:watch

# Run with coverage report
npm run test:coverage

# Run only API tests
npm run test:api

# Run only Editor tests
npm run test:editor
```

## Test Structure

```
JestTests/
├── package.json           # Jest configuration & dependencies
├── tests/
│   ├── api/              # Contact API JavaScript tests
│   │   ├── skycms-contact.test.js
│   │   └── generated-script.test.js
│   └── editor/           # Editor JavaScript tests
│       └── (future tests)
├── coverage/             # Coverage reports (generated)
└── node_modules/         # Dependencies (after npm install)
```

## What's Being Tested

### API Tests
- **skycms-contact.test.js**: Unit tests for the SkyCmsContact JavaScript library
  - Configuration validation
  - Form initialization
  - Field name mapping (default and custom)
  - Form submission handling
  - Success/error callbacks
  - CAPTCHA integration
  
- **generated-script.test.js**: Integration tests for the actual generated JavaScript from `/_api/contact/skycms-contact.js`

### Future: Editor Tests
Tests for Editor web app JavaScript libraries will go in `tests/editor/`

## Coverage Reports

After running `npm run test:coverage`, view the HTML report:

```powershell
# Open coverage report in browser
start coverage/lcov-report/index.html
```

## CI/CD Integration

Add to your build pipeline:

```yaml
- script: |
    cd JestTests
    npm install
    npm run test:coverage
  displayName: 'Run JavaScript Tests'
```
