# Access Levels Documentation - Order Management System

## System Roles
- **SuperAdmin**: System administrator with highest privileges
- **Admin**: Tenant administrator
- **Manager**: Restaurant manager
- **Waiter**: Service staff
- **Kitchen**: Kitchen staff
- **InventoryManager**: Inventory management staff

---

## API Endpoints Access Matrix

| Module | Endpoint | Method | API Authorization | Allowed Roles | Notes |
|--------|----------|--------|-------------------|---------------|-------|
| **Auth** | `/api/v1/Auth/login` | POST | `[AllowAnonymous]` | All (No auth required) | Public endpoint |
| **Auth** | `/api/v1/Auth/seed-super-admin` | POST | `[AllowAnonymous]` | All (No auth required) | Public endpoint |
| **Navigation** | `/api/v1/Navigation/menu` | GET | `[Authorize]` | All authenticated users | Returns menu filtered by user roles |
| **Orders** | `/api/v1/Orders` | POST | `[Authorize(Roles = "SuperAdmin,Manager,Waiter")]` | SuperAdmin, Manager, Waiter | Create order |
| **Orders** | `/api/v1/Orders` | GET | `[Authorize(Roles = "SuperAdmin,Manager,Waiter")]` | SuperAdmin, Manager, Waiter | List orders |
| **Orders** | `/api/v1/Orders/{id}` | GET | `[Authorize(Roles = "SuperAdmin,Manager,Waiter")]` | SuperAdmin, Manager, Waiter | Get order details |
| **Orders** | `/api/v1/Orders/{id}/status` | PATCH | `[Authorize(Roles = "SuperAdmin,Kitchen,Manager")]` | SuperAdmin, Kitchen, Manager | Update order status |
| **Orders** | `/api/v1/Orders/qr` | POST | `[Authorize(Roles = "SuperAdmin,Manager,Waiter")]` | SuperAdmin, Manager, Waiter | Generate QR session |
| **Menu Items** | `/api/v1/MenuItems` | GET | `[Authorize(Roles = "SuperAdmin,Admin,Manager")]` | SuperAdmin, Admin, Manager | List menu items |
| **Menu Items** | `/api/v1/MenuItems/{id}` | GET | `[Authorize(Roles = "SuperAdmin,Admin,Manager")]` | SuperAdmin, Admin, Manager | Get menu item |
| **Menu Items** | `/api/v1/MenuItems` | POST | `[Authorize(Roles = "SuperAdmin,Admin,Manager")]` | SuperAdmin, Admin, Manager | Create menu item |
| **Menu Items** | `/api/v1/MenuItems/{id}` | PUT | `[Authorize(Roles = "SuperAdmin,Admin,Manager")]` | SuperAdmin, Admin, Manager | Update menu item |
| **Menu Items** | `/api/v1/MenuItems/{id}` | DELETE | `[Authorize(Roles = "SuperAdmin,Admin,Manager")]` | SuperAdmin, Admin, Manager | Delete menu item |
| **Kitchen** | `/api/v1/Kitchen/queue` | GET | `[Authorize(Roles = "SuperAdmin,Kitchen,Manager")]` | SuperAdmin, Kitchen, Manager | Get kitchen queue |
| **Reports** | `/api/v1/Reports/sales` | GET | `[Authorize]` | All authenticated users | Sales report |
| **Reports** | `/api/v1/Reports/inventory` | GET | `[Authorize]` | All authenticated users | Inventory report |
| **Reports** | `/api/v1/Reports/subscription` | GET | `[Authorize(Roles = "SuperAdmin,Admin")]` | SuperAdmin, Admin | Subscription usage |
| **Subscriptions** | `/api/v1/Subscriptions/{tenantId}/upgrade` | POST | `[Authorize(Roles = "SuperAdmin")]` | SuperAdmin only | Request plan change |
| **Menu Management** | `/api/v1/menu-management/items` | GET | `[Authorize(Roles = "SuperAdmin")]` | SuperAdmin only | Get all menu items |
| **Menu Management** | `/api/v1/menu-management/items/{id}` | GET | `[Authorize(Roles = "SuperAdmin")]` | SuperAdmin only | Get menu item |
| **Menu Management** | `/api/v1/menu-management/items/{id}/permissions` | PUT | `[Authorize(Roles = "SuperAdmin")]` | SuperAdmin only | Update permissions |
| **Menu Management** | `/api/v1/menu-management/items/{id}/order` | PUT | `[Authorize(Roles = "SuperAdmin")]` | SuperAdmin only | Update display order |
| **Menu Management** | `/api/v1/menu-management/items/{id}/toggle` | PUT | `[Authorize(Roles = "SuperAdmin")]` | SuperAdmin only | Toggle menu item |
| **Menu Management** | `/api/v1/menu-management/items/{id}/permissions` | GET | `[Authorize(Roles = "SuperAdmin")]` | SuperAdmin only | Get permissions |
| **Tenants** | `/api/v1/Tenants` | POST | `[Authorize(Roles = "SuperAdmin")]` | SuperAdmin only | Create tenant |
| **Tenants** | `/api/v1/Tenants/plans` | GET | `[AllowAnonymous]` | All (No auth required) | Get subscription plans |

