---
title: Phase 2 - Creating Layouts
description: Site-wide structure with DRY principles, HTML structure, responsive design, and common patterns
keywords: layouts, DRY, HTML, CSS, responsive, header, footer, navigation, phase-2
audience: [developers, designers]
---

# Phase 2: Creating Layouts Guide

**Timeline:** 4-8 hours  
**Typical Effort:** Building 1-2 Site Designs with responsive markup  
**Output:** Complete Site Design with header, navigation, content area, footer

## Overview

In Phase 1, you planned your site structure and decided how many Layouts you needed. Phase 2 is where you build those Layouts (referred to as "Site Designs" in the SkyCMS editor UI) in SkyCMS.

In the context of SkyCMS, a **Layout** (or Site Design) is the structural container that defines:
- Overall page structure (where content goes)
- Header and footer areas
- Navigation placement
- Responsive breakpoints
- Global HTML structure

This guide walks you through creating your first Site Design, adding variations, and ensuring mobile responsiveness.

---

## Why Layouts Matter: The DRY Principle

One of the most important benefits of using Layouts (Site Designs) is that they help you **stay DRY** — **Don't Repeat Yourself**.

### Without Layouts: Code Repetition Problem

Imagine building a website with 50 pages without layouts:

```html
<!-- Page 1: home.html -->
<!DOCTYPE html>
<html>
<head>...</head>
<body>
  <header>...same header code...</header>
  <nav>...same navigation code...</nav>
  <main>
    <!-- Unique home page content -->
  </main>
  <footer>...same footer code...</footer>
</body>
</html>

<!-- Page 2: about.html -->
<!DOCTYPE html>
<html>
<head>...</head>
<body>
  <header>...same header code...</header>
  <nav>...same navigation code...</nav>
  <main>
    <!-- Unique about page content -->
  </main>
  <footer>...same footer code...</footer>
</body>
</html>

<!-- ... 48 more pages with identical header/nav/footer -->
```

**The Problem:**
- Header/nav/footer code **repeated 50 times**
- Need to update logo? **Edit 50 files**
- Fix footer bug? **Edit 50 files**
- Add menu item? **Edit 50 files**
- High risk of inconsistency and errors
- Massive maintenance burden

### With Layouts: DRY Approach

Using a Layout, you write shared code **once**:

```html
<!-- Layout (Site Design) - Written ONCE -->
<!DOCTYPE html>
<html>
<head>...</head>
<body>
  <header>...header code...</header>
  <nav>...navigation code...</nav>
  <main>
    @RenderBody()  <!-- Pages insert their unique content here -->
  </main>
  <footer>...footer code...</footer>
</body>
</html>

<!-- Page 1: Only unique content -->
<h1>Welcome Home</h1>
<p>Home page content...</p>

<!-- Page 2: Only unique content -->
<h1>About Us</h1>
<p>About page content...</p>

<!-- ... 48 more pages with ONLY their unique content -->
```

**The Benefits:**
- Header/nav/footer written **once** in the Layout
- Update logo? **Edit 1 file** (the Layout)
- Fix footer bug? **Edit 1 file** (automatically applied to all 50 pages)
- Add menu item? **Edit 1 file** (appears on all pages instantly)
- **Zero inconsistency risk** — all pages use identical structure
- **Minimal maintenance** — changes propagate automatically
- **Faster development** — focus on unique content, not repeated boilerplate

### Real-World Impact

Consider a typical website project:

**Without Layouts (Repetition):**
- 50 pages × 150 lines of repeated code = **7,500 lines of duplicate code**
- Logo update: 50 file edits × 2 minutes = **100 minutes**
- Navigation change: 50 file edits × 3 minutes = **150 minutes**
- Risk: Miss 3 pages → **broken consistency**

**With Layouts (DRY):**
- 1 Layout × 150 lines = **150 lines** (shared code)
- 50 pages × 30 lines = **1,500 lines** (unique content only)
- Logo update: **1 file edit × 2 minutes = 2 minutes**
- Navigation change: **1 file edit × 3 minutes = 3 minutes**
- Risk: **Zero** — one source of truth

**Savings:** 5,850 fewer lines of code, 245 minutes saved on simple updates

### Staying DRY Throughout Development

The DRY principle extends beyond just layouts:

**Good DRY Practices:**
```html
<!-- Layout: Define reusable CSS variables once -->
<style>
  :root {
    --primary-color: #0066cc;
    --font-main: Arial, sans-serif;
  }
</style>

<!-- Use variables everywhere (not repeated values) -->
<button style="background: var(--primary-color);">Click</button>

<!-- Layout: Define common classes once -->
<style>
  .btn-primary { background: var(--primary-color); padding: 10px 20px; }
</style>

<!-- Reuse classes (don't repeat styles) -->
<button class="btn-primary">Click</button>
```

**Avoid Repetition:**
```html
<!-- Bad: Repeated inline styles -->
<button style="background: #0066cc; padding: 10px 20px;">Click</button>
<button style="background: #0066cc; padding: 10px 20px;">Submit</button>
<button style="background: #0066cc; padding: 10px 20px;">Go</button>

<!-- Good: Define once, reuse -->
<style>.btn { background: #0066cc; padding: 10px 20px; }</style>
<button class="btn">Click</button>
<button class="btn">Submit</button>
<button class="btn">Go</button>
```

### The Bottom Line

> Layouts are essential for staying DRY. By centralizing shared markup, styles, and scripts in your Site Design, you write less code, maintain it more easily, and ensure perfect consistency across your entire website.

This is not just a convenience — it's a **fundamental best practice** in professional web development.

---

## Important: You Have Complete Design Freedom

Before we dive into step-by-step instructions, let's be clear about what you can do:

### Framework Flexibility

SkyCMS comes with **Bootstrap and Tailwind CSS frameworks pre-configured** as options, but they are:
- **Completely optional** — use them if they suit your project
- **Not required** — you can write 100% custom CSS
- **Not limiting** — you can use a completely different framework if you prefer

