---
name: Angular UI/UX Design Plan
overview: Design and implement comprehensive Angular UI components for POS screen, KDS screen, QR ordering, and Admin screens with PrimeNG, internationalization (English/Malay), offline support, error handling, and accessibility features.
todos:
  - id: setup-project
    content: Initialize Angular project with PrimeNG, configure routing, i18n, and core services
    status: pending
  - id: shared-components
    content: Create shared components (loading, error, currency, language switcher, offline indicator, confirmation dialog)
    status: pending
    dependencies:
      - setup-project
  - id: pos-screen
    content: Build POS screen components (layout, menu search, menu grid, cart, modifier dialog, split bill, payment)
    status: pending
    dependencies:
      - shared-components
  - id: kds-screen
    content: Build KDS screen components (layout, order tickets, item cards, settings, printer integration)
    status: pending
    dependencies:
      - shared-components
  - id: qr-ordering
    content: Build QR ordering components (landing, menu browser, item detail, cart, checkout, order status)
    status: pending
    dependencies:
      - shared-components
  - id: admin-screens
    content: Build admin components (layout, menu management, inventory, roles, settings)
    status: pending
    dependencies:
      - shared-components
  - id: i18n-implementation
    content: Implement internationalization with English/Malay translations for all components
    status: pending
    dependencies:
      - setup-project
  - id: offline-support
    content: Implement offline service, storage, and UI patterns for offline functionality
    status: pending
    dependencies:
      - shared-components
  - id: error-handling
    content: Implement comprehensive error states, error components, and recovery mechanisms
    status: pending
    dependencies:
      - shared-components
  - id: accessibility
    content: Add ARIA labels, keyboard navigation, screen reader support, and visual accessibility features
    status: pending
    dependencies:
      - pos-screen
      - kds-screen
      - qr-ordering
      - admin-screens
---

# Angular UI/UX Design Plan

## 1. Project Structure & Setup

### 1.1 Angular Project Initialization

- Create Angular 17+ project with standalone components
- Configure PrimeNG and PrimeIcons
- Set up Angular i18n for English/Malay
- Configure routing with lazy loading
- Set up environment configurations

### 1.2 Core Module Structure

```
src/app/
├── core/                          # Singleton services
│   ├── services/
│   │   ├── auth.service.ts
│   │   ├── api.service.ts
│   │   ├── cache.service.ts
│   │   ├── offline.service.ts
│   │   ├── notification.service.ts
│   │   └── translation.service.ts
│   ├── guards/
│   │   ├── auth.guard.ts
│   │   └── role.guard.ts
│   ├── interceptors/
│   │   ├── auth.interceptor.ts
│   │   ├── error.interceptor.ts
│   │   ├── offline.interceptor.ts
│   │   └── loading.interceptor.ts
│   └── models/
│       └── user.model.ts
│
├── shared/                        # Shared components & utilities
│   ├── components/
│   │   ├── loading-spinner/
│   │   ├── error-message/
│   │   ├── currency-display/
│   │   ├── language-switcher/
│   │   ├── offline-indicator/
│   │   └── confirmation-dialog/
│   ├── directives/
│   │   ├── currency-format.directive.ts
│   │   └── number-only.directive.ts
│   ├── pipes/
│   │   ├── currency.pipe.ts
│   │   ├── translate.pipe.ts
│   │   └── date-format.pipe.ts
│   └── models/
│
├── features/                      # Feature modules
│   ├── pos/
│   ├── kds/
│   ├── qr-ordering/
│   ├── admin/
│   └── reports/
│
├── layout/                        # Layout components
│   ├── header/
│   ├── sidebar/
│   └── footer/
│
└── i18n/                          # Internationalization
    ├── en/
    └── ms/
```

## 2. Core Shared Components

### 2.1 Loading Spinner Component

- **File**: `shared/components/loading-spinner/loading-spinner.component.ts`
- PrimeNG ProgressSpinner with overlay
- Configurable size and message
- Accessible with ARIA labels

### 2.2 Error Message Component

- **File**: `shared/components/error-message/error-message.component.ts`
- Display error states with icons
- Support for retry actions
- Different error types (network, validation, business)
- Accessible error announcements

### 2.3 Currency Display Component

- **File**: `shared/components/currency-display/currency-display.component.ts`
- Format MYR currency (RM 10.00)
- Support for different currency codes
- Color coding for positive/negative amounts

### 2.4 Language Switcher Component

- **File**: `shared/components/language-switcher/language-switcher.component.ts`
- Dropdown to switch between English/Malay
- Persist selection in localStorage
- Update all translations immediately