---

## Frontend Routes & Components Access Matrix

| Route | Component | Frontend Guard | Component-Level Check | Allowed Roles | API Endpoint Used |
|-------|-----------|----------------|----------------------|---------------|-------------------|
| `/login` | LoginComponent | None (public) | None | All (public) | `POST /api/v1/Auth/login` |
| `/dashboard` | DashboardComponent | `authGuard` (authenticated) | None | All authenticated users | None (display only) |
| `/menu` | MenuListComponent | `authGuard` (authenticated) | `hasAnyRole(['SuperAdmin', 'Admin', 'Manager'])` | SuperAdmin, Admin, Manager | `GET /api/v1/MenuItems` |
| `/orders` | OrderListComponent | `authGuard` (authenticated) | None | All authenticated users | `GET /api/v1/Orders` |
| `/orders/create` | OrderCreateComponent | `authGuard` (authenticated) | None | All authenticated users | `POST /api/v1/Orders` |
| `/orders/:id` | OrderDetailsComponent | `authGuard` (authenticated) | None | All authenticated users | `GET /api/v1/Orders/{id}` |
| `/kitchen` | KitchenDisplayComponent | `authGuard` (authenticated) | None | All authenticated users | `GET /api/v1/Kitchen/queue`, `PATCH /api/v1/Orders/{id}/status` |
| `/reports` | ReportsComponent | `authGuard` (authenticated) | None | All authenticated users | `GET /api/v1/Reports/*` |
| `/settings` | SettingsComponent | `authGuard` (authenticated) | None | All authenticated users | None (TODO) |
| `/tables` | FloorPlanComponent | `authGuard` (authenticated) | None | All authenticated users | None (TODO) |
| `/qr` | CustomerMenuComponent | None (public) | None | All (public) | None (TODO) |

---

## Role-by-Module Access Matrix

| Module | SuperAdmin | Admin | Manager | Waiter | Kitchen | InventoryManager |
|--------|:----------:|:-----:|:-------:|:------:|:-------:|:----------------:|
| **Authentication** | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Navigation Menu** | ✅* | ✅* | ✅* | ✅* | ✅* | ✅* |
| **Orders - Create/View** | ❌ | ❌ | ✅ | ✅ | ❌ | ❌ |
| **Orders - Update Status** | ❌ | ❌ | ✅ | ❌ | ✅ | ❌ |
| **Menu Items (CRUD)** | ❌ | ✅ | ✅ | ❌ | ❌ | ❌ |
| **Kitchen Queue** | ❌ | ❌ | ✅ | ❌ | ✅ | ❌ |
| **Reports - Sales/Inventory** | ❌ | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Reports - Subscription** | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |
| **Subscriptions** | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ |
| **Menu Management** | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ |
| **Tenant Management** | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ |
| **Dashboard** | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Settings** | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |

*Navigation menu items are filtered based on `allowedRoles` property from backend. Users only see menu items where their role is in the `allowedRoles` list.

### ⚠️ IMPORTANT: Code vs. Documentation Discrepancy

**Current API Code Implementation:**
- SuperAdmin IS included in role lists for: Orders, Menu Items, Kitchen Queue
- This means SuperAdmin currently HAS access to these modules in the actual code

**Documented Access Matrix (Above):**
- Shows SuperAdmin with NO access to: Orders, Menu Items, Kitchen Queue
- This reflects the intended/desired access levels