### Your Options

**Option 1: Use Built-in Frameworks (Recommended for Speed)**
- Bootstrap 5 is available and configured
- Tailwind CSS is available and configured
- Choose whichever aligns with your design preferences
- Fastest path to a responsive Site Design

**Option 2: Custom CSS Only**
- Write your own CSS from scratch
- Use your own preferred methodology (BEM, Atomic, etc.)
- Complete control over styling and behavior
- Takes more time but results in minimal code

**Option 3: Different Framework**
- Want Material Design? Use it.
- Want Foundation? Use it.
- Want CSS Grid with custom layouts? Go for it.
- SkyCMS doesn't dictate your tech stack

**Option 4: Minimal Layout or No Layout**
- If you want each page to have its own unique design
- If you're using a per-page theme or CSS framework
- You can create a **minimal layout** (just the `<html>`, `<body>`, and `@RenderBody()`)
- Or define **no layout at all** for each page that needs complete independence
- This gives you maximum flexibility for highly customized designs

### The Key Point

> Your Site Design reflects your design needs and technical preferences. SkyCMS provides the structure; you provide the creativity.

We'll show examples using Bootstrap and Tailwind because they're popular and pre-configured, but every technique shown can be adapted to any framework or custom CSS.

---

## Core Concepts: How Layouts Work in SkyCMS

Before creating your first Site Design, understand how it functions within SkyCMS:

### The Layout File Structure

A Layout in SkyCMS is a Razor template that defines:
1. The overall HTML document structure
2. Where templates will render their content (`@RenderBody()`)
3. Common elements (header, navigation, footer)
4. Required stylesheets and scripts
5. Responsive behavior

### Basic Layout Template

```html
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>@ViewBag.Title</title>
    
    <!-- Bootstrap CSS (if using Bootstrap) -->
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet">
    
    <!-- Your custom CSS -->
    <link href="/css/site.css" rel="stylesheet">
</head>
<body>
    <!-- HEADER -->
    <header class="site-header">
        <div class="container">
            <div class="logo">
                <h1>Site Logo/Name</h1>
            </div>
        </div>
    </header>

    <!-- NAVIGATION -->
    <nav class="navbar navbar-expand-lg navbar-light bg-light">
        <div class="container">
            <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarNav">
                <span class="navbar-toggler-icon"></span>
            </button>
            <div class="collapse navbar-collapse" id="navbarNav">
                <ul class="navbar-nav ms-auto">
                    <li class="nav-item"><a class="nav-link" href="/">Home</a></li>
                    <li class="nav-item"><a class="nav-link" href="/about">About</a></li>
                    <li class="nav-item"><a class="nav-link" href="/contact">Contact</a></li>
                </ul>
            </div>
        </div>
    </nav>

    <!-- MAIN CONTENT -->
    <main class="main-content">
        <div class="container">
            @RenderBody()
        </div>
    </main>

    <!-- FOOTER -->
    <footer class="site-footer bg-dark text-white">
        <div class="container">
            <p>&copy; 2025 Your Company. All rights reserved.</p>
        </div>
    </footer>

    <!-- Bootstrap JS -->
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
</body>
</html>
```

The key line is `@RenderBody()` — this is where template content renders.

---

## Decision Framework: Choosing Your Design Approach

Before building, decide which design approach fits your project:

### Decision Tree

```
START: How should your site look?

├─ All pages look similar with consistent header/nav/footer
│  └─ Do you want to use an existing framework?
│     ├─ YES, Bootstrap → Follow "Bootstrap Approach" below
│     ├─ YES, Tailwind → Follow "Tailwind Approach" below
│     └─ NO, custom CSS → Follow "Custom CSS Approach" below
│
├─ Pages have varying designs but share header/nav/footer
│  └─ Use minimal layout with per-page CSS/frameworks
│     See "Minimal Layout Approach" below
│
└─ Each page has completely unique design (no shared layout)
   └─ Use no layout (each page is independent)
      See "Independent Page Design" below
```

### Approach 1: Bootstrap Framework (Recommended for Speed)

**When to use:** You want responsive design fast, don't need heavily customized styling, prefer CSS classes over custom CSS.

**Pros:**
- Pre-built responsive components (navbar, grid, buttons, forms)
- Extensive documentation and community support
- Quick to implement
- Accessible by default

**Cons:**
- Requires learning Bootstrap class naming
- Default design looks "Bootstrap-like"
- Need customization via CSS overrides for unique branding

**Timeline:** 3-4 hours for a complete Site Design

### Approach 2: Tailwind CSS Framework

**When to use:** You want a modern utility-first approach, prefer building with classes, want a lightweight framework.

**Pros:**
- Highly customizable via configuration
- Utility-first approach (compose designs from small classes)
- Minimal output CSS when purged
- Great documentation
- Modern and popular

**Cons:**
- Steeper learning curve if unfamiliar with utility-first
- Requires understanding composition vs components
- Build process needed for custom configuration

**Timeline:** 4-5 hours for a complete Site Design (includes setup)

### Approach 3: Custom CSS Only

**When to use:** You have specific design needs, want maximum control, prefer minimal dependencies.

**Pros:**
- Complete control over every pixel
- No framework constraints
- Lighter payload (only your CSS)
- Full creative freedom

**Cons:**
- More CSS to write
- Responsive design is your responsibility
- No pre-built components

**Timeline:** 6-8 hours for a complete responsive Site Design

### Approach 4: Minimal Layout + Per-Page Flexibility

**When to use:** Some pages need shared elements (header/nav), others need completely custom designs.

**Pros:**
- Flexibility to use different frameworks per page
- Shared navigation doesn't repeat
- Pages can have unique styling

**Cons:**
- Multiple design systems across site
- Harder to maintain consistency
- More complexity

**Timeline:** 4-6 hours plus additional time per page

