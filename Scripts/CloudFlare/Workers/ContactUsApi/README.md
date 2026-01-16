# SkyCMS Contact Us API CloudFlare Worker

A CloudFlare Worker that proxies contact form requests from public websites to the SkyCMS Editor API, with built-in tenant resolution and CORS protection.

## Overview

This worker acts as a secure gateway between public-facing contact forms and the SkyCMS Editor backend contact endpoint. It handles:

- **Request routing** - Forwards `/_api/contact/*` requests to the backend
- **Tenant resolution** - Sets the `x-origin-hostname` header for multi-tenant support
- **Method validation** - Restricts to GET (for form scripts) and POST (for submissions)
- **CORS protection** - Restricts cross-origin access to domains within your zone
- **Error handling** - Logs failures and returns graceful error responses
- **HTTPS enforcement** - Always communicates with backend over HTTPS

## Setup

### Prerequisites

- Node.js 18+ installed
- Wrangler CLI installed: `npm install -g wrangler`
- CloudFlare account with a registered domain/zone
- Access to SkyCMS Editor backend

### Configuration

Edit `wrangler.toml` to match your environment:

```toml
[vars]
BACKEND_HOST_DNS_NAME = "edit.customwebarts.com"  # Your SkyCMS Editor hostname
ZONE_NAME = "customwebarts.com"                    # Your CloudFlare zone/domain
# ORIGIN_HOST_OVERRIDE = "www.customwebarts.com"  # Optional: force a specific origin
```

- **BACKEND_HOST_DNS_NAME**: The fully qualified hostname of your SkyCMS Editor instance
- **ZONE_NAME**: Your primary domain in CloudFlare (used for CORS validation)
- **ORIGIN_HOST_OVERRIDE**: Optional. If set, all requests will use this as the origin, overriding the inbound host. Useful if your public domain differs from your backend domain.

## Deployment

### Local Testing

```bash
# Install dependencies
npm install

# Run locally with Wrangler
wrangler dev

# The worker will be available at http://localhost:8787
```

Test with curl:
```bash
curl -X GET http://localhost:8787/_api/contact/form \
  -H "Host: customwebarts.com"

curl -X POST http://localhost:8787/_api/contact/submit \
  -H "Host: customwebarts.com" \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com"}'
```

### Deploy to CloudFlare

```bash
# Deploy to CloudFlare
wrangler deploy

# View deployment status
wrangler deployments list
```

The worker will automatically be available at the routes configured in `wrangler.toml`:
- `https://edit.customwebarts.com/_api/contact/*`
- `https://customwebarts.com/_api/contact/*`

## Code Explanation

### Entry Point (`index.js`)

The worker exports a default fetch handler that processes all incoming requests.

#### `isOriginAllowed(origin, zoneName)`

Helper function that validates CORS requests:

- Extracts hostname from the origin URL
- Checks if it matches the zone domain exactly or is a subdomain
- Returns `true` only if origin is from your zone
- Handles malformed URLs gracefully

Example:
```javascript
isOriginAllowed("https://www.customwebarts.com", "customwebarts.com")  // true
isOriginAllowed("https://api.customwebarts.com", "customwebarts.com")  // true
isOriginAllowed("https://external.com", "customwebarts.com")           // false
```

#### Request Handler

1. **Path Validation** - Returns 404 if request doesn't target `/_api/contact/*`
2. **Method Validation** - Only allows GET, POST, OPTIONS; returns 405 for others
3. **CORS Preflight** - Handles OPTIONS requests with zone-restricted CORS headers
4. **Proxy Logic**:
   - Builds upstream URL with HTTPS protocol
   - Sets critical headers:
     - `host` - Backend hostname
     - `x-origin-hostname` - Tenant identifier for SkyCMS multi-tenant system
   - Forwards request to backend
   - Returns backend response unchanged (preserves headers and status)
5. **Error Handling** - Catches exceptions and returns 503, logs details for debugging

### Configuration (`wrangler.toml`)

