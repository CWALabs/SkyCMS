---
title: Search Widget
description: Client-side search form widget for SkyCMS search endpoint with result rendering
keywords: widget, search, client-side, search-form, results
audience: [developers]
---

# Cosmos CMS Search Widget

Client-side helpers for a simple search form that posts to the SkyCMS search endpoint and renders results.

- Location (source): `Editor/wwwroot/lib/cosmos/search.js`
- Expected DOM ids:
  - `frmSearch` — the form element
  - `searchTxt` — search input
  - `btnReset` — reset button (shows after results)
  - `divResults` — results container

> Note: The current script contains two minor issues in naming/wiring (a missing submit handler reference and a function name mismatch). The example below shows a corrected wiring using `postSearchData()` and a proper `handleSearch` function.

## Endpoint

- POST `/Home/CCMS___SEARCH`
- Body: form data from `frmSearch`
- Returns JSON array with items like `{ urlPath, title, updated, authorInfo }`

## Minimal HTML example

```html
<form id="frmSearch">
  <input id="searchTxt" name="q" type="search" placeholder="Search…" />
  <button type="submit">Search</button>
  <button id="btnReset" type="button" style="display:none">Reset</button>
</form>
<div id="divResults" style="display:none"></div>

<link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/@fortawesome/fontawesome-free@6.5.2/css/all.min.css" />
<script src="/lib/cosmos/search.js"></script>
<script>
  // Wire the submit to a proper handler
  document.getElementById('frmSearch').addEventListener('submit', handleSearch);

  async function handleSearch(event) {
    event.preventDefault();
    try {
      const data = await postSearchData();
      const reset = document.getElementById('btnReset');
      const results = document.getElementById('divResults');
      reset.style.display = 'block';
      results.style.display = 'block';
      results.innerHTML = '';

      if (!data || data.length === 0) {
        results.innerHTML = '<p>No results returned.</p>';
        return;
      }

      const p = document.createElement('p');
      p.textContent = `Found ${data.length} result${data.length === 1 ? '' : 's'}:`;
      p.classList.add('underline', 'underline-offset-2', 'mb-4');
      results.appendChild(p);

      const links = data.map(item => ({
        url: item.urlPath,
        text: item.title,
        updated: item.updated,
        author: item.authorInfo
      }));

      results.appendChild(buildOrderedList(links));
    } catch (err) {
      console.error(err);
    }
  }

  // Reset button already wired by script; ensure it exists
</script>
```

## Rendering helper

The included `buildOrderedList(links)` returns an `<ol>` of result entries and appends a Font Awesome icon to each. Include FA as shown above.