### Approach 5: No Layout (Complete Page Independence)

**When to use:** Each page needs a completely unique design, or you're using per-page theme systems.

**Pros:**
- Maximum flexibility
- Each page can be optimized independently
- Use whatever framework/CSS makes sense per page

**Cons:**
- No automatic shared elements
- Harder to maintain consistency
- Content creators must understand layout independence

**Timeline:** Varies greatly depending on per-page complexity

---

## Step-by-Step: Creating Your First Site Design

Let's walk through creating a standard Site Design using Bootstrap (the quickest approach).

### Step 1: Access the Layout Manager

In SkyCMS Admin:
1. Navigate to **Content Management** → **Layouts**
2. Click **Create New Layout** button
3. You'll see a form with these fields:
   - **Layout Name:** How you'll identify this in templates
   - **Layout Code:** The Razor template HTML/markup
   - **Description:** Notes about when to use this layout

### Step 2: Name Your Site Design

Choose a clear, descriptive name:

**Good Names:**
- "Main Site Design" (primary layout)
- "Landing Page Design" (hero-focused)
- "Admin Design" (for internal pages)
- "Blog Design" (if blog pages differ from main)

**Poor Names:**
- "Layout" (too generic)
- "Template" (confusing with templates in Phase 3)
- "Site1" (not descriptive)

**Our Example:** "Main Site Design"

### Step 3: Write the Markup

Start with a basic structure and build from there. Here's a Bootstrap example:

```html
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>@ViewBag.Title - Your Site Name</title>
    
    <!-- Bootstrap CSS from CDN -->
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet">
    
    <!-- Your custom styles -->
    <style>
        :root {
            --primary-color: #0066cc;
            --secondary-color: #6c757d;
        }
        
        body {
            font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif;
            line-height: 1.6;
            color: #333;
        }
        
        .site-header {
            background-color: #fff;
            border-bottom: 1px solid #ddd;
            padding: 1rem 0;
        }
        
        .site-header .logo h1 {
            margin: 0;
            font-size: 1.5rem;
            color: var(--primary-color);
        }
        
        .main-content {
            min-height: calc(100vh - 300px);
            padding: 2rem 0;
        }
        
        .site-footer {
            background-color: #333;
            color: #fff;
            padding: 2rem 0;
            margin-top: 3rem;
        }
        
        @media (max-width: 768px) {
            .site-header .logo h1 {
                font-size: 1.25rem;
            }
        }
    </style>
</head>
<body>
    <!-- HEADER -->
    <header class="site-header">
        <div class="container">
            <div class="logo">
                <h1>Your Company</h1>
            </div>
        </div>
    </header>

    <!-- NAVIGATION -->
    <nav class="navbar navbar-expand-lg navbar-light bg-light sticky-top">
        <div class="container">
            <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarNav" aria-controls="navbarNav" aria-expanded="false" aria-label="Toggle navigation">
                <span class="navbar-toggler-icon"></span>
            </button>
            <div class="collapse navbar-collapse" id="navbarNav">
                <ul class="navbar-nav ms-auto">
                    <li class="nav-item">
                        <a class="nav-link" href="/">Home</a>
                    </li>
                    <li class="nav-item">
                        <a class="nav-link" href="/about">About</a>
                    </li>
                    <li class="nav-item">
                        <a class="nav-link" href="/services">Services</a>
                    </li>
                    <li class="nav-item">
                        <a class="nav-link" href="/blog">Blog</a>
                    </li>
                    <li class="nav-item">
                        <a class="nav-link" href="/contact">Contact</a>
                    </li>
                </ul>
            </div>
        </div>
    </nav>

    <!-- MAIN CONTENT AREA -->
    <main class="main-content">
        <div class="container">
            @RenderBody()
        </div>
    </main>

    <!-- FOOTER -->
    <footer class="site-footer">
        <div class="container">
            <div class="row">
                <div class="col-md-4">
                    <h5>About</h5>
                    <p>Brief company description here.</p>
                </div>
                <div class="col-md-4">
                    <h5>Links</h5>
                    <ul class="list-unstyled">
                        <li><a href="/privacy" class="text-white-50">Privacy Policy</a></li>
                        <li><a href="/terms" class="text-white-50">Terms of Service</a></li>
                    </ul>
                </div>
                <div class="col-md-4">
                    <h5>Contact</h5>
                    <p>Email: <a href="mailto:info@company.com" class="text-white-50">info@company.com</a></p>
                </div>
            </div>
            <hr class="text-white-50">
            <p class="text-center text-white-50 mb-0">&copy; 2025 Your Company. All rights reserved.</p>
        </div>
    </footer>

    <!-- Bootstrap JS Bundle (includes Popper) -->
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
</body>
</html>
```

### Step 4: Test on Different Screen Sizes

Before saving:
1. Open browser DevTools (F12)
2. Toggle device toolbar (Ctrl+Shift+M or Cmd+Shift+M)
3. Test at multiple breakpoints:
   - Mobile: 375px, 480px
   - Tablet: 768px, 1024px
   - Desktop: 1440px, 1920px

Verify:
- Header and navigation are readable
- Main content area is accessible
- Footer displays correctly
- All text is visible (no overflow)

### Step 5: Save Your Site Design

Click **Save Layout** in SkyCMS. The system will:
1. Validate the Razor syntax
2. Store the markup
3. Make it available for templates to use

### Step 6: Create Variations (Optional)

Once you have a main Site Design, you might create variations:

**Example Variations:**
- Main Site Design (header + nav + footer)
- Landing Page Design (no nav, hero focused)
- Blog Design (with sidebar)

**To create a variation:**
1. Create new layout
2. Use similar structure but modify sections as needed
3. Name clearly (e.g., "Landing Page Design")

---

## Real-World Example: E-Commerce Site Design

Here's a complete, production-ready Site Design for an e-commerce site:

