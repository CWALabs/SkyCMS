# Tutorial: Adding a Contact Form to Your Website

This step-by-step tutorial walks you through adding a working contact form to your website using the SkyCMS Contact Form API.

## Prerequisites

- A running SkyCMS Editor instance
- Basic knowledge of HTML and JavaScript
- A website or web page where you want to add the contact form

## Step 1: Set Up Email Configuration

Before adding a contact form, ensure the Editor has email configured so submissions can be delivered.

### In the Editor Admin Panel:

1. Log in to your SkyCMS Editor
2. Go to Settings → Email Configuration
3. Configure one of the following:
   - **SendGrid**: Provide your SendGrid API key
   - **Azure Communication Services**: Provide your connection string
   - **SMTP**: Provide host, port, username, and password

4. Set the **Admin Email** or **Sender Email** address (where form submissions will be sent)

5. Save the configuration

### Or Set Environment Variables:

If you prefer environment variables, set one of these before starting the Editor:

```bash
# SendGrid
SENDGRIDAPIKEY=your-api-key
SENDEREMAIL=noreply@yourdomain.com

# Azure Communication Services
AZUREEMAILCONNECTIONSTRING=endpoint=https://...

# SMTP
SMTPHOST=smtp.example.com
SMTPPORT=587
SMTPUSERNAME=your-username
SMTPPASSWORD=your-password
SENDEREMAIL=noreply@yourdomain.com
```

## Step 2: Create Your HTML Page

Create a basic HTML page for your contact form:

```html
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Contact Us</title>
    <style>
        body {
            font-family: Arial, sans-serif;
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
        }
        
        h1 {
            color: #333;
            text-align: center;
        }
        
        .form-group {
            margin-bottom: 20px;
        }
        
        label {
            display: block;
            margin-bottom: 8px;
            color: #555;
            font-weight: bold;
        }
        
        input[type="text"],
        input[type="email"],
        textarea {
            width: 100%;
            padding: 12px;
            border: 1px solid #ddd;
            border-radius: 4px;
            font-family: Arial, sans-serif;
            font-size: 14px;
            box-sizing: border-box;
        }
        
        textarea {
            resize: vertical;
            min-height: 150px;
        }
        
        button {
            background-color: #007bff;
            color: white;
            padding: 12px 24px;
            border: none;
            border-radius: 4px;
            font-size: 16px;
            cursor: pointer;
            width: 100%;
        }
        
        button:hover {
            background-color: #0056b3;
        }
        
        button:disabled {
            background-color: #ccc;
            cursor: not-allowed;
        }
        
        .alert {
            padding: 15px;
            border-radius: 4px;
            margin-bottom: 20px;
            display: none;
        }
        
        .alert.show {
            display: block;
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
        
        .loading {
            display: none;
            text-align: center;
            color: #666;
        }
        
        .loading.show {
            display: block;
        }
    </style>
</head>
<body>
    <h1>Get In Touch</h1>
    <p>We'd love to hear from you. Send us a message and we'll respond as soon as possible.</p>
    
    <!-- Status messages -->
    <div id="successMessage" class="alert alert-success"></div>
    <div id="errorMessage" class="alert alert-danger"></div>
    
    <!-- Loading indicator -->
    <div id="loading" class="loading">Sending your message...</div>
    
    <!-- Contact form -->
    <form id="contactForm">
        <div class="form-group">
            <label for="name">Your Name *</label>
            <input 
                type="text" 
                id="name" 
                name="name" 
                required 
                minlength="2" 
                maxlength="100"
                placeholder="John Smith">
        </div>
        
        <div class="form-group">
            <label for="email">Email Address *</label>
            <input 
                type="email" 
                id="email" 
                name="email" 
                required 
                maxlength="255"
                placeholder="john@example.com">
        </div>
        
        <div class="form-group">
            <label for="message">Message *</label>
            <textarea 
                id="message" 
                name="message" 
                required 
                minlength="10" 
                maxlength="5000"
                placeholder="Tell us what's on your mind..."></textarea>
        </div>
        
        <button type="submit" id="submitBtn">Send Message</button>
    </form>

    <!-- Load the SkyCMS Contact Form library -->
    <script src="/_api/contact/skycms-contact.js"></script>
    
    <!-- Initialize the form -->
    <script>
        // Wait for the script to load
        if (typeof SkyCmsContact !== 'undefined') {
            initializeForm();
        } else {
            document.addEventListener('DOMContentLoaded', initializeForm);
        }

        function initializeForm() {
            SkyCmsContact.init('#contactForm', {
                onSuccess: handleSuccess,
                onError: handleError
            });
        }

        function handleSuccess(result) {
            // Hide loading indicator
            document.getElementById('loading').classList.remove('show');
            
            // Show success message
            const successDiv = document.getElementById('successMessage');
            successDiv.textContent = result.message;
            successDiv.classList.add('show');
            
            // Hide error message if shown
            document.getElementById('errorMessage').classList.remove('show');
            
            // Clear the form
            document.getElementById('contactForm').reset();
            
            // Re-enable submit button
            document.getElementById('submitBtn').disabled = false;
            
            // Hide success message after 5 seconds
            setTimeout(() => {
                successDiv.classList.remove('show');
            }, 5000);
        }

        function handleError(result) {
            // Hide loading indicator
            document.getElementById('loading').classList.remove('show');
            
            // Show error message
            const errorDiv = document.getElementById('errorMessage');
            errorDiv.textContent = 'Error: ' + result.message;
            errorDiv.classList.add('show');
            
            // Hide success message if shown
            document.getElementById('successMessage').classList.remove('show');
            
            // Re-enable submit button
            document.getElementById('submitBtn').disabled = false;
            
            // Hide error message after 10 seconds
            setTimeout(() => {
                errorDiv.classList.remove('show');
            }, 10000);
        }

        // Show loading state when form is submitted
        document.getElementById('contactForm').addEventListener('submit', () => {
            document.getElementById('submitBtn').disabled = true;
            document.getElementById('loading').classList.add('show');
        });
    </script>
</body>
</html>
```