**Action Required:**
To align the code with the documented access matrix, the following changes are needed:
- `OrdersController.cs` - Remove SuperAdmin from class-level and UpdateStatus method
- `MenuItemsController.cs` - Remove SuperAdmin from class-level
- `KitchenController.cs` - Remove SuperAdmin from class-level
- `SubscriptionsController.cs` - Remove Admin from class-level (change to SuperAdmin only)

---

## Authorization Logic

### API Authorization Logic
1. **`[Authorize]`**: Requires any authenticated user (any role)
2. **`[Authorize(Roles = "Role1,Role2")]`**: Requires user to have at least one of the specified roles
3. **`[AllowAnonymous]`**: No authentication required
4. **Method-level overrides**: Method-level `[Authorize]` attributes override class-level attributes

### Frontend Authorization Logic
1. **Route Guard (`authGuard`)**: Checks if user is authenticated (any role)
2. **Component-level checks**: Uses `authService.hasAnyRole(['Role1', 'Role2'])`
3. **Navigation Service**: Filters menu items based on `allowedRoles` property
4. **AuthService.hasAnyRole()**: 
   - Returns `true` if user is SuperAdmin (bypass logic)
   - Otherwise checks if user has any of the specified roles

---

## Synchronization Issues & Discrepancies

### ⚠️ ISSUE 1: Orders Module - Frontend Missing Guard
- **API**: Requires `SuperAdmin, Manager, Waiter`
- **Frontend**: No component-level check in `OrderListComponent` or `OrderCreateComponent`
- **Impact**: Any authenticated user can access the route, but API will reject unauthorized users
- **Recommendation**: Add `hasAnyRole(['SuperAdmin', 'Manager', 'Waiter'])` check in order components

### ⚠️ ISSUE 2: Menu Items Module - Partial Synchronization
- **API**: Requires `SuperAdmin, Admin, Manager`
- **Frontend**: `MenuListComponent` has check for `['SuperAdmin', 'Admin', 'Manager']` ✅
- **Status**: Synchronized

### ⚠️ ISSUE 3: Kitchen Module - Frontend Missing Guard
- **API**: Requires `SuperAdmin, Kitchen, Manager`
- **Frontend**: No component-level check in `KitchenDisplayComponent`
- **Impact**: Any authenticated user can access the route, but API will reject unauthorized users
- **Recommendation**: Add `hasAnyRole(['SuperAdmin', 'Kitchen', 'Manager'])` check

### ⚠️ ISSUE 4: Reports Module - Frontend Missing Guard
- **API**: 
  - Sales/Inventory: `[Authorize]` (all authenticated)
  - Subscription: `SuperAdmin, Admin`
- **Frontend**: No component-level check in `ReportsComponent`
- **Impact**: All authenticated users can access, but subscription report will fail for non-SuperAdmin/Admin
- **Recommendation**: Add conditional check for subscription report access

### ⚠️ ISSUE 5: AuthService.hasAnyRole() Bypass Logic
- **Current Behavior**: `hasAnyRole()` returns `true` for SuperAdmin regardless of roles parameter
- **API Behavior**: SuperAdmin must be explicitly included in role lists
- **Impact**: Frontend may show UI elements to SuperAdmin that API will reject (though this is unlikely since SuperAdmin is in most role lists)
- **Note**: This is intentional frontend behavior for better UX, but creates a slight mismatch

### ✅ SYNCHRONIZED: Menu Management
- **API**: `SuperAdmin` only
- **Frontend**: No direct route (likely accessed via navigation menu which is filtered by backend)

### ✅ SYNCHRONIZED: Tenant Management
- **API**: `SuperAdmin` only
- **Frontend**: No direct route (likely accessed via navigation menu which is filtered by backend)

---

## Summary

### Fully Synchronized Modules
- Menu Items (with component-level check)
- Menu Management (SuperAdmin only, filtered via navigation)
- Tenant Management (SuperAdmin only, filtered via navigation)
- Navigation (filtered by backend based on roles)

### Partially Synchronized Modules
- Orders: API protected, frontend not guarded
- Kitchen: API protected, frontend not guarded
- Reports: API protected, frontend not guarded (subscription endpoint has stricter requirements)

### Authorization Pattern
- **Backend**: Explicit role requirements - SuperAdmin must be in role list
- **Frontend**: Route-level authentication + component-level role checks (where implemented)
- **Navigation**: Backend-driven filtering based on `allowedRoles` property

