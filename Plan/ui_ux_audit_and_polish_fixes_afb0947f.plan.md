---
name: UI/UX Audit and Polish Fixes
overview: Comprehensive visual and functional audit identifying styling regressions, spacing inconsistencies, button state issues, responsive breakpoints, z-index conflicts, and polish items (shadows, transitions, border-radius). Provides specific Tailwind class fixes for each issue.
todos:
  - id: spacing
    content: Standardize all spacing to 8px grid system (gap-2, gap-4, gap-6) across all order components
    status: completed
  - id: buttons
    content: Fix button state consistency (disabled, loading, hover, active) with standardized classes
    status: completed
  - id: responsive
    content: "Fix responsive breakpoints for tablet (1024px) - add md: breakpoints, fix grid overflow, adjust sidebar"
    status: completed
  - id: zindex
    content: Fix z-index layering hierarchy (toasts z-[60], modals z-50, backdrops z-40)
    status: completed
  - id: polish
    content: Standardize shadows (shadow-lg cards, shadow-md buttons), border-radius (rounded-xl buttons, rounded-2xl cards), transitions (duration-200)
    status: completed
  - id: typography
    content: Fix font scaling for mobile with responsive text sizes (text-2xl md:text-3xl) and add line-height
    status: completed
---

# UI/UX Audit and Polish Fixes Plan

## Audit Results Overview

This plan addresses 47 identified issues across 6 categories: Spacing & Alignment, Button State Consistency, Responsiveness, Z-Index/Layering, Polish (Shadows/Transitions/Border-Radius), and Typography & Font Scaling.---

## Category 1: Spacing & Alignment (8px Grid System)

### Issues Found:

1. **Inconsistent gap values** - Mix of `gap-2` (8px), `gap-3` (12px), `gap-4` (16px), `gap-6` (24px)
2. **Padding inconsistencies** - Mixed use of `p-4`, `p-6`, `px-4 py-3`, `py-2.5`
3. **Margin inconsistencies** - `mb-4`, `mb-6`, `mt-4`, `mt-6` used inconsistently
4. **Space-y values** - Mix of `space-y-4`, `space-y-6` not following 8px grid

### Files to Fix:

- `frontend/src/app/features/orders/order-create/order-create.component.html`
- `frontend/src/app/features/orders/order-create/components/order-sidebar/order-sidebar.component.html`
- `frontend/src/app/features/orders/order-list/order-list.component.html`
- `frontend/src/app/features/orders/order-details/order-details.component.html`
- `frontend/src/app/features/orders/order-create/components/cart-item/cart-item.component.html`

### Standardization Rules:

- Use `gap-2` (8px) for tight spacing, `gap-4` (16px) for medium, `gap-6` (24px) for large
- Buttons: Standardize on `px-4 py-2.5` or `px-6 py-3` (multiple of 8px)
- Cards: Use `p-6` (24px) consistently, reduce to `p-4` on mobile with `md:p-6`
- Headers: Use `mb-6` consistently, `mb-4` only for compact sections

---

## Category 2: Button State Consistency

### Issues Found:

1. **Disabled state opacity** - Mix of `disabled:opacity-50` and no disabled styling
2. **Loading spinner consistency** - Different sizes (`h-5 w-5` vs `h-12 w-12`)
3. **Active scale** - Some buttons use `active:scale-95`, others don't
4. **Hover transitions** - Inconsistent `duration-150` vs `duration-200`
5. **Primary button variants** - Some use `hover:bg-primary/90`, others just `hover:bg-primary`

### Files to Fix:

- All button elements across order components
- `frontend/src/app/features/orders/order-create/components/order-sidebar/order-sidebar.component.html` (lines 88-109)
- `frontend/src/app/features/orders/order-details/order-details.component.html` (action buttons)
- `frontend/src/app/features/orders/order-list/order-list.component.html` (pagination buttons)

### Standard Button Classes:

