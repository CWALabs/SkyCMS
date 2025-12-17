---
title: Phase 1 - Design & Planning
description: Architecture understanding, site planning, brand identity, sitemap, wireframes, and content strategy
keywords: design, planning, architecture, sitemap, wireframes, content-strategy, phase-1
audience: [developers, designers, project-managers]
---

# Phase 1: Design & Planning Guide

**Timeline:** 3-5 hours  
**Typical Effort:** Design-first approach, 1-2 stakeholders  
**Output:** Architecture diagram, site structure outline, content inventory

## Overview

Phase 1 is where you establish the foundation for your SkyCMS website. Before creating a single layout or template, you need to understand how your content will be organized and how users will navigate your site. This phase focuses on strategic planning and architectural decisions that will guide every subsequent phase.

### Why This Phase Matters

Many projects rush past planning and jump straight to creating pages. This usually results in:
- Inconsistent design across pages
- Layouts and templates that don't work well together
- Navigation that confuses users
- Repeated work when designs need to change

By investing time in Phase 1, you create a blueprint that keeps your entire project coherent and efficient.

---

## The SkyCMS Architecture Model: Layouts → Templates → Pages

Before diving into planning, you need to understand how SkyCMS structures content. This hierarchical relationship is central to everything you'll build:

```
┌─────────────────────────────────────────────────────┐
│              SITE LAYOUT(S)                         │
│  (One or more site-wide structural containers)      │
│                                                     │
│  Examples:                                          │
│  - Main Layout (header, nav, footer)               │
│  - Landing Page Layout (no nav, hero focus)        │
│  - Sidebar Layout (content + right sidebar)        │
│                                                     │
│  ┌──────────────┐  ┌──────────────┐  ┌────────────┐│
│  │  Template 1  │  │  Template 2  │  │ Template 3 ││
│  │   (Blog)     │  │  (Product)   │  │   (Page)   ││
│  │              │  │              │  │            ││
│  │  Layout:     │  │  Layout:     │  │  Layout:   ││
│  │  Main        │  │  Main        │  │  Landing   ││
│  │              │  │              │  │            ││
│  └──────────────┘  └──────────────┘  └────────────┘│
│       │                   │                 │       │
│  ┌────┴─────┐         ┌───┴───┐        ┌───┴────┐  │
│  │   Page   │         │ Page  │        │  Page  │  │
│  │  (Blog   │         │ (Prod)│        │ (Info) │  │
│  │  Post 1) │         │       │        │        │  │
│  │   Page   │         │ Page  │        │  Page  │  │
│  │  (Blog   │         │ (Prod)│        │ (Info) │  │
│  │  Post 2) │         │       │        │        │  │
│  └──────────┘         └───────┘        └────────┘  │
│                                                     │
└─────────────────────────────────────────────────────┘
```

### What Each Component Does

**Layouts** provide the structural container for your site:
- Define where the header, navigation, footer, and main content area live
- Enforce consistency across pages that use the same layout
- Handle site-wide elements (branding, navigation menus, footers)
- Typically 1-3 per site (sometimes more for specialized sections)

**Templates** define the blueprint for a specific type of content:
- Sit inside a layout and define the content area structure
- Include fields, widgets, and section arrangement
- Examples: "Blog Post Template", "Product Page Template", "Service Page Template"
- Multiple templates can share the same layout
- Typically 3-8 per site depending on content diversity

**Pages** are the actual content instances:
- Individual web pages created from templates
- Contain the actual text, images, and data
- Published to make them visible to the public
- Can be created, edited, published, and archived by content editors

### Why This Hierarchy Matters for Planning

During Phase 1, you need to decide:
1. **How many layouts do you need?** (Usually 1-3)
2. **What types of pages will exist?** (This determines your templates)
3. **How will content be organized?** (This determines page structure)

This thinking prevents problems later. For example:
- If you design a layout too wide, all templates using it will look cramped
- If you don't plan for different page types upfront, you'll create templates reactively
- If you don't consider navigation structure, you'll struggle during Phase 5

---

## Step-by-Step Planning Process

### Step 1: Define Your Site's Content Categories

Start by listing all the different types of content your site will publish. Think about your business or organization's main areas:

**Examples by Industry:**

| Industry | Content Types |
|----------|---------------|
| **E-Commerce** | Products, Categories, Blog Posts, Reviews, Testimonials |
| **Service Company** | Services, Case Studies, Team Bios, Testimonials, FAQs |
| **Publisher** | Articles, Categories, Author Bios, Ads, Newsletters |
| **Non-Profit** | Programs, Events, Donations, News, Impact Stories |
| **SaaS** | Features, Pricing, Docs, Blog, Case Studies, Webinars |

**Your Site's Content Types:**

Create a list for your specific project. Don't over-complicate—start with 3-6 main categories:

