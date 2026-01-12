# SkyCMS Contact Form API - Usage Guide

## ?? Quick Start

### 1. Include the JavaScript Library

```html
<script src="/_api/contact/skycms-contact.js"></script>
```

The script is dynamically generated with:
- ? Embedded antiforgery token (auto-renewed on each page load)
- ? CAPTCHA configuration (if enabled)
- ? Tenant-specific settings

### 2. Create Your HTML Form

```html
<form id="contactForm">
    <input type="text" name="fullName" required>
    <input type="email" name="emailAddress" required>
    <textarea name="comments" required></textarea>
    <button type="submit">Send</button>
</form>

<!-- Optional: Message containers -->
<div id="successMessage" style="display:none;"></div>
<div id="errorMessage" style="display:none;"></div>
```

### 3. Initialize the Form

```javascript
SkyCmsContact.init('#contactForm', {
    fieldNames: {
        name: 'fullName',
        email: 'emailAddress',
        message: 'comments'
    },
    onSuccess: (result) => {
        document.getElementById('successMessage').textContent = result.message;
        document.getElementById('successMessage').style.display = 'block';
    },
    onError: (result) => {
        document.getElementById('errorMessage').textContent = result.message;
        document.getElementById('errorMessage').style.display = 'block';
    }
});
```

---

## ?? Configuration Options

### Field Name Mapping

If your form fields have different names than the default (`name`, `email`, `message`), provide a `fieldNames` mapping:

```javascript
SkyCmsContact.init('#contactForm', {
    fieldNames: {
        name: 'customerName',      // Maps to your form's name field
        email: 'customerEmail',    // Maps to your form's email field
        message: 'userMessage'     // Maps to your form's message field
    }
});
```

### Callbacks

#### onSuccess
Called when the form is successfully submitted:

```javascript
onSuccess: (result) => {
    // result = { success: true, message: "Thank you for your message..." }
    console.log(result.message);
}
```

#### onError
Called when submission fails (validation errors, network issues, etc.):

```javascript
onError: (result) => {
    // result = { success: false, message: "Error description" }
    console.error(result.message);
}
```

### Using Element IDs (Alternative to Callbacks)

Instead of callbacks, you can specify element IDs to display messages:

```javascript
SkyCmsContact.init('#contactForm', {
    fieldNames: { /* ... */ },
    successElementId: 'successMessage',
    errorElementId: 'errorMessage'
});
```

---

## ?? Common Mistakes

### ? Initializing Multiple Times

**WRONG:**
```javascript
// This creates duplicate event listeners!
SkyCmsContact.init('#contactForm');
SkyCmsContact.init('#contactForm', { fieldNames: {...} });
SkyCmsContact.init('#contactForm', { onSuccess: ... });
```

**CORRECT:**
```javascript
// Initialize ONCE with all options
SkyCmsContact.init('#contactForm', {
    fieldNames: { name: 'fullName', email: 'emailAddress', message: 'comments' },
    onSuccess: (result) => { /* ... */ },
    onError: (result) => { /* ... */ }
});
```

### ? Accessing Non-Existent DOM Elements

**WRONG:**
```javascript
onError: (result) => {
    // If #errorMessage doesn't exist, this will throw an error
    document.getElementById('errorMessage').textContent = result.message;
}
```

**CORRECT:**
```javascript
onError: (result) => {
    const errorEl = document.getElementById('errorMessage');
    if (errorEl) {
        errorEl.textContent = result.message;
        errorEl.style.display = 'block';
    } else {
        console.error(result.message);
        alert(result.message); // Fallback
    }
}
```

### ? Wrong Field Name Mapping

If your form has `<input name="fullName">` but you map it incorrectly:

**WRONG:**
```javascript
fieldNames: {
    name: 'customerName'  // ? Field doesn't exist!
}
```

**CORRECT:**
```javascript
fieldNames: {
    name: 'fullName'  // ? Matches the actual form field
}
```

---

## ??? CAPTCHA Support

### Enabling CAPTCHA

CAPTCHA is configured server-side in the `Settings` table:

```sql
INSERT INTO Settings (Id, Group, Name, Value, Description)
VALUES (
    NEWID(),
    'CAPTCHA',
    'Config',
    '{"Provider":"turnstile","SiteKey":"your-site-key","SecretKey":"your-secret-key","RequireCaptcha":true}',
    'CAPTCHA configuration'
);
```