### 2.5 Offline Indicator Component

- **File**: `shared/components/offline-indicator/offline-indicator.component.ts`
- Banner showing offline status
- Queue indicator for pending operations
- Sync status when back online

### 2.6 Confirmation Dialog Component

- **File**: `shared/components/confirmation-dialog/confirmation-dialog.component.ts`
- Reusable PrimeNG Dialog for confirmations
- Support for custom messages and actions
- Accessible keyboard navigation

## 3. POS Screen Components

### 3.1 POS Layout Component

- **File**: `features/pos/components/pos-layout/pos-layout.component.ts`
- Split view: Menu on left, Cart on right
- Responsive design for tablets
- Keyboard shortcuts support

### 3.2 Menu Search Component

- **File**: `features/pos/components/menu-search/menu-search.component.ts`
- PrimeNG AutoComplete with search
- Search by name (English/Malay)
- Category filtering
- Quick category buttons
- Accessible with ARIA labels

### 3.3 Menu Grid Component

- **File**: `features/pos/components/menu-grid/menu-grid.component.ts`
- Card-based menu item display
- Show: image, name (i18n), price, halal badge, spice level
- Out of stock indicator
- Quick-add button
- Lazy loading for large menus

### 3.4 Quick Add Component

- **File**: `features/pos/components/quick-add/quick-add.component.ts`
- Floating action button for quick add
- Number pad for quantity
- Direct add to cart (default variants)
- Keyboard shortcuts (1-9 for quantity)

### 3.5 Cart Component

- **File**: `features/pos/components/cart/cart.component.ts`
- Display order items with quantities
- Item-level actions (edit, remove)
- Subtotal, SST, Total calculation
- Real-time price updates
- Accessible cart summary

### 3.6 Modifier Dialog Component

- **File**: `features/pos/components/modifier-dialog/modifier-dialog.component.ts`
- PrimeNG Dialog for item customization
- Variant selection (size, spice level)
- Modifier checkboxes/radio buttons
- Price modifiers displayed
- Special instructions textarea
- Required modifiers validation

### 3.7 Split Bill Component

- **File**: `features/pos/components/split-bill/split-bill.component.ts`
- Drag-and-drop items to split
- Multiple split options (by items, by amount)
- Customer name per split
- Preview of each split bill
- Generate separate orders

### 3.8 Payment Dialog Component

- **File**: `features/pos/components/payment-dialog/payment-dialog.component.ts`
- Payment method selection (Cash, TnG eWallet)
- Amount input with change calculation
- Receipt preview
- Print receipt option
- Payment confirmation

## 4. KDS Screen Components

### 4.1 KDS Layout Component

- **File**: `features/kds/components/kds-layout/kds-layout.component.ts`
- Full-screen kitchen display
- Station filter tabs (hot kitchen, cold kitchen, drinks)
- Status filter (pending, preparing, ready)
- Auto-refresh with SignalR/WebSocket
- Responsive grid layout

### 4.2 Order Ticket Component

- **File**: `features/kds/components/order-ticket/order-ticket.component.ts`
- Card display for each order
- Color-coded status (pending=yellow, preparing=blue, ready=green)
- Order number and table number
- Item list with quantities
- Special instructions highlighted
- Aging timer (time since order placed)
- Prep SLA countdown
- Priority indicator

### 4.3 Order Item Card Component

- **File**: `features/kds/components/order-item-card/order-item-card.component.ts`
- Individual item within order ticket
- Item name (i18n)
- Quantity badge
- Prep station indicator
- Status buttons (Start, Ready, Hold)
- Timer for item prep time

### 4.4 KDS Settings Component

- **File**: `features/kds/components/kds-settings/kds-settings.component.ts`
- Toggle between digital display and printer
- Printer configuration (IP, port, format)
- Display preferences (auto-refresh interval, sound alerts)
- Station assignment
- Admin/Manager only access

### 4.5 Printer Integration Service

- **File**: `features/kds/services/printer.service.ts`
- Send orders to thermal printer
- Print format templates
- Queue management for offline printers
- Printer status monitoring

## 5. QR Ordering Components

### 5.1 QR Landing Page Component

- **File**: `features/qr-ordering/components/qr-landing/qr-landing.component.ts`
- Welcome screen with restaurant branding
- Language selection (EN/MS)
- Table number display
- Start ordering button
- Mobile-optimized layout

### 5.2 Menu Browser Component

