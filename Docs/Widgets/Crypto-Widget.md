---
title: Crypto Widget (AES Helpers)
description: Client-side encryption/decryption helpers using CryptoJS AES-128 for Editor UI
keywords: widget, crypto, encryption, AES, CryptoJS, security
audience: [developers]
---

# Cosmos CMS Crypto Widget (AES helpers)

Lightweight client-side helpers for encrypting and decrypting short strings in the Editor UI or custom pages. Uses CryptoJS AES-128/CBC/PKCS7 with Base64 output.

- Location (source): `Editor/wwwroot/lib/cosmos/crypto-js/cryptpo-js-encrypt.js`
- Exposes two globals:
  - `encryptData(plainText: string): string`
  - `decryptData(cipherText: string): string`

> Note: This utility is intended for convenience and UI-level obfuscation. Do not rely on static client-side keys for protecting sensitive data in production.

## Features

- AES encryption/decryption using CryptoJS
- CBC mode, PKCS7 padding
- Base64-encoded ciphertext
- Graceful handling of empty/undefined inputs (returns empty string)

## Dependencies

Load CryptoJS before this script. You can use a CDN or your own hosted copy.

- CryptoJS (includes AES): e.g.
  - `https://cdn.jsdelivr.net/npm/crypto-js@4.2.0/crypto-js.min.js`
- This widget script:
  - `/lib/cosmos/crypto-js/cryptpo-js-encrypt.js`

The script expects a global `CryptoJS` object.

## Security considerations

- The current implementation uses a fixed 16-byte key and IV inside the script (`"1234567890123456"`). This is not secure for protecting sensitive data.
- Recommended options:
  - Prefer server-side encryption for secrets, tokens, and PII.
  - If client-side encryption is necessary, inject a per-session key securely from the server (not hard-coded in JS) and avoid reusing IVs.
  - Consider parameterizing the functions to accept a key/iv rather than using static values.
  - Always transmit over HTTPS.

## Simple usage

```html
<!doctype html>
<html lang="en">
  <head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Crypto Widget Demo</title>
  </head>
  <body class="p-4">
    <h1 class="h5">Crypto Widget Demo</h1>

    <div style="display:grid; gap:8px; max-width:640px;">
      <label>
        Plain text
        <textarea id="plain" rows="3" style="width:100%;"></textarea>
      </label>

      <div>
        <button id="btn-encrypt">Encrypt →</button>
        <button id="btn-decrypt">← Decrypt</button>
      </div>

      <label>
        Cipher text (Base64)
        <textarea id="cipher" rows="3" style="width:100%;"></textarea>
      </label>
    </div>

    <!-- Dependencies -->
    <script src="https://cdn.jsdelivr.net/npm/crypto-js@4.2.0/crypto-js.min.js"></script>

    <!-- Widget helpers (path served by the Editor site) -->
    <script src="/lib/cosmos/crypto-js/cryptpo-js-encrypt.js"></script>

    <script>
      const $ = (id) => document.getElementById(id);

      $('btn-encrypt').addEventListener('click', () => {
        const text = $('plain').value;
        $('cipher').value = encryptData(text);
      });

      $('btn-decrypt').addEventListener('click', () => {
        const c = $('cipher').value;
        $('plain').value = decryptData(c);
      });
    </script>
  </body>
</html>
```

## Programmatic example

```js
// Requires CryptoJS to be loaded and the widget script included
const secret = encryptData('hello world');
const back = decryptData(secret);
console.log({ secret, back }); // { secret: '...', back: 'hello world' }
```

## Implementation notes

The current implementation in `cryptpo-js-encrypt.js` does the following:

- Parses a 16-byte UTF-8 key and IV from the same string (AES-128)
- Uses `CryptoJS.AES.encrypt(plainText, key, { iv, padding: CryptoJS.pad.Pkcs7, mode: CryptoJS.mode.CBC })`
- Serializes to Base64 text via `encrypted.toString()`
- For decryption, calls `CryptoJS.AES.decrypt` with the same key/IV and converts to UTF-8 string.

If you need stronger security or configuration, consider refactoring to accept key/iv parameters and sourcing them securely (not embedded).