```html
<!-- Primary Button -->
class="px-6 py-3 rounded-xl bg-primary hover:bg-primary/90 text-white font-semibold transition-all duration-200 active:scale-95 disabled:opacity-50 disabled:cursor-not-allowed shadow-md hover:shadow-lg"

<!-- Secondary Button -->
class="px-6 py-3 rounded-xl bg-surface-light dark:bg-surface-dark border border-border-light dark:border-border-dark hover:bg-border-light dark:hover:bg-border-dark text-text-primary-light dark:text-text-primary-dark font-medium transition-all duration-200 active:scale-95 disabled:opacity-50 disabled:cursor-not-allowed"

<!-- Icon Button -->
class="min-w-[44px] min-h-[44px] flex items-center justify-center rounded-xl bg-surface-light dark:bg-surface-dark border border-border-light dark:border-border-dark hover:bg-border-light dark:hover:bg-border-dark transition-all duration-200 active:scale-95 disabled:opacity-50 disabled:cursor-not-allowed"
```



### Loading Spinner Standard:

- Small (inline buttons): `h-5 w-5 border-2`
- Large (page loading): `h-12 w-12 border-4`

---

## Category 3: Responsiveness (Tablet/1024px Issues)

### Issues Found:

1. **Order create grid** - Uses `lg:grid-cols-[1fr_400px]` which activates at 1024px, but sidebar might push content off-screen
2. **Sidebar sticky positioning** - `lg:sticky lg:top-6` might cause overflow issues
3. **Filter row wrapping** - Filters might overflow on tablet
4. **Table padding** - Desktop table uses `px-6 py-4`, might be too large for tablet
5. **Modal max-width** - `max-w-md` (448px) might be too narrow on tablets

### Files to Fix:

- `frontend/src/app/features/orders/order-create/order-create.component.html` (line 32)
- `frontend/src/app/features/orders/order-list/order-list.component.html` (filter section, table)
- Modal components (checkout, cancel, reprint)

### Responsive Fixes:

```html
<!-- Order Create Grid - Add md breakpoint -->
<div class="grid grid-cols-1 md:grid-cols-[1fr_380px] lg:grid-cols-[1fr_400px] xl:grid-cols-[1fr_450px] gap-4 md:gap-6">

<!-- Sidebar - Add max-height for overflow -->
<div class="lg:sticky lg:top-6 lg:self-start lg:max-h-[calc(100vh-3rem)]">

<!-- Table - Responsive padding -->
<th class="px-4 md:px-6 py-3 md:py-4 ...">

<!-- Modal - Wider on tablet -->
<div class="w-full max-w-md md:max-w-lg ...">
```

---

## Category 4: Z-Index & Layering

### Issues Found:

1. **Z-index conflict** - Toast container (`z-50`), modals (`z-50`), mobile sidebar (`z-50`) all use same value
2. **Backdrop layering** - Mobile sidebar backdrop uses `z-40`, modals use `z-50` - might conflict
3. **Toast above modals** - Toasts should appear above modals

### Files to Fix:

- `frontend/src/app/shared/components/toast-container/toast-container.component.html`
- All modal components (checkout, cancel, reprint)
- `frontend/src/app/layout/main-layout/main-layout.component.html`

### Z-Index Hierarchy (from lowest to highest):

- Base content: `z-0` (default)
- Sticky elements: `z-10`
- Dropdowns: `z-20`
- Fixed headers/sidebars: `z-30`
- Mobile sidebar backdrop: `z-40`
- Modals backdrop: `z-40`
- Modals content: `z-50`
- Toast container: `z-[60]` (above modals)
- Toast items: `z-[60]`

### Fixes:

```html
<!-- Toast Container -->
<div class="fixed top-4 right-4 z-[60] ...">

<!-- Modal Backdrop -->
<div class="fixed inset-0 z-40 ...">

<!-- Modal Content -->
<div class="... z-50 ...">

<!-- Mobile Sidebar -->
<aside class="... z-30 ...">
```

---

## Category 5: Polish (Shadows, Transitions, Border-Radius)

### Issues Found:

1. **Shadow inconsistency** - Mix of `shadow-lg`, `shadow-xl`, `shadow-2xl`, `shadow-md`
2. **Border-radius inconsistency** - Mix of `rounded-xl` (12px), `rounded-2xl` (16px), `rounded-lg` (8px)
3. **Transition duration** - Mix of `duration-150`, `duration-200`, unspecified
4. **Hover shadow** - Some buttons have `hover:shadow-lg`, others don't

### Standardization Rules:

#### Shadows:

- Cards: `shadow-lg` (default), `hover:shadow-xl` on interactive cards
- Modals: `shadow-2xl`
- Buttons: `shadow-md` (primary), `hover:shadow-lg`
- Surface elements: `shadow-sm` or no shadow