Save this as `contact.html` and open it in your browser. The form should now work!

## Step 3: Customize the Form

### Change Field Names

If you want to use different field names, update the HTML and the initialization:

```html
<!-- Your custom field names -->
<form id="contactForm">
    <div class="form-group">
        <label for="fullName">Full Name</label>
        <input type="text" id="fullName" name="fullName" required>
    </div>
    
    <div class="form-group">
        <label for="emailAddress">Email</label>
        <input type="email" id="emailAddress" name="emailAddress" required>
    </div>
    
    <div class="form-group">
        <label for="comments">Comments</label>
        <textarea id="comments" name="comments" required></textarea>
    </div>
    
    <button type="submit">Send</button>
</form>

<script src="/_api/contact/skycms-contact.js"></script>
<script>
    SkyCmsContact.init('#contactForm', {
        fieldNames: {
            name: 'fullName',
            email: 'emailAddress',
            message: 'comments'
        },
        onSuccess: (result) => {
            alert(result.message);
            document.getElementById('contactForm').reset();
        }
    });
</script>
```

### Add Custom Styling

Feel free to modify the CSS to match your website design. The form works with any styling framework:

- **Bootstrap**: Add `class="form-control"` to inputs
- **Tailwind CSS**: Use Tailwind classes for styling
- **Custom CSS**: Create your own stylesheets

The library doesn't care about styling; it only cares about field names.

### Add Validation Feedback

Add real-time validation feedback to fields:

```javascript
document.getElementById('name').addEventListener('blur', function() {
    if (this.value.length < 2) {
        this.classList.add('error');
        this.title = 'Name must be at least 2 characters';
    } else {
        this.classList.remove('error');
        this.title = '';
    }
});

document.getElementById('message').addEventListener('input', function() {
    const count = this.value.length;
    const counter = document.getElementById('charCount');
    if (counter) {
        counter.textContent = `${count}/5000 characters`;
    }
});
```

## Step 4: Add CAPTCHA Protection (Optional)

For production websites, it's recommended to add CAPTCHA protection. The API supports two providers:

### Using Cloudflare Turnstile

1. Go to https://dash.cloudflare.com/ and create a free account
2. Create a Turnstile site to get your **Site Key** and **Secret Key**
3. In the SkyCMS Editor, add database settings:

| Group | Name | Value |
|-------|------|-------|
| CAPTCHA | Config | `{"Provider":"turnstile","SiteKey":"your-site-key","SecretKey":"your-secret-key","RequireCaptcha":true}` |

The JavaScript library will automatically load Turnstile and validate the CAPTCHA.

### Using Google reCAPTCHA v3

1. Go to https://www.google.com/recaptcha/admin and sign in with your Google account
2. Create a new reCAPTCHA v3 site to get your **Site Key** and **Secret Key**
3. In the SkyCMS Editor, add database settings:

| Group | Name | Value |
|-------|------|-------|
| CAPTCHA | Config | `{"Provider":"recaptcha","SiteKey":"your-site-key","SecretKey":"your-secret-key","RequireCaptcha":true}` |