```html
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>@ViewBag.Title - E-Store</title>
    
    <!-- Bootstrap CSS -->
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet">
    
    <!-- Font Awesome for icons -->
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css">
    
    <style>
        :root {
            --primary: #0066cc;
            --secondary: #666;
            --light-bg: #f8f9fa;
            --border-color: #dee2e6;
        }
        
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }
        
        body {
            font-family: "Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif;
            color: #333;
            background-color: #fff;
        }
        
        /* Header */
        .site-header {
            background-color: #fff;
            border-bottom: 1px solid var(--border-color);
            padding: 1rem 0;
        }
        
        .site-header .logo {
            font-size: 1.5rem;
            font-weight: bold;
            color: var(--primary);
        }
        
        .site-header .header-icons {
            display: flex;
            gap: 1rem;
            align-items: center;
        }
        
        .site-header .cart-icon {
            position: relative;
        }
        
        .site-header .cart-count {
            position: absolute;
            top: -8px;
            right: -8px;
            background-color: #dc3545;
            color: white;
            border-radius: 50%;
            width: 20px;
            height: 20px;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 0.75rem;
        }
        
        /* Navigation */
        .navbar {
            background-color: var(--light-bg) !important;
            box-shadow: 0 2px 4px rgba(0,0,0,0.05);
        }
        
        .navbar-nav .nav-link {
            color: #333 !important;
            font-weight: 500;
            margin-left: 1rem;
        }
        
        .navbar-nav .nav-link:hover {
            color: var(--primary) !important;
        }
        
        .navbar-nav .nav-link.active {
            color: var(--primary) !important;
            border-bottom: 2px solid var(--primary);
        }
        
        /* Main Content */
        .main-content {
            min-height: calc(100vh - 400px);
            padding: 2rem 0;
        }
        
        /* Footer */
        .site-footer {
            background-color: #333;
            color: #fff;
            padding: 3rem 0 1rem;
            margin-top: 4rem;
        }
        
        .site-footer h5 {
            color: var(--primary);
            margin-bottom: 1rem;
            font-weight: 600;
        }
        
        .site-footer a {
            color: #bbb;
            text-decoration: none;
        }
        
        .site-footer a:hover {
            color: var(--primary);
        }
        
        .footer-divider {
            border-top: 1px solid #555;
            margin: 2rem 0;
        }
        
        .footer-bottom {
            text-align: center;
            padding-top: 1rem;
            border-top: 1px solid #555;
            font-size: 0.9rem;
            color: #999;
        }
        
        /* Responsive */
        @media (max-width: 768px) {
            .site-header .header-icons {
                gap: 0.5rem;
            }
            
            .navbar-nav .nav-link {
                margin-left: 0;
                padding: 0.5rem 0;
            }
            
            .site-footer {
                padding: 2rem 0 1rem;
            }
        }
    </style>
</head>
<body>
    <!-- HEADER -->
    <header class="site-header">
        <div class="container">
            <div class="d-flex justify-content-between align-items-center">
                <div class="logo">E-Store</div>
                <div class="header-icons">
                    <div class="search-box" style="flex: 1; margin: 0 2rem;">
                        <input type="search" class="form-control form-control-sm" placeholder="Search products...">
                    </div>
                    <div class="cart-icon">
                        <i class="fas fa-shopping-cart fa-lg"></i>
                        <span class="cart-count">3</span>
                    </div>
                    <div class="account-icon">
                        <i class="fas fa-user fa-lg"></i>
                    </div>
                </div>
            </div>
        </div>
    </header>

    <!-- NAVIGATION -->
    <nav class="navbar navbar-expand-lg sticky-top">
        <div class="container">
            <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarNav">
                <span class="navbar-toggler-icon"></span>
            </button>
            <div class="collapse navbar-collapse" id="navbarNav">
                <ul class="navbar-nav ms-auto">
                    <li class="nav-item">
                        <a class="nav-link active" href="/">Home</a>
                    </li>
                    <li class="nav-item dropdown">
                        <a class="nav-link dropdown-toggle" href="#" id="categoriesDropdown" role="button" data-bs-toggle="dropdown">
                            Categories
                        </a>
                        <ul class="dropdown-menu" aria-labelledby="categoriesDropdown">
                            <li><a class="dropdown-item" href="/category/electronics">Electronics</a></li>
                            <li><a class="dropdown-item" href="/category/clothing">Clothing</a></li>
                            <li><a class="dropdown-item" href="/category/books">Books</a></li>
                        </ul>
                    </li>
                    <li class="nav-item">
                        <a class="nav-link" href="/deals">Deals</a>
                    </li>
                    <li class="nav-item">
                        <a class="nav-link" href="/about">About</a>
                    </li>
                    <li class="nav-item">
                        <a class="nav-link" href="/contact">Contact</a>
                    </li>
                </ul>
            </div>
        </div>
    </nav>

    <!-- MAIN CONTENT AREA -->
    <main class="main-content">
        <div class="container">
            @RenderBody()
        </div>
    </main>

    <!-- FOOTER -->
    <footer class="site-footer">
        <div class="container">
            <div class="row">
                <div class="col-md-3">
                    <h5>Shop</h5>
                    <ul class="list-unstyled">
                        <li><a href="/category/electronics">Electronics</a></li>
                        <li><a href="/category/clothing">Clothing</a></li>
                        <li><a href="/category/books">Books</a></li>
                        <li><a href="/deals">All Deals</a></li>
                    </ul>
                </div>
                <div class="col-md-3">
                    <h5>Customer Service</h5>
                    <ul class="list-unstyled">
                        <li><a href="/contact">Contact Us</a></li>
                        <li><a href="/faq">FAQ</a></li>
                        <li><a href="/shipping">Shipping Info</a></li>
                        <li><a href="/returns">Returns</a></li>
                    </ul>
                </div>
                <div class="col-md-3">
                    <h5>Company</h5>
                    <ul class="list-unstyled">
                        <li><a href="/about">About Us</a></li>
                        <li><a href="/careers">Careers</a></li>
                        <li><a href="/press">Press</a></li>
                        <li><a href="/blog">Blog</a></li>
                    </ul>
                </div>
                <div class="col-md-3">
                    <h5>Connect</h5>
                    <div style="display: flex; gap: 1rem;">
                        <a href="#"><i class="fab fa-facebook fa-lg"></i></a>
                        <a href="#"><i class="fab fa-twitter fa-lg"></i></a>
                        <a href="#"><i class="fab fa-instagram fa-lg"></i></a>
                        <a href="#"><i class="fab fa-linkedin fa-lg"></i></a>
                    </div>
                </div>
            </div>
            <div class="footer-divider"></div>
            <div class="footer-bottom">
                <p>&copy; 2025 E-Store. All rights reserved. | <a href="/privacy">Privacy</a> | <a href="/terms">Terms</a> | <a href="/cookies">Cookie Policy</a></p>
            </div>
        </div>
    </footer>

    <!-- Bootstrap JS -->
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
</body>
</html>
```