#### Border Radius:

- Buttons/Inputs: `rounded-xl` (12px)
- Cards/Modals: `rounded-2xl` (16px)
- Badges/Chips: `rounded-full`
- Small elements: `rounded-lg` (8px)

#### Transitions:

- Standard: `transition-all duration-200`
- Fast (micro-interactions): `transition-all duration-150`
- Always include timing: Never use just `transition-all`

### Files to Update:

All component HTML files - standardize shadows, border-radius, and transitions.---

## Category 6: Typography & Font Scaling

### Issues Found:

1. **Heading sizes** - `text-3xl`, `text-2xl`, `text-xl` - need responsive scaling
2. **Mobile text sizes** - Some headings too large on mobile
3. **Line-height inconsistencies** - Missing explicit line-height for better readability
4. **Font weight consistency** - Mix of `font-semibold`, `font-bold`, `font-medium`

### Typography Scale:

- H1 (Page Title): `text-2xl md:text-3xl font-bold` + `leading-tight`
- H2 (Section Title): `text-xl md:text-2xl font-bold` + `leading-tight`
- H3 (Card Title): `text-lg md:text-xl font-semibold`
- Body: `text-sm md:text-base` with `leading-relaxed`
- Small text: `text-xs md:text-sm`

### Files to Fix:

- All heading elements across order components
- Product card titles
- Table headers

---

## Implementation Checklist

### Phase 1: Core Standardization

- [ ] Standardize all spacing to 8px grid (gap-2, gap-4, gap-6)
- [ ] Fix button states (disabled, loading, hover, active)
- [ ] Standardize padding (p-4 on mobile, p-6 on desktop)
- [ ] Fix border-radius (rounded-xl for buttons, rounded-2xl for cards)

### Phase 2: Responsiveness

- [ ] Add md: breakpoints for order-create grid
- [ ] Fix sidebar overflow on tablet (max-height)
- [ ] Adjust table padding for tablet
- [ ] Update modal widths for tablet

### Phase 3: Z-Index & Layering

- [ ] Update toast container to z-[60]
- [ ] Verify modal z-index hierarchy
- [ ] Test layering with modals open

### Phase 4: Polish

- [ ] Standardize shadows across components
- [ ] Standardize transitions (duration-200 default)
- [ ] Add hover shadow to interactive elements
- [ ] Fix font scaling for mobile

### Phase 5: Testing

- [ ] Test at 1024px (tablet landscape)
- [ ] Test at 768px (tablet portrait)
- [ ] Test modal layering
- [ ] Test button states (hover, active, disabled, loading)
- [ ] Verify spacing consistency visually

---

## Specific File-by-File Changes

### `frontend/src/app/features/orders/order-create/order-create.component.html`

- Line 1: Change `p-4 md:p-6 lg:p-8` to `p-4 md:p-6` (remove lg:p-8)
- Line 3: Standardize gap to `gap-4`
- Line 32: Add md breakpoint: `md:grid-cols-[1fr_380px]`
- Line 32: Change gap to `gap-4 md:gap-6`

### `frontend/src/app/features/orders/order-create/components/order-sidebar/order-sidebar.component.html`

- Line 3: Change `p-6` to `p-4 md:p-6`
- Line 47: Change `px-6 py-4` to `px-4 md:px-6 py-3 md:py-4`
- Line 88-95: Standardize button classes (see Category 2)
- Line 96-109: Standardize checkout button with loading state

### `frontend/src/app/features/orders/order-list/order-list.component.html`

- Line 35: Change filter padding to `p-4 md:p-6`
- Line 36: Standardize gap to `gap-4`
- Line 142-160: Change table padding to `px-4 md:px-6`
- Line 294-308: Fix pagination button classes (inconsistent conditional classes)

### `frontend/src/app/features/orders/order-details/order-details.component.html`

- Line 22: Change padding to `p-4 md:p-6 lg:p-8`
- Line 286: Change modal z-index backdrop to `z-40`, content to `z-50`
- Line 342: Same z-index fix for reprint modal

### All Modal Components

- Standardize z-index: backdrop `z-40`, content `z-50`
- Update max-width: `max-w-md md:max-w-lg`
- Standardize transitions: `duration-200`

### `frontend/src/app/shared/components/toast-container/toast-container.component.html`

- Line 1: Change `z-50` to `z-[60]`