- **File**: `features/qr-ordering/components/menu-browser/menu-browser.component.ts`
- Category tabs/swipe navigation
- Menu item cards with images
- Filter by dietary (halal, vegetarian, vegan)
- Filter by spice level
- Search functionality
- Infinite scroll for performance

### 5.3 Menu Item Detail Component

- **File**: `features/qr-ordering/components/menu-item-detail/menu-item-detail.component.ts`
- Full item details (image, name, description, price)
- Compliance badges (halal, allergens)
- Variant selection
- Modifier selection
- Add to cart button
- Back navigation

### 5.4 Cart Component (QR)

- **File**: `features/qr-ordering/components/cart/cart.component.ts`
- Bottom sheet/drawer for cart
- Item list with edit/remove
- Quantity adjuster
- Subtotal, SST, Total
- Proceed to checkout button
- Mobile swipe gestures

### 5.5 Checkout Component

- **File**: `features/qr-ordering/components/checkout/checkout.component.ts`
- Order summary
- Customer information (name, phone - optional)
- Special instructions
- Payment method selection
- TnG eWallet integration
- Order confirmation
- Order tracking link

### 5.6 Order Status Component

- **File**: `features/qr-ordering/components/order-status/order-status.component.ts`
- Real-time order status updates
- Progress indicator (pending → preparing → ready)
- Estimated time display
- Notification when ready
- Order details and receipt

## 6. Admin Screens Components

### 6.1 Admin Layout Component

- **File**: `features/admin/components/admin-layout/admin-layout.component.ts`
- Sidebar navigation
- Header with user info and branch selector
- Breadcrumb navigation
- Role-based menu items

### 6.2 Menu Management Components

- **File**: `features/admin/components/menu-management/`
  - `menu-list.component.ts` - List all menu items with CRUD
  - `menu-item-form.component.ts` - Create/edit form with validation
  - `category-management.component.ts` - Category CRUD
  - `variant-modifier-management.component.ts` - Variants/modifiers management
  - Image upload component
  - Bulk operations (import/export)

### 6.3 Inventory Management Components

- **File**: `features/admin/components/inventory-management/`
  - `inventory-list.component.ts` - Stock levels with filters
  - `inventory-form.component.ts` - Add/edit inventory items
  - `stock-transaction.component.ts` - Stock in/out operations
  - `low-stock-alerts.component.ts` - Alert management
  - `supplier-management.component.ts` - Supplier CRUD
  - Stock level indicators (color-coded)

### 6.4 Role Management Components

- **File**: `features/admin/components/role-management/`
  - `role-list.component.ts` - List roles with permissions
  - `role-form.component.ts` - Create/edit roles
  - `permission-matrix.component.ts` - Visual permission assignment
  - `user-management.component.ts` - User CRUD with role assignment
  - Branch assignment for users

### 6.5 Settings Components

- **File**: `features/admin/components/settings/`
  - `branch-settings.component.ts` - Branch configuration
  - `payment-settings.component.ts` - Payment gateway setup
  - `printer-settings.component.ts` - Printer configuration
  - `tax-settings.component.ts` - SST configuration
  - `notification-settings.component.ts` - Email/SMS settings

## 7. Internationalization (i18n)

### 7.1 Translation Files Structure

- **Files**: 
  - `i18n/en/common.json` - Common translations
  - `i18n/en/menu.json` - Menu translations
  - `i18n/en/orders.json` - Order-related translations
  - `i18n/en/admin.json` - Admin translations
  - `i18n/ms/*.json` - Malay translations

### 7.2 Translation Service

- **File**: `core/services/translation.service.ts`
- Load translations dynamically
- Switch language without page reload
- Fallback to English if translation missing
- Currency formatting per locale

### 7.3 Translation Pipe

- **File**: `shared/pipes/translate.pipe.ts`
- Custom pipe for translations
- Support for parameters
- Pluralization support

## 8. Offline Support

### 8.1 Offline Service

- **File**: `core/services/offline.service.ts`
- Detect online/offline status
- Queue operations when offline
- Sync queue when back online
- Conflict resolution

### 8.2 Offline Storage

- **File**: `core/services/offline-storage.service.ts`
- IndexedDB for menu cache
- LocalStorage for user preferences
- Service Worker for asset caching
- Sync strategy (last-write-wins or merge)

### 8.3 Offline UI Patterns

- Visual indicators for offline mode
- Disable online-only features
- Show queued operations
- Sync progress indicator
- Error handling for sync failures

## 9. Error States & Handling

### 9.1 Error Types

