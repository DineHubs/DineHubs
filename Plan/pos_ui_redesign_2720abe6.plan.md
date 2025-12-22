---
name: POS UI Redesign
overview: Transform the Angular 19 POS system from Material Design to a modern dark/light mode interface using Tailwind CSS, Signals for state management, and custom components with glassmorphism effects, rounded corners, and micro-interactions for a premium bistro experience.
todos:
  - id: install-deps
    content: Install Tailwind CSS, Lucide-Angular, and configure build system. Remove Angular Material dependencies.
    status: completed
  - id: setup-theme
    content: Create theme service with dark/light mode toggle, configure Tailwind with custom color palette, add Inter font.
    status: completed
    dependencies:
      - install-deps
  - id: refactor-signals
    content: Convert order-create.component.ts to use Signals for all state (menuItems, cart, filters, etc.) and computed values.
    status: completed
    dependencies:
      - install-deps
  - id: create-product-grid
    content: Build product-grid and product-card components with Tailwind styling, touch-friendly targets, and hover effects.
    status: completed
    dependencies:
      - refactor-signals
      - setup-theme
  - id: create-category-nav
    content: Build category navigation component with horizontal scroll, active states, and smooth transitions.
    status: completed
    dependencies:
      - refactor-signals
      - setup-theme
  - id: create-order-sidebar
    content: Build order-sidebar and cart-item components with glassmorphism effects, sticky positioning, and scrollable cart list.
    status: completed
    dependencies:
      - refactor-signals
      - setup-theme
  - id: create-checkout-modal
    content: Build checkout modal component with custom form inputs, payment options, and responsive overlay behavior.
    status: completed
    dependencies:
      - create-order-sidebar
  - id: integrate-components
    content: Integrate all components into order-create, implement responsive layout, and add theme toggle to header.
    status: completed
    dependencies:
      - create-product-grid
      - create-category-nav
      - create-order-sidebar
      - create-checkout-modal
  - id: add-interactions
    content: Add micro-interactions (button presses, hover effects, loading states, toast notifications) and smooth animations.
    status: completed
    dependencies:
      - integrate-components
  - id: polish-accessibility
    content: Test and improve accessibility (keyboard navigation, ARIA labels, focus indicators, contrast ratios).
    status: completed
    dependencies:
      - add-interactions
---

# POS System UI Redesign Plan

## Overview

Complete redesign of the restaurant POS system from Angular Material to a modern Tailwind CSS-based interface with dark/light mode support, Signals-based state management, and premium UI/UX patterns.

## Architecture Changes

### 1. Dependencies & Setup

- **Install Tailwind CSS v3** with Angular configuration
- **Install Lucide-Angular** for modern icon set
- **Install Inter font** (or Geist) via Google Fonts
- **Remove Angular Material dependencies** (keep only if absolutely necessary for complex components)
- **Configure Tailwind** with custom color palette and dark mode support

### 2. Color Palette & Design Tokens

**Dark Mode:**

- Background: `#0A0A0F` (deep dark)
- Surface: `#141420` (elevated cards)
- Primary: `#6366F1` (indigo accent)
- Secondary: `#8B5CF6` (purple accent)
- Success: `#10B981` (emerald)
- Warning: `#F59E0B` (amber)
- Text Primary: `#F9FAFB` (near white)
- Text Secondary: `#9CA3AF` (gray-400)
- Border: `#1F2937` (gray-800)

**Light Mode:**

- Background: `#FAFAFA` (off-white)
- Surface: `#FFFFFF` (pure white)
- Primary: `#4F46E5` (indigo-600)
- Secondary: `#7C3AED` (purple-600)
- Text Primary: `#111827` (gray-900)
- Text Secondary: `#6B7280` (gray-500)
- Border: `#E5E7EB` (gray-200)

**Glassmorphism:**

- Backdrop blur: `backdrop-blur-xl`
- Background opacity: `bg-white/10` (dark) or `bg-black/5` (light)
- Border: `border border-white/20` (dark) or `border border-black/10` (light)

### 3. Component Architecture with Signals

**Refactor `order-create.component.ts` to use Signals:**

- Convert `menuItems`, `filteredItems`, `cart`, `tableNumber`, `isTakeAway`, `selectedCategory`, `searchTerm` to signals
- Use `computed()` for derived state (cart total, item count, filtered items)
- Implement `effect()` for side effects (filtering, search)
- Create signal-based service for cart state management (optional, for reusability)

**Component Structure:**

```
order-create/
  ├── order-create.component.ts (Signals-based)
  ├── order-create.component.html (Tailwind markup)
  ├── order-create.component.scss (minimal, mostly Tailwind)
  ├── components/
  │   ├── product-grid/
  │   │   ├── product-grid.component.ts
  │   │   ├── product-grid.component.html
  │   │   └── product-card.component.ts (standalone)
  │   ├── category-nav/
  │   │   ├── category-nav.component.ts
  │   │   └── category-nav.component.html
  │   ├── order-sidebar/
  │   │   ├── order-sidebar.component.ts
  │   │   ├── order-sidebar.component.html
  │   │   └── cart-item.component.ts (standalone)
  │   └── checkout-modal/
  │       ├── checkout-modal.component.ts
  │       └── checkout-modal.component.html
```

### 4. Layout Structure

**Main Layout (order-create.component.html):**