```
Content Category          | Description
--------------------------|------------------------------------------
[Category 1]             | What will this contain? Who creates it?
[Category 2]             | 
[Category 3]             | 
[Category 4]             | 
```

### Step 2: Understand Your User Journeys

How will visitors navigate through your content? Map out the main user paths:

**Example: E-Commerce Site**
```
Visitor arrives at home
  ├─ Browse product categories
  │  └─ View individual products
  │     └─ Read reviews/testimonials
  └─ Read blog posts
     └─ See related products
```

**Example: Service Company**
```
Visitor arrives at home
  ├─ Explore services
  │  └─ View case studies
  │     └─ Contact for demo
  └─ Learn about team
     └─ See testimonials
```

**Your Site's User Journeys:**

Think about 2-3 primary paths visitors will take through your site.

### Step 3: Plan Your Layout(s)

Now think about the visual structure. Will all pages look the same, or do you need variations?

**Common Layout Scenarios:**

**Single Layout (Simple Site)**
- All pages share: Header → Navigation → Footer
- Works for: blogs, brochure sites, portfolios
- Simplest to maintain and design

**Two Layouts (Moderate Complexity)**
- Main Layout: Header → Navigation → Main Content → Footer
- Landing Layout: Hero Section (no nav) → Features → CTA → Footer
- Works for: SaaS, e-commerce, marketing sites

**Three+ Layouts (Complex Sites)**
- Main Layout: Standard pages with full navigation
- Landing Layout: Hero-focused layouts for campaigns
- Admin Layout: Internal pages with different styling
- Works for: Large enterprise sites, multi-brand projects

**Decision Framework:**

Ask yourself:
- Do all your pages follow the same visual structure? → Use one layout
- Do you have landing pages that need different structure? → Two layouts
- Are some sections visually separate (admin, mobile app)? → Three+ layouts

**Your Layout Plan:**

```
Layout Name          | Used For | Key Sections
---------------------|----------|----------------------------------
[Layout 1]          | Which content types? | Header, Nav, Footer, etc
[Layout 2]          | 
```

### Step 4: Plan Your Templates

Templates translate content types into visual presentations. Each content type from Step 1 becomes a template.

**Key Template Planning Questions:**

1. **Which content types share the same visual layout?**
   - "Blog Post" and "Press Release" might use the same template
   - "Product" and "Service" might share structure with slight variations

2. **What sections does each template need?**
   - Header image area?
   - Sidebar?
   - Related content widget?
   - Author/date information?

3. **What flexibility do content editors need?**
   - Multiple section layouts on same template?
   - Reusable widget sections?
   - Flexible content blocks?

**Template Planning Worksheet:**

```
Content Type: _______________
Layout Used: _______________
Key Sections:
  □ Hero/Header Image
  □ Main Content Area
  □ Sidebar
  □ Author Information
  □ Date/Publication Info
  □ Related [Content Type]
  □ Tags/Categories
  □ Comments Section
  □ Call-to-Action
  □ Other: _______________

Widgets Needed:
  □ Image gallery
  □ Video player
  □ Product carousel
  □ Testimonial slider
  □ Social share buttons
  □ Newsletter signup
  □ Other: _______________
```

**Template Consolidation Strategy:**

Before finalizing, look for opportunities to consolidate:
- Can a "Product" template work for "Service" too? (Consolidate to one)
- Can an "About" page and "Team" page use the same template? (Consolidate)
- Do you really need "Post" and "Article" separately? (Combine)

Most sites need 4-6 templates, not 20. Consolidation makes maintenance easier.

### Step 5: Create Your Site Structure Outline

Now map your planned layouts and templates together. This is your architectural blueprint.

**Visual Site Structure:**

```
SITE ARCHITECTURE
=================

LAYOUTS:
├─ Main Layout (Header | Nav | Content Area | Footer)
└─ Landing Layout (Hero | Content | CTA | Footer)

TEMPLATES (using Main Layout):
├─ Home Page Template
├─ Blog Post Template
├─ Service Page Template
└─ Testimonial Page Template

TEMPLATES (using Landing Layout):
└─ Campaign Landing Template

CONTENT PAGES:
├─ Home (from Home Page Template)
├─ Services
│  ├─ Service 1 (from Service Template)
│  ├─ Service 2
│  └─ Service 3
├─ Blog
│  ├─ Post 1 (from Blog Post Template)
│  ├─ Post 2
│  └─ Post 3
├─ About (from Testimonial Template)
└─ Contact (from Home Page Template - single column)
```

### Step 6: Create a Wireframe Sketch

You don't need Figma or design software. A simple pencil sketch or text-based wireframe shows the layout clearly:

**Text-Based Wireframe Example:**

