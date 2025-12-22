---
name: Fix print CSS to hide page content
overview: The print CSS is not properly hiding the entire page content. Need to use more aggressive selectors and ensure router-outlet content is completely hidden before showing the thermal receipt.
todos: []
---

# Fix Print CSS to Hide Entire Page Content

## Problem
The current CSS isn't hiding the router-outlet content (order creation page, menu items, etc.) when printing. The entire page appears in print preview instead of just the thermal receipt.

## Root Cause
1. Router-outlet content is dynamically inserted and may not be caught by current selectors
2. Need to hide everything at the body/html level first, then show only thermal receipt
3. CSS specificity may not be high enough to override other styles

## Solution

### 1. Use More Aggressive Hiding Strategy
**File**: `frontend/src/app/shared/components/thermal-receipt/thermal-receipt.component.scss`

- Hide ALL elements using `* { display: none }` first
- Then selectively show only `.thermal-receipt` and its children
- Ensure router-outlet and all Angular components are hidden
- Hide main-layout component if present in route

### 2. Add Higher Specificity Rules
- Use `!important` on all hiding rules
- Target common Angular selectors: `router-outlet`, `app-*` components
- Hide the main content area that contains the route

### 3. Verify DOM Structure
- Check if routes render inside a layout component (main-layout)
- Hide the layout wrapper if it exists
- Ensure thermal receipt is a direct child of app-root (which it is)

## Implementation Details

**CSS Approach**:
1. `* { display: none !important; }` - Hide everything
2. `.thermal-receipt, .thermal-receipt * { display: block !important; visibility: visible !important; }` - Show only receipt
3. Specific rules for router-outlet, app-toast-container, and any layout wrappers