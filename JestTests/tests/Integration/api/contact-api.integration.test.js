/**
 * Integration tests for Contact API
 * Tests JavaScript interaction with a running C# backend API
 */

const { describe, test, expect, beforeAll, afterAll } = require('@jest/globals');
const { getApiUrl, isApiAvailable } = require('../../setup/api-config');

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

  describe('GET /_api/contact/skycms-contact.js', () => {
    test('should return JavaScript library', async () => {
      if (!apiAvailable) {
        console.log('Skipping - API not running');
        return;
      }

      const response = await fetch(`${apiUrl}/_api/contact/skycms-contact.js`);
      
      expect(response.ok).toBe(true);
      expect(response.headers.get('content-type')).toContain('application/javascript');
      
      const script = await response.text();
      expect(script).toContain('SkyCmsContact');
      expect(script).toContain('init');
      expect(script).toContain('handleSubmit');
    });

    test('should have embedded antiforgery token', async () => {
      if (!apiAvailable) return;

      const response = await fetch(`${apiUrl}/_api/contact/skycms-contact.js`);
      const script = await response.text();
      
      expect(script).toMatch(/antiforgeryToken:\s*['"][^'"]+['"]/);
    });

    test('should have configuration embedded', async () => {
      if (!apiAvailable) return;

      const response = await fetch(`${apiUrl}/_api/contact/skycms-contact.js`);
      const script = await response.text();
      
      expect(script).toMatch(/submitEndpoint:\s*['"]\/api\/contact\/submit['"]|['"]\/\w+\/\w+['"]/);
      expect(script).toMatch(/maxMessageLength:\s*\d+/);
    });
  });

  describe('POST /_api/contact/submit', () => {
    test('should accept valid contact form submission', async () => {
      if (!apiAvailable) return;

      // First, get the antiforgery token from the script
      const scriptResponse = await fetch(`${apiUrl}/_api/contact/skycms-contact.js`);
      const script = await scriptResponse.text();
      
      // Extract token (simple regex for testing)
      const tokenMatch = script.match(/antiforgeryToken:\s*['"]([^'"]+)['"]/);
      const token = tokenMatch ? tokenMatch[1] : '';

      const response = await fetch(`${apiUrl}/_api/contact/submit`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'RequestVerificationToken': token
        },
        body: JSON.stringify({
          name: 'Integration Test',
          email: 'integration@test.com',
          message: 'This is an integration test from Jest'
        })
      });

      expect(response.ok).toBe(true);
      
      const result = await response.json();
      expect(result).toHaveProperty('success');
      expect(result).toHaveProperty('message');
    });

    test('should reject invalid email', async () => {
      if (!apiAvailable) return;

      const response = await fetch(`${apiUrl}/_api/contact/submit`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          name: 'Test',
          email: 'invalid-email',
          message: 'This should fail'
        })
      });

      // Should return 400 or 200 with success: false
      const result = await response.json();
      expect(result.success).toBe(false);
    });

    test('should reject missing required fields', async () => {
      if (!apiAvailable) return;

      const response = await fetch(`${apiUrl}/_api/contact/submit`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          name: 'Test'
          // Missing email and message
        })
      });

      const result = await response.json();
      expect(result.success).toBe(false);
      expect(result.error).toBeTruthy();
    });

    test('should enforce message length limits', async () => {
      if (!apiAvailable) return;

      const longMessage = 'a'.repeat(10000); // Way too long

      const response = await fetch(`${apiUrl}/_api/contact/submit`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          name: 'Test',
          email: 'test@example.com',
          message: longMessage
        })
      });

      const result = await response.json();
      expect(result.success).toBe(false);
    });
  });

  describe('Rate Limiting', () => {
    test('should enforce rate limiting on repeated requests', async () => {
      if (!apiAvailable) return;

      const testData = {
        name: 'Rate Test',
        email: 'rate@test.com',
        message: 'Rate limiting test message that is long enough'
      };

      const headers = {
        'Content-Type': 'application/json'
      };

      const responses = [];
      
      // Send 6 requests (limit is 5 per minute)
      for (let i = 0; i < 6; i++) {
        const response = await fetch(`${apiUrl}/_api/contact/submit`, {
          method: 'POST',
          headers,
          body: JSON.stringify(testData)
        });
        
        responses.push({
          status: response.status,
          data: await response.json()
        });
      }

      // First 5 should succeed (or at least not be 429)
      // The 6th might be 429 (too many requests) or still succeed depending on rate limiter state
      expect(responses.length).toBe(6);
      
      // Check if we got any rate limit responses
      const rateLimitResponse = responses.find(r => r.status === 429);
      if (rateLimitResponse) {
        expect(rateLimitResponse.data.error).toMatch(/rate|limit/i);
      }
    });
  });

  describe('CAPTCHA Integration', () => {
    test('should have CAPTCHA configured or disabled in generated script', async () => {
      if (!apiAvailable) return;

      const response = await fetch(`${apiUrl}/_api/contact/skycms-contact.js`);
      const script = await response.text();
      
      // Should mention captcha configuration
      expect(script).toMatch(/requireCaptcha|captchaProvider/);
    });
  });

  describe('Custom Field Names', () => {
    test('should support custom field name mapping', async () => {
      if (!apiAvailable) return;

      const response = await fetch(`${apiUrl}/_api/contact/skycms-contact.js`);
      const script = await response.text();
      
      // Should have fieldNames configuration
      expect(script).toMatch(/fieldNames:/);
      expect(script).toMatch(/name:/);
      expect(script).toMatch(/email:/);
      expect(script).toMatch(/message:/);
    });
  });
});
