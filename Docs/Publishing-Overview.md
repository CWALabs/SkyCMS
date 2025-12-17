---
title: Publishing Overview
description: Publishing modes and workflows for making content live on your SkyCMS website
keywords: publishing, workflow, deployment, content-lifecycle, versioning
audience: [developers, content-creators, administrators]
---

# Publishing in SkyCMS

Publishing is the process of making content live on your website. SkyCMS supports multiple publishing modes and workflows to fit different content strategies and team structures.

---

## Table of Contents

- [Publishing Overview](#publishing-overview)
- [Publishing Modes](#publishing-modes)
- [Publishing Workflows](#publishing-workflows)
- [Publishing Steps](#publishing-steps)
- [Scheduling & Automation](#scheduling--automation)
- [Unpublishing & Archiving](#unpublishing--archiving)
- [Best Practices](#best-practices)

---

## Publishing Overview

Publishing makes content visible to your website visitors. Before publishing, content exists only in the SkyCMS editor.

### Content States

| State | Visibility | Audience | Can Edit |
|-------|-----------|----------|----------|
| **Draft** | Private | Editors only | Yes |
| **Scheduled** | Private | Editors only | No |
| **Published** | Public | All visitors | Depends |
| **Archived** | Private | Editors only | No |

### Publishing Modes

SkyCMS supports different publishing modes to accommodate various hosting and workflow needs:

1. **Direct Publishing** - Publish directly to live site immediately
2. **Staged Publishing** - Publish to staging environment first, then promote to production
3. **Static Generation** - Generate static files for hosting anywhere
4. **Git-Based Publishing** - Commit content to Git repository

---

## Publishing Modes

### 1. Direct Publishing

**How it works:**
- Content is published directly to live website
- Changes appear immediately
- No intermediate steps

**When to use:**
- Small teams with trusted editors
- Rapid content updates needed
- Real-time content (news, breaking announcements)
- Development/testing environments

**Advantages:**
- Fast: Changes live in seconds
- Simple: No complex workflows
- Immediate: No waiting for CI/CD

**Disadvantages:**
- Risky: Mistakes immediately visible
- No review process: Can't vet changes before publishing
- No rollback: Errors require immediate fixing

**Configuration:**
```json
{
  "Publishing": {
    "Mode": "Direct",
    "Target": "https://production.example.com"
  }
}
```

---

### 2. Staged Publishing

**How it works:**
- Content published to staging environment first
- Review, test, and verify on staging
- Promote to production when ready
- Rollback possible if issues found

**When to use:**
- Teams requiring review before publishing
- Complex content with dependencies
- High-visibility websites
- Regulated industries (finance, healthcare)

**Advantages:**
- Safe: Review before going live
- Testable: Verify on staging first
- Reversible: Rollback if needed
- Auditable: Track all changes

**Disadvantages:**
- Slower: Additional approval steps
- Complex: Multiple environments to manage
- Overhead: More process required

**Workflow:**
```
1. Edit in Editor (Draft)
   ↓
2. Request Publication (Pending Review)
   ↓
3. Review on Staging
   ↓
4. Approve/Reject
   ↓
5. If Approved → Promote to Production (Published)
   ↓
6. If Rejected → Return to Draft
```

**Configuration:**
```json
{
  "Publishing": {
    "Mode": "Staged",
    "Staging": "https://staging.example.com",
    "Production": "https://example.com",
    "RequireApproval": true
  }
}
```

---

### 3. Static Generation

**How it works:**
- Content generates static HTML files
- Static files hosted on any web server or CDN
- Fast, secure, scalable deployment
- Typical JAMstack approach

**When to use:**
- High-traffic sites
- Security-critical content
- CDN distribution needed
- Serverless/static hosting (GitHub Pages, Netlify, etc.)
- Migrating from JAMstack

**Advantages:**
- Fast: Static files are very fast
- Secure: No server-side processing
- Scalable: Minimal server resources
- Cost-effective: Cheap hosting options
- Migrates to existing static host easily

**Disadvantages:**
- Build time: Generation takes seconds/minutes
- Complexity: Requires build process
- Limitations: Some dynamic features not available
- Stale content: Needs regeneration for updates

**Workflow:**
```
1. Edit in Editor
   ↓
2. Publish (triggers generation)
   ↓
3. Generate Static Files (HTML, CSS, JS)
   ↓
4. Deploy to Web Server/CDN
   ↓
5. Site is Live
```

**Configuration:**
```json
{
  "Publishing": {
    "Mode": "StaticGeneration",
    "Generator": "Jekyll",
    "OutputFolder": "./publish",
    "BuildCommand": "jekyll build",
    "DeployTarget": "GitHub Pages"
  }
}
```

---

### 4. Git-Based Publishing

**How it works:**
- Content stored as files in Git repository
- Publishing commits changes to repository
- CI/CD pipeline deploys from repository
- Familiar workflow for developers

**When to use:**
- Developer-focused teams
- Existing CI/CD pipelines
- Migrating from Git-based systems
- Version control required
- Content in source repository

**Advantages:**
- Version control: Full Git history
- CI/CD: Integrate with existing pipelines
- Familiar: Git workflows
- Auditability: All changes tracked
- Reversible: Easy rollback via Git

**Disadvantages:**
- Complexity: Requires Git understanding
- Overhead: More process
- Limited UI: Less visual preview
- Technical: Requires developer involvement

**Workflow:**
```
1. Edit in SkyCMS Editor
   ↓
2. Publish → Commit to Git
   ↓
3. Git Webhook → Triggers CI/CD
   ↓
4. CI/CD Pipeline Builds & Deploys
   ↓
5. Site is Updated
```

**Configuration:**
```json
{
  "Publishing": {
    "Mode": "GitBased",
    "Repository": "https://github.com/user/site.git",
    "Branch": "main",
    "CommitMessage": "Content: Auto-published from SkyCMS",
    "WebhookSecret": "secure-webhook-key"
  }
}
```

---

## Publishing Workflows

### Simple Workflow (Small Teams)

**For:** Single editor, rapid updates, no review needed

```
1. Edit Page/Post
2. Click "Publish"
3. Content is live
```

**Time:** Immediate  
**Approval:** None  
**Risk:** High (mistakes immediately visible)

---

### Editorial Workflow (Managed Teams)

**For:** Multiple editors, need review, want quality control

```
1. Editor creates content (Draft)
2. Editor requests publication
3. Reviewer checks on Staging
4. Reviewer approves or requests changes
5. If approved → Promoted to Production
6. If rejected → Returned to Draft
```

**Time:** Minutes to hours  
**Approval:** Required  
**Risk:** Low (reviewed before publishing)

---

### Complex Workflow (Large Organizations)

**For:** Large teams, complex dependencies, regulated environments

```
1. Author writes content (Draft)
2. Editor reviews and requests changes
3. Author updates content
4. Editor approves for publication (Pending)
5. Checker verifies on Staging
6. Compliance reviews metadata/permissions
7. All approvals received → Production deployment
8. Monitoring verifies deployment successful
```

**Time:** Hours to days  
**Approvals:** Multiple (editing, review, compliance, deployment)  
**Risk:** Very low (extensive review process)

---

## Publishing Steps

### Step-by-Step: Publishing Page Content

1. **Edit Your Content**
   - Open page in Live Editor
   - Make changes to page content
   - Use widgets for media and forms
   - Preview your changes

2. **Review Before Publishing**
   - Click "Preview" to see live appearance
   - Check on desktop and mobile
   - Verify all links work
   - Check form functionality
   - Verify images display correctly

3. **Set Publishing Options**
   - **Publish Date** - Today or schedule for later
   - **Visibility** - Public, private, password-protected
   - **Permissions** - Who can access
   - **SEO** - Meta title, description, keywords
   - **Redirect** - Set if replacing old URL

4. **Request Publication** (if using workflow)
   - Click "Request Publication"
   - Add publication message
   - Assign to reviewer
   - Wait for approval

5. **Publish**
   - Click "Publish" to make live
   - Or wait for approval then promote
   - Content is now visible to visitors

6. **Verify**
   - Visit live website
   - Check page displays correctly
   - Verify all functionality works
   - Check social media preview (if applicable)

### Step-by-Step: Publishing Blog Posts

1. **Write Your Post**
   - Create new blog post
   - Write content using Live Editor
   - Add featured image
   - Add categories/tags

2. **Set Post Settings**
   - **Title** - Catchy, descriptive
   - **Slug** - URL-friendly identifier
   - **Excerpt** - Summary for listings
   - **Categories** - Classify your post
   - **Tags** - Topic keywords
   - **Author** - Who wrote it
   - **Featured Image** - Thumbnail

3. **Schedule Publication**
   - **Publish Date** - When should it go live
   - **Schedule** - Automatic publication at time
   - **Timezone** - Correct timezone
   - **Notify Subscribers** - Email notification

4. **Request Review** (if using workflow)
   - Submit for editor review
   - Wait for feedback
   - Make requested changes
   - Resubmit if needed

5. **Publish Post**
   - Click "Publish" or schedule
   - Post appears in blog listings
   - Social media notification sent (if enabled)
   - Subscribers notified

6. **Share**
   - Share link on social media
   - Email to subscribers
   - Add to newsletter
   - Link from other pages

---

## Scheduling & Automation

### Scheduled Publishing

Publish content at a specific future date and time:

1. **Set Publish Date** - Choose when content should go live
2. **Verify Timezone** - Ensure correct timezone selected
3. **Confirm Scheduling** - Double-check date and time
4. **Set & Forget** - SkyCMS publishes automatically

**Use Cases:**
- Blog posts at specific time
- News announcements at launch
- Promotional content on schedule
- Time-zone appropriate posts
- Content drip campaigns

**Configuration:**
```
Publishing Date: December 25, 2025
Publishing Time: 10:00 AM
Timezone: Eastern Time (US)
```

### Recurring/Automated Publishing

Automatically republish content on schedule:

```
Publish: Every Monday at 9:00 AM
Frequency: Weekly
Keep: Previous version archived
```

**Use Cases:**
- Weekly blog features
- Regular newsletter content
- Rotating promotions
- Scheduled announcements
- Content refreshing

### Conditional Publishing

Publish based on conditions:

```
If URL contains "/summer/"
  Then set visibility to public
  And add category "Seasonal"
  And schedule unpublishing for Sept 1
```

---

## Unpublishing & Archiving

### Unpublishing Content

Remove content from public view but keep for reference:

1. **Find Published Content** - Locate in content list
2. **Click "Unpublish"** - Remove from public
3. **Choose Archive** - Optionally archive
4. **Confirm** - Content now private

**Preservation:** Content remains in editor; can be republished

### Archiving Content

Move old content to archive:

1. **Select Content** - Choose to archive
2. **Click "Archive"** - Move to archive
3. **Optional Message** - Note reason for archiving
4. **Archived** - Removed from active list, saved for reference

**Preservation:** Can search archive and restore if needed

### Redirects

When replacing or removing pages, set up redirects:

1. **Old URL** - `https://example.com/old-page`
2. **New URL** - `https://example.com/new-page`
3. **Redirect Type** - Permanent (301) or Temporary (302)
4. **Save** - Redirect now active

**Best Practices:**
- Always redirect old URLs
- Use permanent (301) for content moves
- Use temporary (302) for short-term changes
- Update internal links when possible

---

## Best Practices

### Before Publishing

- **Write First, Publish Later** - Finish content before publishing
- **Review Your Work** - Check spelling, grammar, links
- **Preview Thoroughly** - Check desktop and mobile views
- **Test Interactive Elements** - Forms, buttons, videos
- **Verify Images** - All images load and display correctly
- **Check SEO** - Meta title, description, keywords
- **Proofread** - Multiple readings catch more errors

### Publishing Decisions

- **Know Your Audience** - Who needs to see this?
- **Choose Right Time** - When should content go live?
- **Set Visibility** - Who should see this content?
- **Plan Promotion** - How will you publicize it?
- **Consider Scheduling** - Automatic publishing when appropriate

### After Publishing

- **Verify Live** - Visit website and confirm content is live
- **Check Links** - Test all links work
- **Test Forms** - Submit test form if applicable
- **Social Media** - Share if appropriate
- **Monitor** - Watch for errors or issues
- **Respond to Feedback** - Answer comments/questions

### Publishing Frequency

- **Be Consistent** - Regular publishing builds audience
- **Quality over Quantity** - Don't publish just to publish
- **Plan Ahead** - Schedule content in advance
- **Avoid Overload** - Don't publish too frequently
- **Seasonal** - Adjust for seasonal content patterns

### Team Communication

- **Document Workflow** - Make process clear
- **Assign Responsibility** - Who publishes what?
- **Set Deadlines** - When should content be ready?
- **Review Schedule** - When do reviewers check work?
- **Communication** - Use message/notes feature

---

## See Also

- **[Blog Post Lifecycle](./blog/BlogPostLifecycle.md)** - Creating and publishing blog posts
- **[Page Scheduling](./Editors/PageScheduling.md)** - Schedule pages for automatic publication
- **[CDN Configuration](./Configuration/CDN-Overview.md)** - Cache purging on publish
- **[Authentication & Authorization](./Authentication-Overview.md)** - Access control and permissions
- **[LEARNING_PATHS: Content Editor](./LEARNING_PATHS.md#-content-editor-non-technical)** - Content creation guide
- **[LEARNING_PATHS: DevOps](./LEARNING_PATHS.md#️-devops--system-administrator)** - Publishing workflow setup
- **[QuickStart: Publishing Content](./QuickStart.md)** - Quick reference
- **[Troubleshooting Guide](./Troubleshooting.md)** - Publishing issues and solutions
- **[Main Documentation Hub](./README.md)** - Browse all documentation

---

## Related Documentation (To Be Created)

- Publishing workflows and approval processes
- Git-based publishing setup and automation
- Static site generation deployment
- Publishing performance optimization
- Content versioning and rollback
- Publishing analytics and monitoring
- Promotional content best practices
- International content and scheduling

---

**Last Updated:** December 17, 2025  
**Owner:** @toiyabe
