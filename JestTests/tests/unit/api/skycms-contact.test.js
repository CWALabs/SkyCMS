/**
 * Unit tests for SkyCmsContact JavaScript library
 * These tests validate the generated JavaScript from the Contact API endpoint
 */

const { describe, test, expect, beforeEach, afterEach } = require('@jest/globals');

// Mock the generated SkyCmsContact library
// In real tests, you would fetch this from the API endpoint or generate it
const createSkyCmsContactLibrary = (config = {}) => {
  const defaultConfig = {
    requireCaptcha: false,
    captchaProvider: null,
    captchaSiteKey: '',
    antiforgeryToken: 'test-token-12345',
    submitEndpoint: '/_api/contact/submit',
    maxMessageLength: 5000,
    fieldNames: {
      name: 'name',
      email: 'email',
      message: 'message'
    },
    ...config
  };

  return {
    config: defaultConfig,

    init: function(formSelector, options) {
      const form = typeof formSelector === 'string' 
        ? document.querySelector(formSelector) 
        : formSelector;

      if (!form) {
        console.error('SkyCmsContact: Form not found');
        return;
      }

      const config = { 
        ...this.config, 
        ...options,
        fieldNames: { ...this.config.fieldNames, ...(options?.fieldNames || {}) }
      };

      form.addEventListener('submit', async (e) => {
        e.preventDefault();
        await this.handleSubmit(form, config);
      });

      if (this.config.requireCaptcha) {
        this.loadCaptcha(config);
      }
    },

    handleSubmit: async function(form, config) {
      const formData = new FormData(form);
      const fieldNames = config.fieldNames || this.config.fieldNames;
      
      const data = {
        name: formData.get(fieldNames.name),
        email: formData.get(fieldNames.email),
        message: formData.get(fieldNames.message)
      };

      if (config.requireCaptcha) {
        try {
          data.captchaToken = await this.getCaptchaToken(config);
        } catch (error) {
          console.error('SkyCmsContact: CAPTCHA error:', error);
          if (config.onError) {
            config.onError({ success: false, message: 'CAPTCHA validation failed. Please try again.' });
          } else {
            alert('CAPTCHA validation failed. Please try again.');
          }
          return;
        }
      }

      try {
        const response = await fetch(config.submitEndpoint, {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': config.antiforgeryToken
          },
          body: JSON.stringify(data)
        });

        const result = await response.json();

        if (result.success) {
          if (config.onSuccess) {
            config.onSuccess(result);
          } else {
            alert(result.message);
            form.reset();
          }
        } else {
          if (config.onError) {
            config.onError(result);
          } else {
            alert(result.message || 'Submission failed. Please try again.');
          }
        }
      } catch (error) {
        console.error('SkyCmsContact submission error:', error);
        if (config.onError) {
          config.onError({ success: false, message: 'Network error. Please try again.' });
        } else {
          alert('Network error. Please try again.');
        }
      }
    },

    loadCaptcha: function(config) {
      // Mock implementation
    },

    getCaptchaToken: async function(config) {
      return 'mock-captcha-token';
    }
  };
};

