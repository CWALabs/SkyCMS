{% include nav.html %}

# Cosmos CMS Nav Builder

Builds a simple list of links (child pages) under a given root path using the SkyCMS Table of Contents API.

- Location (source): `Editor/wwwroot/lib/cosmos/nav-builder.js`
- Primary API: `ccms___NavBuilder(pagePath, navElementId, liClassName?, anchorClassName?)`

## How it works

- Fetches `/Home/GetTOC?page=/{pagePath}` (GET) and expects JSON with an `Items` array.
- For each item, takes the last segment of `item.Title` for link text and uses `"/" + item.UrlPath` for the `href`.
- Appends `<li><a></a></li>` to the container with `id=navElementId`.

## Minimal HTML example

```html
<ul id="ulNav" class="nav flex-column"></ul>

<script src="/lib/cosmos/nav-builder.js"></script>
<script>
  // Build navigation for children under "/docs"
  // li gets Bootstrap class "nav-item" and anchor gets "nav-link"
  ccms___NavBuilder('docs', 'ulNav', 'nav-item', 'nav-link');
</script>
```

## Notes

- Ensure the element with `navElementId` exists before invoking.
- `pagePath` should not include a leading slash.
- You can omit `liClassName` and/or `anchorClassName` to avoid adding classes.
