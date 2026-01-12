# SkyCMS Editor Manual Test Plans (Azure DevOps)

This folder contains manual test artifacts for Azure DevOps Test Plans to validate SkyCMS Editor functionality across releases.

## Structure
- `SkyCMS-Editor-v1.0.1-TestCases.csv` — importable CSV of manual test cases
- Future versions: `SkyCMS-Editor-vX.Y.Z-TestCases.csv`
- Optional: suite-specific CSVs (e.g., `Auth.csv`, `Publishing.csv`) if you prefer modular imports

## How to Use (Azure DevOps Test Plans)
1. Create a Test Plan: "SkyCMS Editor v1.0.1"
2. Create Test Suites aligned to feature areas (Auth, Editing, Media, Publishing, Versioning, Scheduling, SEO, Storage, Admin)
3. Import the CSV into the plan (Test Plans → Import → CSV)
4. Assign testers and execute using the Test Runner
5. Log bugs directly from failed steps; link to the corresponding test case

## Conventions
- Cases use concise titles, explicit steps, and clear expected results
- Priority: 1 (critical), 2 (high), 3 (normal)
- Keep steps action-oriented; avoid ambiguous phrasing
- Map cases to features/areas using suite names and tags

## Prerequisites
- A deployed SkyCMS stack (Editor URL available)
- Admin and Editor test accounts
- Sample content (template, assets) prepared for testing

## Next
- Fill `SkyCMS-Editor-v1.0.1-TestCases.csv` with detailed cases
- Add non-functional suites later (performance, security, accessibility)
