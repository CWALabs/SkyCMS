# Contact Form API Integration Guide

This guide shows developers how to integrate the Contact Form API into their SkyCMS Editor web app and website pages.

## Overview

The Contact Form API is built into the Editor application. When you install and configure the Editor, the API is automatically available at:

```
/_api/contact/skycms-contact.js  (GET)   - JavaScript library
/_api/contact/submit              (POST)  - Form submission endpoint
```

No additional setup is needed beyond normal Editor configuration.

## Quick Start

### 1. Basic HTML Form

The simplest way to add a contact form to your website is with an HTML form and the provided JavaScript library.

```html
<!DOCTYPE html>
<html>
<head>
  <meta charset="UTF-8">
  <title>Contact Us</title>
  <style>
    .form-group {
      margin-bottom: 1rem;
    }
    label {
      display: block;
      margin-bottom: 0.5rem;
      font-weight: bold;
    }
    input, textarea {
      width: 100%;
      padding: 0.5rem;
      font-family: inherit;
    }
    button {
      background-color: #007bff;
      color: white;
      padding: 0.75rem 1.5rem;
      border: none;
      cursor: pointer;
    }
    .alert {
      padding: 1rem;
      margin-bottom: 1rem;
      border-radius: 4px;
    }
    .alert-success {
      background-color: #d4edda;
      color: #155724;
      border: 1px solid #c3e6cb;
    }
    .alert-danger {
      background-color: #f8d7da;
      color: #721c24;
      border: 1px solid #f5c6cb;
    }
  </style>
</head>
<body>
  <h1>Contact Us</h1>
  
  <!-- Success/error messages appear here -->
  <div id="messageContainer"></div>
  
  <!-- Contact form with default field names -->
  <form id="contactForm">
    <div class="form-group">
      <label for="name">Your Name</label>
      <input type="text" id="name" name="name" required minlength="2" maxlength="100">
    </div>
    
    <div class="form-group">
      <label for="email">Email Address</label>
      <input type="email" id="email" name="email" required maxlength="255">
    </div>
    
    <div class="form-group">
      <label for="message">Message</label>
      <textarea id="message" name="message" required minlength="10" maxlength="5000" rows="5"></textarea>
    </div>
    
    <button type="submit">Send Message</button>
  </form>

  <!-- Load the SkyCMS Contact Form library -->
  <script src="/_api/contact/skycms-contact.js"></script>
  
  <!-- Initialize the form with callbacks -->
  <script>
    SkyCmsContact.init('#contactForm', {
      onSuccess: (result) => {
        // Show success message
        const messageDiv = document.getElementById('messageContainer');
        messageDiv.innerHTML = `
          <div class="alert alert-success">
            <strong>Success!</strong> ${result.message}
          </div>
        `;
        
        // Clear the form
        document.getElementById('contactForm').reset();
        
        // Hide message after 5 seconds
        setTimeout(() => {
          messageDiv.innerHTML = '';
        }, 5000);
      },
      
      onError: (result) => {
        // Show error message
        const messageDiv = document.getElementById('messageContainer');
        messageDiv.innerHTML = `
          <div class="alert alert-danger">
            <strong>Error:</strong> ${result.message}
          </div>
        `;
        
        // Hide message after 10 seconds
        setTimeout(() => {
          messageDiv.innerHTML = '';
        }, 10000);
      }
    });
  </script>
</body>
</html>
```

## How It Works

### 1. Load the JavaScript Library

The first step is to load the SkyCMS Contact Form library:

```html
<script src="/_api/contact/skycms-contact.js"></script>
```

This single request:
- Generates an anti-forgery token for CSRF protection
- Loads your CAPTCHA configuration (if enabled)
- Creates the global `SkyCmsContact` object
- Automatically loads CAPTCHA scripts if needed

### 2. Define Your Form HTML

Create an HTML form with input fields. By default, the library expects fields named:
- `name` - The visitor's name
- `email` - The visitor's email
- `message` - The visitor's message

```html
<form id="contactForm">
  <input type="text" name="name" required>
  <input type="email" name="email" required>
  <textarea name="message" required></textarea>
  <button type="submit">Send</button>
</form>
```

### 3. Initialize the Form

Call `SkyCmsContact.init()` with your form selector and optional callbacks:

```javascript
SkyCmsContact.init('#contactForm', {
  onSuccess: (result) => {
    console.log('Form sent successfully:', result.message);
  },
  onError: (result) => {
    console.error('Form submission failed:', result.message);
  }
});
```

## Custom Field Names

If your form uses different field names, specify them in the `fieldNames` option:

```html
<form id="myForm">
  <input type="text" name="fullName" required>
  <input type="email" name="contactEmail" required>
  <textarea name="comments" required></textarea>
  <button type="submit">Send</button>
</form>

<script src="/_api/contact/skycms-contact.js"></script>
<script>
  SkyCmsContact.init('#myForm', {
    fieldNames: {
      name: 'fullName',
      email: 'contactEmail',
      message: 'comments'
    },
    onSuccess: (result) => {
      alert(result.message);
      document.getElementById('myForm').reset();
    }
  });
</script>
```

## Using with Popular Frameworks

### React Example

```jsx
import React, { useEffect, useState } from 'react';

export function ContactForm() {
  const [message, setMessage] = useState('');
  const [isError, setIsError] = useState(false);

  useEffect(() => {
    // Load the script
    const script = document.createElement('script');
    script.src = '/_api/contact/skycms-contact.js';
    document.head.appendChild(script);

    script.onload = () => {
      // Initialize after script loads
      window.SkyCmsContact.init('#contactForm', {
        onSuccess: (result) => {
          setMessage(result.message);
          setIsError(false);
          setTimeout(() => setMessage(''), 5000);
        },
        onError: (result) => {
          setMessage(result.message);
          setIsError(true);
          setTimeout(() => setMessage(''), 10000);
        }
      });
    };
  }, []);

  return (
    <div>
      <h1>Contact Us</h1>
      
      {message && (
        <div className={isError ? 'alert-error' : 'alert-success'}>
          {message}
        </div>
      )}
      
      <form id="contactForm">
        <div>
          <label>Name:</label>
          <input type="text" name="name" required />
        </div>
        
        <div>
          <label>Email:</label>
          <input type="email" name="email" required />
        </div>
        
        <div>
          <label>Message:</label>
          <textarea name="message" required></textarea>
        </div>
        
        <button type="submit">Send</button>
      </form>
    </div>
  );
}
```

### Vue 3 Example

```vue
<template>
  <div>
    <h1>Contact Us</h1>
    
    <div v-if="message" :class="['alert', isError ? 'alert-error' : 'alert-success']">
      {{ message }}
    </div>
    
    <form id="contactForm">
      <div>
        <label>Name:</label>
        <input type="text" name="name" required />
      </div>
      
      <div>
        <label>Email:</label>
        <input type="email" name="email" required />
      </div>
      
      <div>
        <label>Message:</label>
        <textarea name="message" required></textarea>
      </div>
      
      <button type="submit">Send</button>
    </form>
  </div>
</template>

<script setup>
import { onMounted, ref } from 'vue';

const message = ref('');
const isError = ref(false);

onMounted(() => {
  // Load the SkyCMS script
  const script = document.createElement('script');
  script.src = '/_api/contact/skycms-contact.js';
  document.head.appendChild(script);

  script.onload = () => {
    window.SkyCmsContact.init('#contactForm', {
      onSuccess: (result) => {
        message.value = result.message;
        isError.value = false;
        setTimeout(() => (message.value = ''), 5000);
      },
      onError: (result) => {
        message.value = result.message;
        isError.value = true;
        setTimeout(() => (message.value = ''), 10000);
      }
    });
  };
});
</script>
```

## Configuration

The Contact Form API is configured in the Editor's Settings table in the database.

### Database Settings

The API reads configuration from the `Settings` table:

| Group | Name | Value | Example |
|-------|------|-------|---------|
| ContactApi | AdminEmail | Email address to receive submissions | `admin@example.com` |
| ContactApi | MaxMessageLength | Maximum message length in characters | `5000` |
| CAPTCHA | Config | JSON configuration for CAPTCHA | See below |

### CAPTCHA Configuration

If you want to enable CAPTCHA protection, add a setting with the CAPTCHA configuration as JSON:

```json
{
  "Provider": "turnstile",
  "SiteKey": "your-turnstile-site-key",
  "SecretKey": "your-turnstile-secret-key",
  "RequireCaptcha": true
}
```

Or for reCAPTCHA v3:

```json
{
  "Provider": "recaptcha",
  "SiteKey": "your-recaptcha-site-key",
  "SecretKey": "your-recaptcha-secret-key",
  "RequireCaptcha": true
}
```

### Email Configuration

The API automatically uses the Editor's email configuration (SendGrid, Azure Communication Services, or SMTP). If no `ContactApi.AdminEmail` is configured, it falls back to the email service's `SenderEmail`.

This means:
- **Multi-tenant**: Each tenant can have its own admin email or use the email provider's default
- **Flexible**: You can configure per-contact-form or rely on default email settings
- **Reliable**: Falls back gracefully if ContactApi settings are missing

