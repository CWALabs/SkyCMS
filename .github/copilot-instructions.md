# Copilot Instructions

## General Guidelines
- First general instruction
- Second general instruction

## Architecture Overview
- The SkyCMS multi-tenant architecture utilizes the following components:
  - **IDynamicConfigurationProvider** (singleton) for tenant resolution via headers (x-origin-hostname priority over Host header).
  - **Per-request scoped services** that inject the provider to get the current tenant.
  - **Cookie isolation** with CookieDomain claims in Sky.Editor.
  - **Early middleware** (DomainMiddleware) to establish tenant context.
  - **Settings queries** filtered by tenant domain.
  - **Rate limiter policy** "contact-form" already configured (3 req/5min in production, 20 req/1min in development).
  - **Antiforgery tokens** automatically scoped per HttpContext (per-tenant).