```
MAIN LAYOUT
===========
┌────────────────────────────────────────┐
│ HEADER (Logo + Tagline)                │
├────────────────────────────────────────┤
│ NAVIGATION (Home | Services | Blog)    │
├────────────────────────────────────────┤
│                                        │
│  [MAIN CONTENT AREA]                  │
│  (Template content goes here)          │
│                                        │
├────────────────────────────────────────┤
│ FOOTER (Copyright | Links)             │
└────────────────────────────────────────┘

LANDING LAYOUT
==============
┌────────────────────────────────────────┐
│ HEADER (Logo only, no nav)             │
├────────────────────────────────────────┤
│ HERO SECTION                           │
│ (Big image + headline + CTA button)    │
├────────────────────────────────────────┤
│ FEATURES SECTION                       │
│ (3 columns with icons)                 │
├────────────────────────────────────────┤
│ TESTIMONIALS SECTION                   │
│ (Carousel or grid)                     │
├────────────────────────────────────────┤
│ CTA SECTION                            │
│ (Final call-to-action)                 │
├────────────────────────────────────────┤
│ FOOTER (Copyright | Links)             │
└────────────────────────────────────────┘
```

**Template Wireframe Example:**

```
BLOG POST TEMPLATE (uses Main Layout)
======================================
[Hero Image - Full Width]

Title | Author | Date | Category Tags

[Main Content Area - 2 Column]
┌────────────────────┬──────────────────┐
│                    │  SIDEBAR:        │
│  Article Content   │  - Related Posts │
│  (Text + Images)   │  - Categories    │
│                    │  - Archives      │
│                    │  - Recent Posts  │
│                    │                  │
└────────────────────┴──────────────────┘

[Next/Previous Post Navigation]
[Comments Section]
```

---

## Planning Your Navigation Strategy

Your site structure should drive your navigation decisions:

### Primary Navigation (Main Menu)

**Rule:** Usually 3-7 main items max

Examples:
- **E-Commerce:** Home | Shop | Blog | About | Contact
- **Service Company:** Home | Services | Case Studies | Team | Contact
- **Publisher:** Home | Categories | About | Contribute | Contact

### Hierarchical Navigation (Multi-Level)

When you have content categories from Step 1:

```
Services (main nav item)
  ├─ Service 1
  ├─ Service 2
  └─ Service 3

Blog (main nav item)
  ├─ Technology
  ├─ Industry News
  └─ Company Updates
```

**Decision:** Do you want dropdown menus, or should each category be a separate page?
- **Dropdowns:** Cleaner nav bar, easier browsing
- **Separate Pages:** Better for SEO, simpler implementation

### Footer Navigation

Typically shows:
- Company/site links (About, Privacy, Terms)
- Duplicate key main nav items
- Social media links
- Contact info

---

## Design Consistency Guidelines

Now's the time to establish design rules that will be implemented in templates:

### Typography Decisions

- How many font families? (Usually 1-2 max)
- How many font sizes? (Headline, subheading, body, small text)
- Line spacing and paragraph margins?

### Color Decisions

- Primary brand color (for links, buttons, accents)
- Secondary accent color
- Neutral colors for text and backgrounds
- How many shades of gray? (Typically 3-4)

### Spacing Decisions

- Margin between sections? (e.g., 40px, 60px)
- Padding inside containers? (e.g., 20px, 30px)
- Space between elements? (e.g., 10px, 15px)

### Image Consistency

- Do featured images have minimum dimensions?
- Should all images have a consistent aspect ratio?
- Any image filters or overlays for consistency?

**Note:** These will be documented in detail in the Phase 4 guide and implemented through templates, but deciding them now prevents later redesigns.

---

## Common Decision Trees

### "Should We Use One Layout or Multiple?"

```
Start: Are all your pages visually similar?
  ├─ YES → Do you have landing pages that need hero sections?
  │        ├─ NO → Use ONE layout for everything
  │        └─ YES → Use TWO layouts (Main + Landing)
  │
  └─ NO → Do different sections have drastically different styling?
          ├─ NO → Use TWO layouts (Main + Specialized)
          └─ YES → Use THREE+ layouts (Main, Specialized, Admin)
```

### "How Many Templates Do We Need?"

```
Start: List your content types
Count the distinct visual structures needed
  ├─ 1-2 structures → 2-3 templates (consolidate similar types)
  ├─ 3-4 structures → 4-6 templates (normal for medium sites)
  ├─ 5-6 structures → 6-8 templates (larger site)
  └─ 7+ structures → Question: Can any be consolidated? Re-evaluate

Rule of Thumb: Most sites need fewer templates than they think
```

### "Should We Embed or Reference Related Content?"

**Embed (Show inside current page):**
- Pros: Users see all info without navigating away
- Cons: Page becomes longer, more complex, slower load time

**Reference (Link to related page):**
- Pros: Shorter pages, better focus, users control navigation
- Cons: Extra click for users

Decision: Use embedding for quick info (3-5 items), reference for extensive content.

