// Check if origin is allowed from the zone domain
function isOriginAllowed(origin, zoneName) {
  if (!origin) return false;
  try {
    const originUrl = new URL(origin);
    const originHost = originUrl.hostname;
    // Allow apex domain and any subdomain of the zone
    return originHost === zoneName || originHost.endsWith('.' + zoneName);
  } catch {
    return false;
  }
}

export default {
  async fetch(request) {
    const inboundUrl = new URL(request.url);

    // Only handle /_api/contact/*
    if (!inboundUrl.pathname.startsWith("/_api/contact")) {
      return new Response("Not found", { status: 404 });
    }

    // Restrict to GET, POST and OPTIONS methods
    const method = request.method;
    if (!["GET", "POST", "OPTIONS"].includes(method)) {
      console.warn(`[ContactUsApi] Rejected ${method} request from ${inboundUrl.host}`);
      return new Response("Method not allowed", { status: 405 });
    }

    // Handle CORS preflight
    if (method === "OPTIONS") {
      // Determine allowed origin from request or restrict to zone
      const requestOrigin = request.headers.get("origin");
      const allowedOrigin = isOriginAllowed(requestOrigin, ZONE_NAME) ? requestOrigin : null;

      return new Response(null, {
        status: 204,
        headers: allowedOrigin ? {
          "Access-Control-Allow-Origin": allowedOrigin,
          "Access-Control-Allow-Methods": "GET, POST, OPTIONS",
          "Access-Control-Allow-Headers": "Content-Type",
        } : {
          "Access-Control-Allow-Methods": "GET, POST, OPTIONS",
        },
      });
    }

    try {
      // Build upstream URL (force HTTPS)
      const upstream = new URL(inboundUrl.toString());
      upstream.protocol = "https:";
      upstream.hostname = BACKEND_HOST_DNS_NAME.trim();
      upstream.port = "";

      // Determine x-origin-hostname
      const originHostOverride = ORIGIN_HOST_OVERRIDE?.trim();
      const originHost = originHostOverride || inboundUrl.host;

      // Clone headers and set host + x-origin-hostname
      const headers = new Headers(request.headers);
      headers.set("host", upstream.host);
      headers.set("x-origin-hostname", originHost);

      // Build upstream request
      const upstreamRequest = new Request(upstream.toString(), {
        method: request.method,
        headers,
        body: request.method === "GET" || request.method === "HEAD" ? undefined : request.body,
        redirect: "manual",
      });

      const response = await fetch(upstreamRequest, { cf: { cacheEverything: false } });

      return new Response(response.body, {
        status: response.status,
        statusText: response.statusText,
        headers: response.headers,
      });
    } catch (error) {
      console.error(`[ContactUsApi] Error proxying request: ${error.message}`, {
        url: inboundUrl.toString(),
        method: method,
      });
      return new Response("Service unavailable", { status: 503 });
    }
  },
};