```
┌─────────────────────────────────────────────────────────┐
│  Header: Search + Theme Toggle                          │
├──────────────────────┬──────────────────────────────────┤
│                      │                                  │
│  Category Navigation │  Product Grid (responsive)       │
│  (horizontal scroll) │  (auto-fill, min 200px)         │
│                      │                                  │
│                      │                                  │
├──────────────────────┴──────────────────────────────────┤
│  Order Sidebar (sticky, right side on desktop,          │
│  bottom sheet on mobile)                                 │
│  ┌──────────────────────────────────────────────────┐  │
│  │ Table/Takeaway Input                             │  │
│  │ ──────────────────────────────────────────────── │  │
│  │ Cart Items (scrollable)                          │  │
│  │ ──────────────────────────────────────────────── │  │
│  │ Total + Checkout Button                          │  │
│  └──────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────┘
```

**Responsive Breakpoints:**

- Mobile: `< 768px` - Single column, bottom sheet cart
- Tablet: `768px - 1024px` - Two columns, sidebar cart
- Desktop: `> 1024px` - Full layout with sticky sidebar

### 5. Key File Changes

**Files to Modify:**

1. `frontend/package.json` - Add Tailwind, Lucide-Angular, remove Material
2. `frontend/angular.json` - Update styles array, add Tailwind config
3. `frontend/tailwind.config.js` - Create with custom theme, dark mode
4. `frontend/src/styles.scss` - Import Tailwind, Inter font, global styles
5. `frontend/src/index.html` - Add Inter font link
6. `frontend/src/app/features/orders/order-create/order-create.component.ts` - Convert to Signals
7. `frontend/src/app/features/orders/order-create/order-create.component.html` - Complete Tailwind rewrite
8. `frontend/src/app/features/orders/order-create/order-create.component.scss` - Minimal custom styles

**New Files to Create:**

1. `frontend/src/app/features/orders/order-create/components/product-card/product-card.component.ts`
2. `frontend/src/app/features/orders/order-create/components/category-nav/category-nav.component.ts`
3. `frontend/src/app/features/orders/order-create/components/order-sidebar/order-sidebar.component.ts`
4. `frontend/src/app/features/orders/order-create/components/cart-item/cart-item.component.ts`
5. `frontend/src/app/features/orders/order-create/components/checkout-modal/checkout-modal.component.ts`
6. `frontend/src/app/core/services/theme.service.ts` - Theme toggle service
7. `frontend/src/app/core/services/cart.service.ts` - Optional cart state service

### 6. Implementation Details

**Product Grid:**

- Grid layout: `grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5 gap-4`
- Cards: `rounded-2xl`, `bg-surface`, `shadow-lg`, `hover:shadow-xl`, `transition-all duration-200`
- Touch targets: Minimum `44x44px` for buttons
- Image: `aspect-square`, `rounded-t-2xl`, `object-cover`
- Price: Large, bold, high contrast (`text-2xl font-bold text-primary`)

**Category Navigation:**

- Horizontal scroll: `flex overflow-x-auto gap-2 pb-2`
- Pills: `rounded-full px-6 py-2`, active state with `bg-primary text-white`
- Smooth scroll behavior

**Order Sidebar:**

- Glassmorphism: `backdrop-blur-xl bg-surface/80 border border-border/50`
- Sticky positioning: `sticky top-4`
- Scrollable cart: `max-h-[calc(100vh-300px)] overflow-y-auto`
- Fixed footer: Total and checkout button always visible

**Cart Items:**

- List layout with clear hierarchy
- Quantity controls: Large touch-friendly buttons (`min-w-[44px] min-h-[44px]`)
- Micro-interactions: Scale on press (`active:scale-95`)
- Smooth animations: `transition-transform duration-150`

**Checkout Modal:**

- Full-screen overlay on mobile, centered modal on desktop
- Backdrop: `bg-black/50 backdrop-blur-sm`
- Modal: `rounded-2xl`, `bg-surface`, `shadow-2xl`
- Form inputs: Custom styled with Tailwind (no Material)
- Payment options: Card-based selection

**Micro-interactions:**

- Button press: `active:scale-95 transition-transform`
- Hover effects: `hover:scale-105` for cards
- Loading states: Skeleton screens or subtle spinners
- Success feedback: Toast notifications with slide-in animation

### 7. Typography

**Font Stack:**

- Primary: `Inter` (via Google Fonts)
- Fallback: `system-ui, -apple-system, sans-serif`
- Headings: `font-semibold` or `font-bold`
- Body: `font-normal`
- Sizes: Use Tailwind scale (`text-sm`, `text-base`, `text-lg`, `text-xl`, `text-2xl`)

### 8. Accessibility

- WCAG AA contrast ratios for all text
- Keyboard navigation support
- Focus indicators: `focus:ring-2 focus:ring-primary focus:ring-offset-2`
- ARIA labels for icon-only buttons
- Screen reader announcements for cart updates

### 9. Performance Optimizations

- Lazy load product images
- Virtual scrolling for large product lists (if needed)
- OnPush change detection strategy
- Signal-based reactivity (automatic optimization)
- Debounced search input

### 10. Testing Considerations

- Touch event handlers for mobile
- Responsive breakpoint testing
- Theme toggle functionality
- Cart state persistence (optional: localStorage)
- Error handling and loading states

## Implementation Order

1. Install dependencies and configure Tailwind
2. Set up theme service and global styles
3. Refactor order-create component to use Signals
4. Create product-grid and product-card components
5. Create category-nav component
6. Create order-sidebar and cart-item components
7. Create checkout-modal component
8. Integrate all components and test responsive behavior
9. Add micro-interactions and polish
10. Test accessibility and performance