- **name** - CloudFlare project identifier
- **main** - Entry point for the worker
- **compatibility_date** - Ensures consistent CloudFlare Workers API behavior
- **[vars]** - Environment variables accessible in the worker code
- **routes** - URL patterns this worker handles (must match your CloudFlare zone)

## CORS Security

The worker implements **zone-scoped CORS** to prevent unauthorized domains from accessing your contact endpoint.

### How It Works

When a browser makes a cross-origin request (e.g., from a client-side form), it sends an `Origin` header. The worker validates:

```javascript
const requestOrigin = request.headers.get("origin");
const allowedOrigin = isOriginAllowed(requestOrigin, ZONE_NAME) ? requestOrigin : null;
```

- ✅ Requests from `customwebarts.com` or subdomains are allowed
- ❌ Requests from external domains are rejected (no CORS headers sent)

### For Development

If you need to test from a different domain, temporarily set `ORIGIN_HOST_OVERRIDE` in `wrangler.toml`.

## Logging

The worker logs important events to CloudFlare's logging system:

### View Logs

**CloudFlare Dashboard:**
1. Go to Workers → Your Worker → Real-time logs tab
2. See incoming requests and any errors

**Command Line:**
```bash
# Stream live logs to your terminal
wrangler tail

# Filter logs
wrangler tail --status error
```

### Log Messages

- **Rejected method**: `[ContactUsApi] Rejected GET request from example.com` (when method not in GET/POST/OPTIONS)
- **Proxy error**: `[ContactUsApi] Error proxying request: <error details>` (network/timeout issues)

## Security Considerations

1. **HTTPS Only** - All upstream communication is forced to HTTPS
2. **CORS Validation** - Only zones you control can access the endpoint
3. **Host Header Protection** - The `host` header is explicitly set to backend hostname
4. **Method Restrictions** - Only necessary HTTP methods are allowed
5. **Tenant Isolation** - The `x-origin-hostname` header ensures multi-tenant isolation in SkyCMS

## Troubleshooting

### 405 Method Not Allowed

**Cause**: Request method is not GET, POST, or OPTIONS.

**Solution**: Check that form submissions use POST and preflight requests use OPTIONS.

### 503 Service Unavailable

**Cause**: Backend is unreachable, timed out, or returned an error.

**Action**: 
1. Check logs: `wrangler tail --status error`
2. Verify `BACKEND_HOST_DNS_NAME` is correct and accessible
3. Ensure SkyCMS Editor is running and healthy
4. Check CloudFlare network connectivity

### CORS Errors in Browser Console

**Cause**: Request origin is not from your zone domain.

**Examples of errors:**
- Request from `localhost:3000`
- Request from `external-domain.com`
- Request without Origin header (some clients)

**Solution**: 
1. Test from a domain matching your `ZONE_NAME`
2. Or set `ORIGIN_HOST_OVERRIDE` for testing (not recommended for production)

### Tenant Not Resolving in SkyCMS

**Cause**: `x-origin-hostname` header not being set correctly.

**Action**:
1. Check logs for the actual origin being sent
2. Verify your SkyCMS `IDynamicConfigurationProvider` recognizes the origin
3. Confirm settings exist for that domain in your database

## Development Workflow

```bash
# 1. Make changes to index.js or wrangler.toml
# 2. Test locally
wrangler dev

# 3. Deploy when ready
wrangler deploy

# 4. Monitor production
wrangler tail
```

## Related Documentation

- [CloudFlare Workers Documentation](https://developers.cloudflare.com/workers/)
- [Wrangler CLI Reference](https://developers.cloudflare.com/workers/wrangler/)
- SkyCMS Multi-tenant Architecture (see copilot-instructions.md)

## Support

For issues or questions:
1. Check the Troubleshooting section above
2. Review CloudFlare logs: `wrangler tail`
3. Verify configuration in `wrangler.toml` matches your environment
4. Test locally with `wrangler dev` before deploying