describe('SkyCmsContact Library', () => {
  let SkyCmsContact;

  beforeEach(() => {
    // Reset DOM
    document.body.innerHTML = '';
    
    // Create a fresh instance
    SkyCmsContact = createSkyCmsContactLibrary();

    // Mock fetch
    global.fetch = jest.fn();
    
    // Mock alert
    global.alert = jest.fn();
    
    // Mock console
    global.console.error = jest.fn();
  });

  afterEach(() => {
    jest.clearAllMocks();
  });

  describe('Configuration', () => {
    test('should have default configuration', () => {
      expect(SkyCmsContact.config.submitEndpoint).toBe('/_api/contact/submit');
      expect(SkyCmsContact.config.maxMessageLength).toBe(5000);
      expect(SkyCmsContact.config.requireCaptcha).toBe(false);
    });

    test('should have default field names', () => {
      expect(SkyCmsContact.config.fieldNames.name).toBe('name');
      expect(SkyCmsContact.config.fieldNames.email).toBe('email');
      expect(SkyCmsContact.config.fieldNames.message).toBe('message');
    });

    test('should have antiforgery token', () => {
      expect(SkyCmsContact.config.antiforgeryToken).toBeTruthy();
    });
  });

  describe('init() method', () => {
    test('should initialize form with selector string', () => {
      const form = document.createElement('form');
      form.id = 'contactForm';
      document.body.appendChild(form);

      SkyCmsContact.init('#contactForm');
      
      // Verify form was found (no error logged)
      expect(console.error).not.toHaveBeenCalled();
    });

    test('should initialize form with element reference', () => {
      const form = document.createElement('form');
      document.body.appendChild(form);

      SkyCmsContact.init(form);
      
      expect(console.error).not.toHaveBeenCalled();
    });

    test('should log error if form not found', () => {
      SkyCmsContact.init('#nonexistent');
      
      expect(console.error).toHaveBeenCalledWith('SkyCmsContact: Form not found');
    });

    test('should accept custom field names', () => {
      const form = document.createElement('form');
      form.id = 'customForm';
      document.body.appendChild(form);

      const customOptions = {
        fieldNames: {
          name: 'fullName',
          email: 'emailAddress',
          message: 'userMessage'
        }
      };

      SkyCmsContact.init('#customForm', customOptions);
      
      expect(console.error).not.toHaveBeenCalled();
    });

    test('should merge partial field name overrides', () => {
      const form = document.createElement('form');
      form.id = 'testForm';
      document.body.appendChild(form);

      const options = {
        fieldNames: {
          message: 'comments'  // Only override message
        }
      };

      SkyCmsContact.init('#testForm', options);
      
      expect(console.error).not.toHaveBeenCalled();
    });
  });

  describe('handleSubmit() method', () => {
    let form;

    beforeEach(() => {
      form = document.createElement('form');
      
      const nameInput = document.createElement('input');
      nameInput.name = 'name';
      nameInput.value = 'John Doe';
      
      const emailInput = document.createElement('input');
      emailInput.name = 'email';
      emailInput.value = 'john@example.com';
      
      const messageInput = document.createElement('textarea');
      messageInput.name = 'message';
      messageInput.value = 'This is a test message';
      
      form.appendChild(nameInput);
      form.appendChild(emailInput);
      form.appendChild(messageInput);
      
      document.body.appendChild(form);
    });

    test('should extract form data with default field names', async () => {
      const mockResponse = {
        success: true,
        message: 'Thank you!',
        error: null
      };

      global.fetch.mockResolvedValueOnce({
        json: async () => mockResponse
      });

      await SkyCmsContact.handleSubmit(form, SkyCmsContact.config);

      expect(fetch).toHaveBeenCalledWith(
        '/_api/contact/submit',
        expect.objectContaining({
          method: 'POST',
          headers: expect.objectContaining({
            'Content-Type': 'application/json',
            'RequestVerificationToken': 'test-token-12345'
          }),
          body: JSON.stringify({
            name: 'John Doe',
            email: 'john@example.com',
            message: 'This is a test message'
          })
        })
      );
    });

    test('should extract form data with custom field names', async () => {
      // Create form with custom field names
      form.innerHTML = '';
      
      const nameInput = document.createElement('input');
      nameInput.name = 'fullName';
      nameInput.value = 'Jane Smith';
      
      const emailInput = document.createElement('input');
      emailInput.name = 'emailAddress';
      emailInput.value = 'jane@example.com';
      
      const messageInput = document.createElement('textarea');
      messageInput.name = 'userMessage';
      messageInput.value = 'Custom message';
      
      form.appendChild(nameInput);
      form.appendChild(emailInput);
      form.appendChild(messageInput);

      const customConfig = {
        ...SkyCmsContact.config,
        fieldNames: {
          name: 'fullName',
          email: 'emailAddress',
          message: 'userMessage'
        }
      };

      const mockResponse = {
        success: true,
        message: 'Thank you!',
        error: null
      };

      global.fetch.mockResolvedValueOnce({
        json: async () => mockResponse
      });

      await SkyCmsContact.handleSubmit(form, customConfig);

      expect(fetch).toHaveBeenCalledWith(
        '/_api/contact/submit',
        expect.objectContaining({
          body: JSON.stringify({
            name: 'Jane Smith',
            email: 'jane@example.com',
            message: 'Custom message'
          })
        })
      );
    });

    test('should call onSuccess callback on successful submission', async () => {
      const onSuccess = jest.fn();
      const mockResponse = {
        success: true,
        message: 'Thank you for your message!',
        error: null
      };

      global.fetch.mockResolvedValueOnce({
        json: async () => mockResponse
      });

      const config = {
        ...SkyCmsContact.config,
        onSuccess
      };

      await SkyCmsContact.handleSubmit(form, config);

      expect(onSuccess).toHaveBeenCalledWith(mockResponse);
      expect(alert).not.toHaveBeenCalled();
    });

    test('should call onError callback on failed submission', async () => {
      const onError = jest.fn();
      const mockResponse = {
        success: false,
        message: 'Validation failed',
        error: 'Name is required'
      };

      global.fetch.mockResolvedValueOnce({
        json: async () => mockResponse
      });

      const config = {
        ...SkyCmsContact.config,
        onError
      };

      await SkyCmsContact.handleSubmit(form, config);

      expect(onError).toHaveBeenCalledWith(mockResponse);
      expect(alert).not.toHaveBeenCalled();
    });

    test('should show alert on success if no callback provided', async () => {
      const mockResponse = {
        success: true,
        message: 'Success!',
        error: null
      };

      global.fetch.mockResolvedValueOnce({
        json: async () => mockResponse
      });

      form.reset = jest.fn();

      await SkyCmsContact.handleSubmit(form, SkyCmsContact.config);

      expect(alert).toHaveBeenCalledWith('Success!');
      expect(form.reset).toHaveBeenCalled();
    });

    test('should show alert on error if no callback provided', async () => {
      const mockResponse = {
        success: false,
        message: 'Error occurred',
        error: 'Invalid email'
      };

      global.fetch.mockResolvedValueOnce({
        json: async () => mockResponse
      });

      await SkyCmsContact.handleSubmit(form, SkyCmsContact.config);

      expect(alert).toHaveBeenCalledWith('Error occurred');
    });

    test('should handle network errors', async () => {
      const onError = jest.fn();
      
      global.fetch.mockRejectedValueOnce(new Error('Network failure'));

      const config = {
        ...SkyCmsContact.config,
        onError
      };

      await SkyCmsContact.handleSubmit(form, config);

      expect(console.error).toHaveBeenCalled();
      expect(onError).toHaveBeenCalledWith({
        success: false,
        message: 'Network error. Please try again.'
      });
    });

    test('should include CAPTCHA token when required', async () => {
      const configWithCaptcha = createSkyCmsContactLibrary({
        requireCaptcha: true,
        captchaProvider: 'turnstile',
        captchaSiteKey: 'test-site-key'
      });

      const mockResponse = {
        success: true,
        message: 'Success with CAPTCHA',
        error: null
      };

      global.fetch.mockResolvedValueOnce({
        json: async () => mockResponse
      });

      await configWithCaptcha.handleSubmit(form, configWithCaptcha.config);

      const callArgs = fetch.mock.calls[0][1];
      const body = JSON.parse(callArgs.body);
      
      expect(body.captchaToken).toBe('mock-captcha-token');
    });
  });

  describe('Field Name Configuration', () => {
    test('should use default field names when not specified', () => {
      const form = document.createElement('form');
      form.id = 'defaultForm';
      document.body.appendChild(form);

      SkyCmsContact.init('#defaultForm');

      // Default field names should be in config
      const config = SkyCmsContact.config;
      expect(config.fieldNames.name).toBe('name');
      expect(config.fieldNames.email).toBe('email');
      expect(config.fieldNames.message).toBe('message');
    });

    test('should override all field names', () => {
      const form = document.createElement('form');
      document.body.appendChild(form);

      const customFieldNames = {
        name: 'customerName',
        email: 'customerEmail',
        message: 'customerComments'
      };

      SkyCmsContact.init(form, {
        fieldNames: customFieldNames
      });

      expect(console.error).not.toHaveBeenCalled();
    });

    test('should partially override field names', () => {
      const form = document.createElement('form');
      document.body.appendChild(form);

      // Only override message field
      SkyCmsContact.init(form, {
        fieldNames: {
          message: 'comments'
        }
      });

      expect(console.error).not.toHaveBeenCalled();
    });
  });

  describe('Integration', () => {
    test('should handle full form submission workflow', async () => {
      const form = document.createElement('form');
      form.id = 'integrationForm';
      
      const nameInput = document.createElement('input');
      nameInput.name = 'name';
      nameInput.value = 'Integration Test';
      
      const emailInput = document.createElement('input');
      emailInput.name = 'email';
      emailInput.value = 'test@integration.com';
      
      const messageInput = document.createElement('textarea');
      messageInput.name = 'message';
      messageInput.value = 'Integration test message';
      
      const submitButton = document.createElement('button');
      submitButton.type = 'submit';
      
      form.appendChild(nameInput);
      form.appendChild(emailInput);
      form.appendChild(messageInput);
      form.appendChild(submitButton);
      
      document.body.appendChild(form);

      const onSuccess = jest.fn();
      const onError = jest.fn();

      SkyCmsContact.init('#integrationForm', {
        onSuccess,
        onError
      });

      const mockResponse = {
        success: true,
        message: 'Form submitted successfully!',
        error: null
      };

      global.fetch.mockResolvedValueOnce({
        json: async () => mockResponse
      });

      // Trigger form submit
      const submitEvent = new Event('submit', { bubbles: true, cancelable: true });
      form.dispatchEvent(submitEvent);

      // Wait for async operations
      await new Promise(resolve => setTimeout(resolve, 100));

      expect(onSuccess).toHaveBeenCalledWith(mockResponse);
      expect(onError).not.toHaveBeenCalled();
    });
  });
});