## Rate Limiting

The Contact Form API has built-in rate limiting to prevent spam:

- **Limit**: 3 submissions per 5 minutes per IP address
- **Status Code**: 429 (Too Many Requests)

This is configured in the Editor's `Program.cs`:

```csharp
options.AddFixedWindowLimiter("contact-form", opt =>
{
    opt.Window = TimeSpan.FromMinutes(5);
    opt.PermitLimit = 3;  // Max 3 contact form submissions per 5 minutes per IP
    opt.QueueLimit = 0;   // No queuing
});
```

To adjust the rate limit, modify the Editor's configuration and restart the application.

## Response Messages

The API returns different messages based on the submission result:

### Success (200 OK)
```json
{
  "success": true,
  "message": "Thank you for your message. We'll get back to you soon!",
  "error": null
}
```

### Validation Error (400 Bad Request)
```json
{
  "success": false,
  "message": "Validation failed",
  "error": "Message must be between 10 and 5000 characters"
}
```

### CAPTCHA Failed (400 Bad Request)
```json
{
  "success": false,
  "message": "We're sorry, but there was a problem with your submission.",
  "error": "CAPTCHA validation failed"
}
```

### Rate Limited (429 Too Many Requests)
```json
{
  "success": false,
  "message": "Too many requests. Please try again later.",
  "error": "Rate limit exceeded"
}
```

## Troubleshooting

### Form submissions aren't being received

1. **Check AdminEmail configuration**:
   - Verify the `ContactApi.AdminEmail` setting exists in the database
   - If not configured, the API falls back to the email service's sender email
   
2. **Check email service**:
   - Verify email configuration is complete (SendGrid, Azure, or SMTP)
   - Check application logs for email sending errors
   
3. **Check email filters**:
   - Verify the admin email account isn't filtering the emails as spam
   - Check spam/junk folders

4. **Check application logs**:
   - Look for errors in the Editor application logs
   - Check database connectivity and email service connectivity

### CAPTCHA not showing or failing

1. **Verify CAPTCHA configuration**:
   - Check the CAPTCHA setting in the database
   - Ensure `RequireCaptcha` is set to `true`
   - Verify the provider (turnstile or recaptcha) is correct

2. **Verify site and secret keys**:
   - Confirm the SiteKey and SecretKey are correct for your provider
   - For Turnstile: https://dash.cloudflare.com/
   - For reCAPTCHA: https://www.google.com/recaptcha/admin

3. **Check browser console**:
   - Open browser developer tools (F12)
   - Look for JavaScript errors related to CAPTCHA loading
   - Check network tab to see if CAPTCHA script is loading

### "Anti-forgery token is missing"

This error occurs if:
- The script endpoint (`/_api/contact/skycms-contact.js`) isn't being called first
- The page is cached without the token
- The form is submitted with a different domain

**Solution**: Ensure you're loading the script from the same domain as your form.

### Rate limiting is too strict

The default is 3 submissions per 5 minutes per IP. To adjust:

1. Edit `Editor/Program.cs`
2. Find the "contact-form" rate limiter configuration
3. Change `opt.PermitLimit = 3` to your desired limit
4. Restart the application

## Security Best Practices

1. **Always use HTTPS** - The anti-forgery tokens and CAPTCHA require secure connections
2. **Enable CAPTCHA in production** - Protects against automated attacks
3. **Monitor submissions** - Check admin email regularly for suspicious patterns
4. **Keep secrets secure** - CAPTCHA secret keys should never be exposed in client code
5. **Validate on server** - The API validates all input server-side

## Performance Considerations

- **Initial load**: ~3-5 KB for the JavaScript library
- **CAPTCHA**: Adds 500ms (Turnstile) or 300ms (reCAPTCHA) on first use
- **Form submission**: < 100ms (excluding network latency and email sending)
- **Email sending**: Asynchronous (doesn't block response)

## What Happens When You Submit

1. **Client-side validation**: HTML5 validation (name, email format, message length)
2. **CAPTCHA validation**: If enabled, the client gets a CAPTCHA token
3. **Server receives**: JSON payload with form data and CAPTCHA token (if any)
4. **Server validates**: Checks all fields and CAPTCHA (if required)
5. **Email sent**: Asynchronously sends email to admin
6. **Response**: Returns success or error to client
7. **Client callback**: Runs `onSuccess` or `onError` callback

## Complete Example with Styling

See the [ContactForm.md](./ContactForm.md) documentation for more complete examples including:
- CAPTCHA setup (Turnstile and reCAPTCHA)
- Advanced JavaScript usage
- cURL and fetch examples
- Error handling
- Field validation

