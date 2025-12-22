---
name: Restore Missing Dependencies
overview: Add the missing npm packages (lucide-angular, tailwindcss, postcss, autoprefixer) to package.json that are currently being used in the codebase but were accidentally deleted.
todos:
  - id: add-lucide-angular
    content: Add lucide-angular to dependencies section in package.json
    status: completed
  - id: add-tailwind-deps
    content: Add tailwindcss, postcss, and autoprefixer to devDependencies section in package.json
    status: completed
---

# Restore Missing Dependencies in package.json

## Problem

The `frontend/package.json` is missing several dependencies that are actively used in the codebase:

- **lucide-angular**: Used extensively for icons (10+ components import it)
- **tailwindcss**: Configured in `tailwind.config.js` and used in `styles.scss`
- **postcss**: Required for PostCSS processing (referenced in `postcss.config.js`)
- **autoprefixer**: PostCSS plugin for vendor prefixes (referenced in `postcss.config.js`)

## Solution

Add the missing dependencies to [`frontend/package.json`](frontend/package.json):

### Dependencies to Add:

1. **lucide-angular** - Add to `dependencies` section (latest compatible version with Angular 19)
2. **tailwindcss** - Add to `devDependencies` section
3. **postcss** - Add to `devDependencies` section  
4. **autoprefixer** - Add to `devDependencies` section

### Implementation Steps:

1. Update `frontend/package.json` to include all missing packages with appropriate versions
2. Verify the versions are compatible with Angular 19 and the current setup
3. The packages will be installed when the user runs `npm install`

## Files to Modify:

- `frontend/package.json` - Add missing dependencies

## Notes:

- `lucide-angular` should be in `dependencies` (runtime dependency)
- `tailwindcss`, `postcss`, and `autoprefixer` should be in `devDependencies` (build-time dependencies)
- After updating package.json, run `npm install` to restore the packages