- Network errors (offline, timeout, server error)
- Validation errors (form fields)
- Business rule errors (out of stock, invalid status)
- Permission errors (unauthorized access)

### 9.2 Error Display Components

- Toast notifications (PrimeNG Toast)
- Inline form errors
- Error pages (404, 500, offline)
- Retry mechanisms
- Error logging service

### 9.3 Error Recovery

- Automatic retry for transient errors
- Manual retry buttons
- Offline queue for failed operations
- User-friendly error messages (i18n)

## 10. Accessibility Features

### 10.1 ARIA Labels & Roles

- All interactive elements have ARIA labels
- Proper heading hierarchy (h1-h6)
- Landmark regions (nav, main, aside)
- Form labels and error associations

### 10.2 Keyboard Navigation

- Tab order logical flow
- Keyboard shortcuts for common actions
- Skip links for main content
- Focus management in modals
- Escape key to close dialogs

### 10.3 Screen Reader Support

- Semantic HTML elements
- Live regions for dynamic content
- Alt text for images
- Descriptive link text
- Form validation announcements

### 10.4 Visual Accessibility

- High contrast mode support
- Color not sole indicator (icons + text)
- Focus indicators visible
- Responsive text sizing
- WCAG 2.1 AA compliance target

## 11. State Management

### 11.1 Services with RxJS

- BehaviorSubject for shared state
- Menu cache service
- Cart state service
- Order state service
- User session service

### 11.2 State Management Pattern

- Service-based state (no NgRx initially)
- Reactive forms for form state
- Local component state where appropriate
- Consider NgRx if complexity grows

## 12. Performance Optimization

### 12.1 Lazy Loading

- Feature modules lazy loaded
- Route-based code splitting
- Component lazy loading for heavy components

### 12.2 Change Detection

- OnPush change detection strategy
- TrackBy functions for *ngFor
- Avoid unnecessary change detection

### 12.3 Caching Strategy

- Menu items cached in IndexedDB
- API response caching
- Image lazy loading
- Virtual scrolling for long lists

## 13. Responsive Design

### 13.1 Breakpoints

- Mobile: < 768px
- Tablet: 768px - 1024px
- Desktop: > 1024px

### 13.2 Responsive Components

- Adaptive layouts for all screens
- Touch-friendly targets (min 44x44px)
- Swipe gestures for mobile
- Collapsible sidebars on mobile

## 14. Testing Considerations

### 14.1 Component Testing

- Unit tests for components
- Service tests
- Pipe tests
- Directive tests

### 14.2 E2E Testing

- Critical user flows
- Payment flows
- Order creation flows
- Admin operations

## 15. Implementation Priority

### Phase 1: Core Infrastructure

1. Project setup with PrimeNG
2. i18n configuration
3. Core services (auth, API, offline)
4. Shared components
5. Layout components

### Phase 2: POS Screen

1. Menu search and grid
2. Cart component
3. Modifier dialog
4. Payment dialog
5. Split bill

### Phase 3: QR Ordering

1. Menu browser
2. Cart and checkout
3. Order status
4. Payment integration

### Phase 4: KDS Screen

1. KDS layout
2. Order tickets
3. Status updates
4. Printer integration (optional)

### Phase 5: Admin Screens

1. Menu management
2. Inventory management
3. Role management
4. Settings

### Phase 6: Polish

1. Offline support
2. Error handling
3. Accessibility improvements
4. Performance optimization

## 16. Key Design Decisions

1. **PrimeNG Components**: Use PrimeNG for consistent, accessible UI components
2. **Standalone Components**: Angular 17+ standalone components for better tree-shaking
3. **Service-Based State**: Start with services + RxJS, add NgRx if needed
4. **Progressive Enhancement**: Core functionality works offline, enhanced features online
5. **Mobile-First**: Design for mobile, enhance for larger screens
6. **Accessibility First**: Build accessible from start, not as afterthought
7. **i18n from Start**: All text in translation files, no hardcoded strings
8. **Error Boundaries**: Graceful error handling at component level

## 17. Dependencies

- @angular/core, @angular/common, @angular/router, @angular/forms
- @angular/platform-browser, @angular/platform-browser-dynamic
- primeng, primeicons
- @angular/localize (for i18n)
- rxjs
- idb (IndexedDB wrapper)
- workbox (Service Worker)

## 18. Configuration Files

- `angular.json` - Build configuration
- `tsconfig.json` - TypeScript configuration
- `prime-ng.config.ts` - PrimeNG theme configuration
- `i18n.config.ts` - Internationalization configuration
- `service-worker.config.js` - Service Worker configuration