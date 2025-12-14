{% include nav.html %}

# GitHub Pages Setup Instructions

Your documentation is now organized and ready for GitHub Pages deployment! Follow these steps to enable it:

## Step 1: Enable GitHub Pages in Repository Settings

1. Go to your GitHub repository: https://github.com/CWALabs/SkyCMS
2. Click **Settings** (gear icon)
3. In the left sidebar, click **Pages** (under "Code and automation")
4. Under "Build and deployment":
   - **Source**: Select "Deploy from a branch"
   - **Branch**: Select `main` (or your default branch)
   - **Folder**: Select `/ (root)` or `/docs` (depending on your setup)
5. Click **Save**

> **Note:** GitHub will automatically detect your `_config.yml` and use Jekyll to build the site.

## Step 2: Wait for Deployment

GitHub will automatically build and deploy your site. This may take 1-2 minutes.

You'll see a notification like:
> "Your site is published at `https://cwalabs.github.io/SkyCMS/`"

## Step 3: Verify Your Site

Visit your new documentation site:
- **URL**: `https://yourusername.github.io/SkyCMS/`
- **Example**: `https://cwalabs.github.io/SkyCMS/`

## File Structure Created

```
Docs/
├── _config.yml              # GitHub Pages Jekyll configuration
├── CHANGELOG.md             # Moved from root
├── README.md                # Main documentation home
├── MASTER_TOC.md            # Updated table of contents
├── Components/
│   ├── AspNetCore.Identity.FlexDb.md    # Identity library docs
│   ├── Cosmos.Common.md                 # Core library docs
│   └── Cosmos.BlobService.md            # Storage library docs
├── Development/
│   └── Testing/
│       └── README.md        # Testing guide
├── FileManagement/          # (existing)
├── Layouts/                 # (existing)
├── Templates/               # (existing)
├── Widgets/                 # (existing)
├── Editors/                 # (existing)
└── [other existing docs]
```

## Customization Options

### Change the Site Title or Theme

Edit `Docs/_config.yml`:

```yaml
title: "SkyCMS Documentation"  # Change this
theme: jekyll-theme-slate     # Or use another theme (see options below)
```

### Recommended Themes

- `jekyll-theme-slate` (default) - Clean, professional dark theme
- `jekyll-theme-minimal` - Minimal, clean design
- `jekyll-theme-dinky` - Compact, elegant
- `jekyll-theme-cayman` - Modern, colorful
- `jekyll-theme-leap-day` - Modern with navigation

### Custom Domain (Optional)

To use a custom domain like `docs.example.com`:

1. Go to **Settings > Pages**
2. Under "Custom domain", enter your domain
3. Add DNS records to your domain registrar (GitHub will provide instructions)

## Troubleshooting

### Site Not Building?

Check the build logs:
1. Go to **Settings > Pages**
2. Look for "Latest deployment" status
3. Click the build status to see detailed logs

### Links Not Working?

Make sure links use relative paths and include `.md` extension:
```markdown
❌ Wrong:  [Components](Components/)
✅ Correct: [Components](./Components/Cosmos.Common.md)
```

### Theme Not Updating?

Clear your browser cache or use an incognito window to see latest changes.

## Next Steps

1. **Update Root README.md**: Add a link to your GitHub Pages site
2. **Test Links**: Verify all internal documentation links work
3. **Monitor Updates**: GitHub Pages will auto-rebuild on every push
4. **Add Navigation**: Consider updating your main README.md with a documentation link

## Advanced: Local Testing

To test locally before pushing:

```bash
# Install Jekyll (requires Ruby)
gem install jekyll bundler

# Navigate to Docs folder
cd Docs

# Build and serve locally
jekyll serve

# Visit http://localhost:4000/SkyCMS/
```

## Resources

- [GitHub Pages Documentation](https://docs.github.com/pages)
- [Jekyll Documentation](https://jekyllrb.com/)
- [Markdown Reference](https://guides.github.com/features/mastering-markdown/)

---

Your documentation is now ready for the world to see! 🎉
