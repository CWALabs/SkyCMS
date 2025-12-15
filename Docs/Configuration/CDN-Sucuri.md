# Sucuri CDN/WAF with SkyCMS

SkyCMS can trigger cache purge on Sucuri after publish.

## Values you need

- **API Key**
- **API Secret**

## Get credentials from Sucuri dashboard

1. Log in to your Sucuri dashboard.
2. Select the site you protect with Sucuri.
3. Go to **API** (or Account → API). Copy the **API Key** and **API Secret** for that site.

## Configure in SkyCMS

1. In the Editor, go to **Settings → CDN**.
2. Under **Sucuri CDN/Firewall**, enter:
   - **API Key**
   - **API Secret**
3. Save and test. If the test fails, verify the key/secret and that your site is active in Sucuri.

## Tips

- Store the key/secret in your secret manager (Key Vault, Secrets Manager, etc.) and inject via environment variables.
- Rotate the credentials periodically and update SkyCMS.