The JavaScript library will automatically load reCAPTCHA and validate.

## Step 5: Test Your Form

### Test with Valid Data

1. Fill out all fields with valid data
2. Click "Send Message"
3. You should see a success message
4. Check your admin email inbox for the submission

### Test with Invalid Data

1. **Too short name**: Try "A" (should fail)
2. **Invalid email**: Try "notanemail" (should fail)
3. **Short message**: Try "Hi" (should fail)
4. Check that validation error messages appear

### Test Rate Limiting

1. Submit the form 3 times rapidly
2. The 4th submission within 5 minutes should be rate limited (429 error)
3. Wait 5 minutes and try again (should work)

## Step 6: Integrate into Your Website

### Static HTML Pages

Just copy the form code into any `.html` file on your website.

### Server-Side Rendering (ASP.NET, PHP, etc.)

Include the form in your server templates:

**ASP.NET Razor**:
```razor
@await Html.PartialAsync("ContactForm")
```

**PHP**:
```php
<?php include 'contact-form.php'; ?>
```

### JavaScript Frameworks (React, Vue, Angular)

Create a component that loads the library:

**React**:
```jsx
useEffect(() => {
    const script = document.createElement('script');
    script.src = '/_api/contact/skycms-contact.js';
    document.body.appendChild(script);
    
    script.onload = () => {
        window.SkyCmsContact.init('#contactForm', {
            onSuccess: handleSuccess,
            onError: handleError
        });
    };
}, []);
```

**Vue**:
```javascript
mounted() {
    const script = document.createElement('script');
    script.src = '/_api/contact/skycms-contact.js';
    document.body.appendChild(script);
    
    script.onload = () => {
        window.SkyCmsContact.init('#contactForm', {
            onSuccess: this.handleSuccess,
            onError: this.handleError
        });
    };
}
```

## Troubleshooting

### Form submits but emails aren't received

**Check email configuration**:
1. Log into the Editor
2. Go to Settings → Email
3. Verify SendGrid, Azure, or SMTP settings are correct
4. Test sending an email from the admin panel

**Check spam folder**:
- Emails might be filtered as spam
- Add the Editor's sender email to your contacts

**Check browser console**:
- Open DevTools (F12)
- Look for JavaScript errors
- The error details will help diagnose the issue

### "Anti-forgery token is missing"

- Make sure you're on the same domain as your Editor
- The script loads the token from `/_api/contact/skycms-contact.js`
- Clear browser cache and try again

### CAPTCHA not showing (if enabled)

- Verify CAPTCHA is configured in the Editor settings
- Check that Site Key is correct
- Ensure you're using HTTPS (CAPTCHA providers require it)

### Rate limit errors

This means the form was submitted more than 3 times in 5 minutes from your IP:
- Wait 5 minutes before trying again
- Check if someone is spam-testing your form

## What Happens Behind the Scenes

When you submit the form:

1. **Client-side validation** (HTML5)
   - Name: 2-100 characters
   - Email: Valid email format
   - Message: 10-5000 characters

2. **Get CAPTCHA token** (if enabled)
   - JavaScript asks the CAPTCHA provider for a token
   - Waits for user interaction or automatic verification

3. **Send to server**
   - POST to `/_api/contact/submit`
   - Includes form data and CAPTCHA token
   - Includes anti-forgery token for CSRF protection

4. **Server-side validation**
   - Validates all field lengths and formats
   - Validates CAPTCHA token (if required)
   - Checks rate limiting

5. **Send email**
   - Creates formatted email with submission details
   - Includes visitor IP address
   - Uses configured email provider (SendGrid, Azure, SMTP)
   - Asynchronously sends (doesn't block response)

6. **Return response**
   - Returns success or error JSON
   - Client runs `onSuccess()` or `onError()` callback

## Best Practices

1. **Use HTTPS** - Always. CAPTCHA and tokens require it.
2. **Enable CAPTCHA** - In production, use Turnstile or reCAPTCHA
3. **Monitor emails** - Check submissions regularly for spam
4. **Customize messages** - Use friendly, helpful success/error messages
5. **Test thoroughly** - Test with valid and invalid data before going live
6. **Style consistently** - Match your website's design system
7. **Be accessible** - Add proper labels and ARIA attributes for screen readers

## Next Steps

- Read the [Contact Form API Reference](./ContactForm.md) for more advanced usage
- Check the [Configuration Guide](./Configuration.md) for all configuration options
- See [Integration Guide](./Integration-Guide.md) for framework-specific examples