---

## Best Practices for Site Designs

### 1. Always Include Viewport Meta Tag

```html
<meta name="viewport" content="width=device-width, initial-scale=1.0">
```

This ensures proper rendering on mobile devices.

### 2. Use Semantic HTML

```html
<!-- Good -->
<header>...</header>
<nav>...</nav>
<main>...</main>
<footer>...</footer>

<!-- Avoid -->
<div class="header">...</div>
<div class="nav">...</div>
<div class="main">...</div>
<div class="footer">...</div>
```

### 3. Keep Layouts Focused

A Site Design should define:
- ✅ Structure (header, nav, footer)
- ✅ Responsive breakpoints
- ✅ Global styles (typography, spacing)

A Site Design should NOT:
- ❌ Define page-specific content (that's for templates)
- ❌ Include too much decorative code
- ❌ Have hard-coded menu items (use dynamic navigation instead)

### 4. Use CSS Classes for Flexibility

```html
<!-- Good: Use classes for styling -->
<div class="site-header">
    <div class="container">
        <h1 class="site-title">My Site</h1>
    </div>
</div>

<!-- Avoid: Inline styles -->
<div style="background-color: blue; padding: 20px;">
    <h1 style="color: white; font-size: 24px;">My Site</h1>
</div>
```

### 5. Test Responsive Behavior

Before publishing, verify:
- [ ] Mobile (320px, 480px)
- [ ] Tablet (768px)
- [ ] Desktop (1024px+)
- [ ] Large displays (1920px)

### 6. Optimize Performance

```html
<!-- Minimize render-blocking resources -->
<link rel="stylesheet" href="critical.css">

<!-- Defer non-critical JS -->
<script defer src="analytics.js"></script>

<!-- Use modern image formats -->
<img src="logo.webp" alt="Logo" loading="lazy">
```

### 7. Consider Accessibility

```html
<!-- Use semantic elements -->
<button class="hamburger" aria-label="Toggle navigation">
    <span class="hamburger-icon"></span>
</button>

<!-- Provide alt text -->
<img src="banner.jpg" alt="Welcome banner with company logo">

<!-- Use proper heading hierarchy -->
<h1>Page Title</h1>
<h2>Section</h2>
<h3>Subsection</h3>
```

---

## When to Modify vs. Create New

### When to Modify Existing Site Design

You should edit an existing Site Design when:
- ✅ Updating colors (maintain consistency)
- ✅ Adjusting spacing or sizing
- ✅ Adding minor CSS classes
- ✅ Fixing responsive issues

### When to Create a New Site Design

You should create a new Site Design when:
- ✅ Pages need fundamentally different structure
- ✅ A section needs different navigation or footer
- ✅ Landing pages have a different visual approach
- ✅ Admin pages look completely different

---

## Advanced: Custom CSS with Minimal Layout

If you prefer complete design control, create a minimal Site Design that pages can customize:

```html
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>@ViewBag.Title</title>
</head>
<body>
    @RenderBody()
    
    <!-- Optional: Global footer script or analytics -->
    <script src="/js/analytics.js"></script>
</body>
</html>
```

Then each template or page can include its own CSS framework, custom styles, and unique design. This gives maximum flexibility but requires more coordination.

---

## Tips and Best Practices for Site Designs

### Philosophy: Simplicity and Flexibility

**SkyCMS is designed to place as few restrictions as possible on how web developers implement the UI and framework.** You have complete control over your technology choices, design approach, and implementation strategy. The system provides structure without limiting creativity or technical decisions.

**Keep things as simple as possible.** Simpler Site Designs:
- Load faster (better performance)
- Are easier to maintain and update
- Have fewer potential points of failure
- Make debugging easier
- Reduce complexity for team members

Start minimal and add complexity only when necessary.

---

### Performance Optimization

#### Use CDNs for Common Libraries

**Recommendation:** Use Content Delivery Networks (CDNs) for popular CSS and JavaScript libraries.

**Why CDNs improve performance:**
```html
<!-- Good: Use CDN for popular libraries -->
<link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet">
<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
```

**Benefits:**
1. **Browser caching** — Users likely already have these files cached from other sites
2. **Reduced server load** — CDN handles delivery, not your server
3. **Geographic distribution** — CDN serves files from nearest location
4. **High availability** — CDN uptime typically exceeds individual servers
5. **Bandwidth savings** — Less traffic from your hosting

**When NOT to use CDNs:**

❌ **Version control concerns**
- You need to lock specific library versions
- You can't tolerate unexpected updates or changes
- Your deployment process requires fixed dependencies

❌ **Privacy and data sovereignty**
- You need to keep all web traffic private
- Regulatory requirements prevent external requests
- Corporate policies prohibit third-party resources

❌ **Custom libraries**
- You've modified the library code
- You have proprietary or internal-only libraries
- You need libraries not available on CDNs

❌ **Offline/intranet applications**
- App must work without internet access
- Internal-only network with no external access

**In these cases, self-host your CSS/JS libraries:**
```html
<!-- Self-hosted alternative -->
<link href="/css/vendor/bootstrap.min.css" rel="stylesheet">
<script src="/js/vendor/bootstrap.bundle.min.js"></script>
```

#### Minify CSS and JavaScript for Production

Always minify resources before deployment:

```html
<!-- Development: Use readable versions -->
<link href="/css/site.css" rel="stylesheet">

<!-- Production: Use minified versions -->
<link href="/css/site.min.css" rel="stylesheet">
```

**Benefits:**
- Reduces file size by 30-70%
- Faster downloads and page loads
- Less bandwidth consumption

**Tools:**
- CSS: cssnano, clean-css, UglifyCSS
- JavaScript: UglifyJS, Terser, esbuild

#### Implement Lazy Loading

Load images and resources only when needed:

```html
<!-- Lazy load images -->
<img src="logo.jpg" alt="Logo" loading="lazy">

<!-- Defer non-critical JavaScript -->
<script defer src="/js/analytics.js"></script>
<script defer src="/js/comments.js"></script>
```

**What to lazy load:**
- Below-the-fold images
- Non-critical JavaScript
- Social media widgets
- Ads and tracking scripts

**What NOT to lazy load:**
- Above-the-fold images
- Critical CSS
- Core functionality scripts

#### Use Critical CSS

Inline critical above-the-fold CSS to eliminate render-blocking:

```html
<head>
    <!-- Inline critical CSS -->
    <style>
        /* Critical styles for above-the-fold content */
        body { margin: 0; font-family: sans-serif; }
        .header { background: #fff; padding: 1rem; }
        .nav { display: flex; gap: 1rem; }
    </style>
    
    <!-- Load full CSS asynchronously -->
    <link rel="preload" href="/css/site.css" as="style" onload="this.onload=null;this.rel='stylesheet'">
    <noscript><link rel="stylesheet" href="/css/site.css"></noscript>
</head>
```

**Benefits:**
- Faster initial render
- Improved perceived performance
- Better Core Web Vitals scores

**Tools:**
- Critical (npm package)
- Penthouse
- Critters (Next.js built-in)

#### Optimize Images

```html
<!-- Use modern image formats -->
<picture>
  <source srcset="hero.webp" type="image/webp">
  <source srcset="hero.jpg" type="image/jpeg">
  <img src="hero.jpg" alt="Hero image" loading="lazy">
</picture>

<!-- Responsive images -->
<img srcset="logo-small.jpg 480w, logo-medium.jpg 800w, logo-large.jpg 1200w"
     sizes="(max-width: 600px) 480px, (max-width: 1000px) 800px, 1200px"
     src="logo-medium.jpg" alt="Logo">
```

**Image optimization tips:**
- Use WebP format (70-80% smaller than JPEG)
- Implement responsive images with `srcset`
- Compress images before upload
- Set appropriate dimensions (don't serve 4K images for 400px display)

#### Reduce HTTP Requests

```html
<!-- Bad: Multiple separate files -->
<link href="/css/reset.css" rel="stylesheet">
<link href="/css/layout.css" rel="stylesheet">
<link href="/css/typography.css" rel="stylesheet">
<link href="/css/components.css" rel="stylesheet">

<!-- Good: Combined file -->
<link href="/css/site.css" rel="stylesheet">
```

**Strategies:**
- Combine CSS files into one
- Combine JavaScript files into one
- Use CSS sprites for icons
- Inline small SVGs instead of external files

#### Preload and Prefetch Resources

```html
<head>
    <!-- Preload critical resources -->
    <link rel="preload" href="/fonts/main.woff2" as="font" type="font/woff2" crossorigin>
    <link rel="preload" href="/css/critical.css" as="style">
    
    <!-- Prefetch resources for next page -->
    <link rel="prefetch" href="/about">
    <link rel="prefetch" href="/images/about-hero.jpg">
    
    <!-- DNS prefetch for external resources -->
    <link rel="dns-prefetch" href="https://fonts.googleapis.com">
</head>
```

---

### Maintainability Best Practices

#### Use CSS Custom Properties (Variables)

Makes theming and updates much easier:

```css
:root {
    /* Colors */
    --primary-color: #0066cc;
    --secondary-color: #6c757d;
    --text-color: #333;
    --bg-color: #fff;
    
    /* Spacing */
    --spacing-sm: 0.5rem;
    --spacing-md: 1rem;
    --spacing-lg: 2rem;
    
    /* Typography */
    --font-main: -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif;
    --font-heading: Georgia, serif;
    --font-size-base: 1rem;
    --font-size-lg: 1.25rem;
}

/* Use variables throughout */
body {
    font-family: var(--font-main);
    font-size: var(--font-size-base);
    color: var(--text-color);
    background-color: var(--bg-color);
}

.btn-primary {
    background-color: var(--primary-color);
    padding: var(--spacing-md) var(--spacing-lg);
}
```

**Benefits:**
- Single source of truth for design values
- Easy theme switching (including dark mode)
- Consistent styling across site
- Quick global changes

#### Avoid Inline Styles

```html
<!-- Bad: Inline styles are hard to maintain -->
<div style="background-color: blue; padding: 20px; margin: 10px; font-size: 16px;">
    Content
</div>

<!-- Good: Use CSS classes -->
<div class="card">
    Content
</div>
```

**Why classes are better:**
- Reusable across multiple elements
- Easier to update globally
- Better performance (classes are cached)
- Separation of concerns (HTML for structure, CSS for style)

#### Comment Your Code

```html
<!-- HEADER SECTION
     Sticky header with logo and navigation
     Responsive: Hamburger menu on mobile (<768px) -->
<header class="site-header">
    <!-- Logo -->
    <div class="logo">
        <h1>Company Name</h1>
    </div>
    
    <!-- Main Navigation -->
    <nav class="main-nav">
        <!-- ... -->
    </nav>
</header>
```

```css
/* =================================
   HEADER STYLES
   ================================= */

/* Logo section - fixed height for consistency */
.logo {
    height: 60px;
    display: flex;
    align-items: center;
}

/* Mobile: Stack navigation vertically below 768px */
@media (max-width: 768px) {
    .main-nav {
        display: none; /* Hidden by default, shown via JS toggle */
    }
}
```

#### Keep Selectors Simple

```css
/* Bad: Overly specific selectors */
body div.container section.main-content article.post div.content p.text {
    font-size: 1rem;
}

/* Good: Simple, maintainable selectors */
.post-content p {
    font-size: 1rem;
}
```

#### Version Your Custom CSS/JS Files (Cache Busting)

```html
<!-- Add version query string for cache busting -->
<link href="/css/site.css?v=2.1.0" rel="stylesheet">
<script src="/js/site.js?v=2.1.0"></script>

<!-- Or use hash-based versioning (build tools) -->
<link href="/css/site.a3f5b8.css" rel="stylesheet">
<script src="/js/site.e9c2d1.js"></script>
```

**When to update version:**
- After CSS changes
- After JavaScript updates
- After bug fixes
- During deployments

---

### Accessibility Best Practices

#### Use Semantic HTML

```html
<!-- Good: Semantic elements -->
<header>
    <nav>
        <ul>
            <li><a href="/">Home</a></li>
        </ul>
    </nav>
</header>

<main>
    <article>
        <h1>Article Title</h1>
        <p>Content</p>
    </article>
</main>

<footer>
    <p>&copy; 2025 Company</p>
</footer>

<!-- Avoid: Generic div soup -->
<div class="header">
    <div class="nav">
        <div class="menu">...</div>
    </div>
</div>
```

**Benefits:**
- Better screen reader support
- Improved SEO
- Clearer code structure
- Better browser parsing

#### Add ARIA Labels Where Needed

```html
<!-- Navigation toggle button -->
<button class="hamburger" 
        aria-label="Toggle navigation menu" 
        aria-expanded="false">
    <span class="hamburger-icon"></span>
</button>

<!-- Search form -->
<form role="search">
    <label for="search">Search</label>
    <input type="search" id="search" aria-label="Search the website">
    <button type="submit" aria-label="Submit search">Search</button>
</form>

<!-- Skip to main content link -->
<a href="#main-content" class="skip-link">Skip to main content</a>
<main id="main-content">...</main>
```

#### Ensure Sufficient Color Contrast

```css
/* Bad: Low contrast (fails WCAG) */
.text {
    color: #999;
    background-color: #fff;
}

/* Good: High contrast (passes WCAG AA) */
.text {
    color: #333;
    background-color: #fff;
}

/* Tool: Use contrast checker to verify */
```

**WCAG Standards:**
- AA: 4.5:1 contrast ratio for normal text
- AA: 3:1 contrast ratio for large text
- AAA: 7:1 contrast ratio for normal text

#### Support Keyboard Navigation

```css
/* Always show focus indicators */
a:focus,
button:focus,
input:focus {
    outline: 2px solid var(--primary-color);
    outline-offset: 2px;
}

/* Never remove focus outline without replacement */
/* Bad: */
*:focus { outline: none; }

/* Good: Custom focus style */
*:focus {
    outline: 2px solid #0066cc;
}
```

---

### Security Best Practices

#### Use Subresource Integrity (SRI) for CDN Resources

```html
<!-- With SRI: Prevents tampered files from loading -->
<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js" 
        integrity="sha384-geWF76RCwLtnZ8qwWowPQNguL3RmwHVBC9FhGdlKrxdiJJigb/j/68SIy3Te4Bkz"
        crossorigin="anonymous"></script>

<!-- Without SRI: Vulnerable to CDN compromise -->
<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
```

**How to get SRI hashes:**
- Most CDNs provide them (jsdelivr, cdnjs, unpkg)
- Generate with: `openssl dgst -sha384 -binary file.js | openssl base64 -A`

#### Always Use HTTPS for External Resources

```html
<!-- Good: HTTPS only -->
<link href="https://fonts.googleapis.com/css2?family=Roboto" rel="stylesheet">

<!-- Bad: HTTP (insecure) -->
<link href="http://fonts.googleapis.com/css2?family=Roboto" rel="stylesheet">

<!-- Bad: Protocol-relative URLs (deprecated) -->
<link href="//fonts.googleapis.com/css2?family=Roboto" rel="stylesheet">
```

#### Implement Content Security Policy (CSP)

```html
<head>
    <!-- Define Content Security Policy -->
    <meta http-equiv="Content-Security-Policy" 
          content="default-src 'self'; 
                   script-src 'self' https://cdn.jsdelivr.net; 
                   style-src 'self' 'unsafe-inline' https://fonts.googleapis.com;
                   font-src 'self' https://fonts.gstatic.com;">
</head>
```

**Benefits:**
- Prevents XSS attacks
- Restricts resource loading
- Mitigates injection attacks

---

### Development Workflow Best Practices

#### Mobile-First Responsive Design

```css
/* Mobile-first: Base styles for mobile */
.container {
    width: 100%;
    padding: 1rem;
}

.grid {
    display: block; /* Stacked on mobile */
}

/* Tablet and up */
@media (min-width: 768px) {
    .container {
        max-width: 720px;
        margin: 0 auto;
    }
    
    .grid {
        display: grid;
        grid-template-columns: repeat(2, 1fr);
    }
}

/* Desktop and up */
@media (min-width: 1024px) {
    .container {
        max-width: 960px;
    }
    
    .grid {
        grid-template-columns: repeat(3, 1fr);
    }
}
```

**Why mobile-first:**
- Majority of traffic is mobile
- Easier to scale up than down
- Better performance on mobile devices
- Forces focus on essential content

#### Test Across Browsers and Devices

**Minimum testing matrix:**
- [ ] Chrome (latest)
- [ ] Firefox (latest)
- [ ] Safari (latest)
- [ ] Edge (latest)
- [ ] Mobile Safari (iOS)
- [ ] Chrome Mobile (Android)

**Screen sizes to test:**
- [ ] Mobile: 375px, 414px (iPhone)
- [ ] Tablet: 768px, 1024px (iPad)
- [ ] Desktop: 1366px, 1920px
- [ ] Large: 2560px+ (4K monitors)

**Tools:**
- Browser DevTools device emulation
- BrowserStack or LambdaTest
- Real device testing when possible

#### Consider Dark Mode Support

```css
/* Light mode (default) */
:root {
    --bg-color: #ffffff;
    --text-color: #333333;
}

/* Dark mode */
@media (prefers-color-scheme: dark) {
    :root {
        --bg-color: #1a1a1a;
        --text-color: #e0e0e0;
    }
}

/* Use variables throughout */
body {
    background-color: var(--bg-color);
    color: var(--text-color);
}
```

**Benefits:**
- Better user experience
- Reduces eye strain
- Saves battery on OLED screens
- Modern, expected feature

---

### Font Optimization

```html
<head>
    <!-- Preload fonts -->
    <link rel="preload" href="/fonts/main-regular.woff2" as="font" type="font/woff2" crossorigin>
    
    <!-- Font display strategy -->
    <style>
        @font-face {
            font-family: 'CustomFont';
            src: url('/fonts/main-regular.woff2') format('woff2');
            font-display: swap; /* Show fallback immediately, swap when loaded */
        }
    </style>
</head>
```

**Font display options:**
- `swap`: Show fallback immediately (best for most cases)
- `block`: Wait briefly for font (can cause flash)
- `fallback`: Show fallback, swap if font loads quickly
- `optional`: Let browser decide based on connection speed

**Best practices:**
- Subset fonts (include only needed characters)
- Use WOFF2 format (best compression)
- Limit number of font families (1-2 max)
- Limit font weights loaded (only what you use)

---

## Common Issues and Solutions

### Issue: Navigation items are hard-coded

**Problem:** You manually typed menu items instead of using dynamic navigation

**Solution:** Use a navigation management system or data structure that templates can loop through

### Issue: Mobile menu doesn't work

**Problem:** JavaScript for toggle button isn't loading or Bootstrap JS missing

**Solution:** 
- Verify `<script src="bootstrap.bundle.min.js"></script>` is included
- Check that data attributes are correct: `data-bs-toggle="collapse"`

### Issue: Responsive design breaks at certain widths

**Problem:** Bootstrap breakpoints don't match your wireframe

**Solution:**
- Test at Bootstrap's standard breakpoints (sm: 576px, md: 768px, lg: 992px, xl: 1200px)
- Or customize breakpoints if using Tailwind

### Issue: Footer doesn't stay at bottom on short pages

**Problem:** Content is shorter than viewport, footer floats

**Solution:**
```css
body {
    display: flex;
    flex-direction: column;
    min-height: 100vh;
}

main {
    flex: 1;
}
```

---

## Security Considerations

### Protect Against XSS (Cross-Site Scripting)

Always sanitize user input in layouts:

```html
<!-- Good: Razor automatically encodes -->
<h1>@ViewBag.Title</h1>

<!-- Bad: Unencoded HTML -->
<h1>@Html.Raw(ViewBag.Title)</h1>
```

### Validate External Resources

When including external CSS/JS:
- ✅ Use HTTPS URLs only
- ✅ Verify CDN integrity with Subresource Integrity (SRI)
- ✅ Keep libraries updated

```html
<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js" 
        integrity="sha384-..." 
        crossorigin="anonymous"></script>
```

---

## Checklist: Site Design Completion

Before moving to Phase 3, verify you have:

- [ ] **Site Design Created**
  - Name clearly describes purpose
  - Includes header, navigation, main content area, footer
  
- [ ] **Responsive Design Tested**
  - Works at 375px (mobile)
  - Works at 768px (tablet)
  - Works at 1024px+ (desktop)
  - No horizontal overflow or text cutoff
  
- [ ] **Framework Chosen**
  - Bootstrap, Tailwind, custom CSS, or other
  - All needed CSS/JS files included
  
- [ ] **Performance Verified**
  - CSS/JS resources load correctly
  - Page renders without console errors
  - Load time is acceptable
  
- [ ] **Documentation Added**
  - Site Design has clear name and description
  - Any special classes or conventions documented
  
- [ ] **Navigation Clear**
  - Main menu items are visible and clickable
  - Mobile menu works on small screens
  - Link paths are correct (or will be filled by content team)

---

## Next Steps

Once you've created your Site Design(s):

1. **Test with a template** — Phase 3 will walk you through creating templates that use your Site Design
2. **Iterate if needed** — After Phase 3, you might realize design adjustments needed; that's normal
3. **Proceed to Phase 3** — Creating Templates that use your Site Design

---

## Resources

- [Phase 1: Design & Planning Guide](./01-Design-and-Planning.md) - How you planned this
- [Phase 3: Creating Templates Guide](./03-Creating-Templates.md) - Using this Site Design in templates
- [Website Launch Workflow](./Website-Launch-Workflow.md) - Full overview

### Framework Documentation
- [Bootstrap 5 Documentation](https://getbootstrap.com/docs/5.0/)
- [Tailwind CSS Documentation](https://tailwindcss.com/docs)
- [MDN Web Docs: CSS](https://developer.mozilla.org/en-US/docs/Web/CSS)
- [MDN Web Docs: HTML](https://developer.mozilla.org/en-US/docs/Web/HTML)

---

**Duration Estimate:** 4-8 hours  
**Key Deliverable:** One or more complete Site Designs ready for Phase 3

**When to Proceed to Phase 3:** When your Site Design renders correctly, is responsive, and you're satisfied with the structure and styling.
