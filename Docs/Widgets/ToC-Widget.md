{% include nav.html %}

# Cosmos CMS Table of Contents (ToC) Widgets

Helpers to render navigation lists from the SkyCMS Table of Contents API and to build breadcrumbs automatically.

- Location (source): `Editor/wwwroot/lib/cosmos/toc.js`
- Public functions:
  - `cosmos_cms_build_toc_default(navElementId, anchorClassName?, activeCssClass?)`
  - `cosmos_cms_build_toc(pagePath, navElementId, liClassName?, anchorClassName?, activeCssClass?)`
  - `cosmos_cms_build_breadcrumbs(breadCrumbId)`

## API endpoints

- GET `/Home/GetTOC?page=/{pagePath}`
  - Optional query parameters: `pageNo`, `pageSize`, `orderByPub`
  - Returns JSON with `Items` array: `{ UrlPath, Title, Published, Updated, BannerImage, AuthorInfo }`

## cosmos_cms_build_toc_default

Reads the current page path from the head meta tag and renders children pages under it.

- Requires `<meta name="cwps-meta-path-url" content="docs/getting-started" />` in `<head>`
- If content is `root`, uses an empty path (top-level)

Example:

```html
<ul id="ulToc"></ul>
<meta name="cwps-meta-path-url" content="docs/getting-started" />
<script src="/lib/cosmos/toc.js"></script>
<script>
  cosmos_cms_build_toc_default('ulToc', 'nav-link', 'active');
</script>
```

## cosmos_cms_build_toc

Manually specify a root path and target container. The function attaches a `DOMContentLoaded` listener and uses XHR to fetch the ToC.

- `pagePath`: path without a leading slash (e.g., `docs/getting-started`)
- `navElementId`: id of `<ul>` or `<ol>` container
- `liClassName`: optional class name for each `<li>`
- `anchorClassName`: optional class name for each `<a>`
- `activeCssClass`: optional; if set, anchors that match the current `window.location.pathname` get this class

Example:

```html
<ul id="ulToc" class="list-unstyled"></ul>
<script src="/lib/cosmos/toc.js"></script>
<script>
  cosmos_cms_build_toc('docs', 'ulToc', 'nav-item', 'nav-link', 'active');
</script>
```

## cosmos_cms_build_breadcrumbs

Builds breadcrumb `<li><a></a></li>` items from the page path meta tag and document title (segments separated by `/`).

Example:

```html
<meta name="cwps-meta-path-url" content="docs/getting-started/installation" />
<title>Docs/Getting Started/Installation</title>
<ol id="olCrumbs"></ol>
<script src="/lib/cosmos/toc.js"></script>
<script>
  cosmos_cms_build_breadcrumbs('olCrumbs');
</script>
```

## Notes

- Link text uses the last segment of `item.Title`.
- Link href uses `"/" + item.UrlPath`.
- When `activeCssClass` is provided, the script will compare `href` to the current path (case-insensitive) and add the class when it matches.