---

## Planning Checklist

Before moving to Phase 2, verify you have:

- [ ] **Content Categories Defined**
  - List of 3-6 main content types for your site
  
- [ ] **User Journeys Mapped**
  - 2-3 primary paths visitors will take
  
- [ ] **Layouts Planned**
  - Decision on 1, 2, or 3 layouts with names and purposes
  - Simple wireframe for each layout
  
- [ ] **Templates Listed**
  - Content type → Template mapping
  - Key sections for each template
  - Consolidation decisions made
  
- [ ] **Site Structure Outline**
  - Visual hierarchy of layouts → templates → pages
  - Navigation strategy documented
  
- [ ] **Design Guidelines Started**
  - Preliminary typography, color, spacing decisions
  - Any brand standards you're following
  
- [ ] **Team Alignment**
  - Stakeholders have reviewed and approved the plan
  - No major surprises when moving to Phase 2

---

## Anti-Patterns: What NOT to Do

### ❌ Creating a Template for Every Page

**Problem:** You end up with 20+ templates, each slightly different
**Solution:** Identify common structures and consolidate (maximum 6-8 templates)

### ❌ Planning Without Understanding the Layouts → Templates → Pages Hierarchy

**Problem:** You design content pages first, then realize your layout doesn't work
**Solution:** Always plan layouts first, then templates, then pages (top-down approach)

### ❌ Making Navigation Decisions During Phase 2-3

**Problem:** You build templates, then realize the navigation structure doesn't work
**Solution:** Decide navigation and site structure in Phase 1

### ❌ Forgetting About Mobile/Responsive Design

**Problem:** Phase 2 layout doesn't work on mobile, requires rework
**Solution:** Wireframe for both desktop and mobile during Phase 1 planning

### ❌ Over-Designing Too Early

**Problem:** Perfect mockups in Phase 1 → reality checks force major redesigns
**Solution:** Keep wireframes simple and flexible; details come in Phase 2-3

### ❌ Not Consolidating Similar Content Types

**Problem:** "Blog", "News", "Articles" end up as separate templates that are 95% identical
**Solution:** Question every template: "Can this be consolidated with another?"

---

## Frequently Asked Questions

### Q: How do I know if I need 2 or 3 layouts?

**A:** Ask: "Do I have content that needs a fundamentally different visual structure?" 
- Landing pages with hero sections? → Need 2 layouts
- Admin/internal pages that look different? → Need 3 layouts
- Everything else? → Just one layout

Start with one. Add a second only if you genuinely need it.

### Q: Should I use a grid/columns approach or containers?

**A:** Most modern sites use a flexible grid system (e.g., 12-column grid) that templates can leverage. During Phase 1, just understand that templates will need to define content areas (full-width, 2-column, 3-column, etc.). Phase 2 will implement this.

### Q: What if we're not sure about all content types yet?

**A:** That's fine. Plan for the content types you know about (typically 3-4), and leave room for expansion. SkyCMS makes it easy to add new templates later without disrupting existing ones.

### Q: How detailed should wireframes be?

**A:** For Phase 1, keep them simple—ASCII art or hand sketches are perfect. You're communicating structure, not design. Detailed design mockups come later if needed.

### Q: Do we need a style guide in Phase 1?

**A:** Not formally. Just make preliminary decisions about colors, fonts, and spacing. Create a formal style guide during Phase 4 when you have templates built and can document actual implemented styles.

### Q: Can templates be reused across different layouts?

**A:** Not directly in SkyCMS—each template must specify a layout. However, you can create similar templates for different layouts. Better approach: consolidate layouts so you don't need this.

### Q: How do we handle multi-language sites?

**A:** Phase 1 planning is the same. SkyCMS supports multi-language through middleware and tenant configuration (not covered in this guide). Plan your content structure first; language routing is a deployment concern, not an architectural one.

---

## Next Steps

Once you've completed this planning phase:

1. **Schedule a review meeting** with stakeholders to confirm layout and template decisions
2. **Document your decisions** (keep them with your project files)
3. **Proceed to Phase 2** with your site structure plan in hand

**Phase 2:** Creating Layouts - You'll take this plan and actually build the first layout structure in SkyCMS.

---

## Resources Referenced

- [Website Launch Workflow Overview](./Website-Launch-Workflow.md) - Full 6-phase workflow
- [Creating Layouts Guide](./02-Creating-Layouts.md) - Phase 2 walkthrough
- SkyCMS Architecture docs: Layouts, Templates, Pages concepts
- UX Planning Resources: User journey mapping, information architecture basics

---

**Duration Estimate:** 3-5 hours  
**Estimated Output Size:** Architecture diagram, wireframes, template list, navigation map

**When to Proceed to Phase 2:** When you have stakeholder sign-off on your layout and template plan, and your wireframes are documented (even if rough).
