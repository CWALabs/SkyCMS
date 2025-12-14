{% include nav.html %}

# Cosmos CMS Forms Helpers

Small helpers for posting forms with the Cosmos antiforgery token and basic element checks.

- Location (source): `Editor/wwwroot/lib/cosmos/forms.js`
- Exposed functions:
  - `ccms___ElementExists(element: any): boolean`
  - `ccms___GetXsrfToken(): Promise<string>` — fetches an antiforgery token from `/ccms__antiforgery/token` response header `XSRF-TOKEN`
  - `ccms___PostForm(endpoint: string, formName: string): Promise<Response>` — posts the named form with header `RequestVerificationToken`

## Requirements

- Server endpoint `/ccms__antiforgery/token` must return a response with an `XSRF-TOKEN` header.
- Target form must be accessible via `document.forms[formName]`.

## Simple usage

```html
<form name="contactForm" id="contactForm">
  <input name="email" type="email" required />
  <textarea name="message" required></textarea>
  <button id="btnSubmit" type="submit">Send</button>
</form>

<script src="/lib/cosmos/forms.js"></script>
<script>
  document.getElementById('contactForm').addEventListener('submit', async (e) => {
    e.preventDefault();
    try {
      const resp = await ccms___PostForm('/Contact/Send', 'contactForm');
      if (!resp.ok) throw new Error('Request failed: ' + resp.status);
      alert('Thank you!');
    } catch (err) {
      console.error(err);
      alert('Sorry, please try again.');
    }
  });
</script>
```

## Notes

- `ccms___PostForm` builds a `FormData(form)` and sends it with header `RequestVerificationToken` using the value retrieved from `ccms___GetXsrfToken()`.
- Handle errors by checking `response.ok` and reading body as needed.