### Supported Providers

- **Cloudflare Turnstile** (`"Provider": "turnstile"`)
- **Google reCAPTCHA v3** (`"Provider": "recaptcha"`)

When CAPTCHA is enabled, the JavaScript library automatically:
1. Loads the CAPTCHA provider script
2. Renders the CAPTCHA widget
3. Includes the token in form submissions

---

## ?? Security Features

### Antiforgery Token

Every request includes an antiforgery token to prevent CSRF attacks. The token is:
- ? Automatically embedded in the JavaScript
- ? Sent with every form submission
- ? Validated server-side

### Rate Limiting

Default rate limits (configured in `Program.cs`):

- **Development**: 20 submissions per minute per IP
- **Production**: 3 submissions per 5 minutes per IP

---

## ?? Testing

### Development Mode

In development, rate limits are relaxed to allow rapid testing. To test:

1. Open browser DevTools (F12)
2. Go to Network tab
3. Submit the form
4. Inspect the request:
   - Method: `POST`
   - URL: `/_api/contact/submit`
   - Payload: `{ "name": "...", "email": "...", "message": "..." }`
   - Headers: Contains `RequestVerificationToken`

### Production Mode

Rate limiting is strict. If you exceed the limit, you'll receive:

```json
{
    "success": false,
    "error": "Too many requests. Please try again later."
}
```

Wait 5 minutes or use a different IP address to test again.

---

## ?? Multi-Tenant Support

The Contact API is **tenant-aware**:

- Each tenant can have its own admin email configured in the `Settings` table
- Falls back to the email provider's sender email if not configured
- CAPTCHA settings are tenant-specific

---

## ?? Email Configuration

Contact form submissions are sent to the admin email configured in:

1. **Primary**: `Settings` table ? Group: `ContactApi`, Name: `AdminEmail`
2. **Fallback**: Email provider's `SenderEmail` (from tenant email settings)
3. **Default**: `admin@example.com` (if nothing else is configured)

---

## ?? Troubleshooting

### "Cannot set properties of null (setting 'textContent')"

**Cause**: Your callback tries to access a DOM element that doesn't exist.

**Fix**: Add null checks in your callbacks:

```javascript
onError: (result) => {
    const errorEl = document.getElementById('errorMessage');
    if (errorEl) {
        errorEl.textContent = result.message;
    } else {
        console.error('Error element not found');
        alert(result.message);
    }
}
```

### Form submits but nothing happens

**Cause**: 
1. Form initialized multiple times
2. Field names don't match
3. JavaScript console shows errors

**Fix**:
1. Only call `SkyCmsContact.init()` once
2. Verify field name mapping matches your HTML
3. Check browser console for errors

### "Name is required" / "Email is required" errors

**Cause**: Field name mapping is incorrect.

**Fix**: Ensure `fieldNames` matches your form's `name` attributes:

```javascript
// If your form has:
// <input name="fullName">
// <input name="emailAddress">
// <textarea name="comments">

SkyCmsContact.init('#contactForm', {
    fieldNames: {
        name: 'fullName',        // ?
        email: 'emailAddress',   // ?
        message: 'comments'      // ?
    }
});
```

---

## ?? API Reference

### `SkyCmsContact.init(formSelector, options)`

Initializes the contact form.

**Parameters:**
- `formSelector` (string|HTMLFormElement): CSS selector or form element
- `options` (object): Configuration options

**Options:**
- `fieldNames` (object): Maps API fields to your form fields
  - `name` (string): Name field name (default: `'name'`)
  - `email` (string): Email field name (default: `'email'`)
  - `message` (string): Message field name (default: `'message'`)
- `onSuccess` (function): Success callback
- `onError` (function): Error callback
- `successElementId` (string): ID of success message element
- `errorElementId` (string): ID of error message element

**Example:**
```javascript
SkyCmsContact.init('#contactForm', {
    fieldNames: { name: 'fullName', email: 'email', message: 'message' },
    onSuccess: (result) => console.log(result),
    onError: (result) => console.error(result)
});
```

---

## ?? License

This is part of SkyCMS, licensed under the MIT License.
