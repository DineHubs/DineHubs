---
name: Update Route Access Control
overview: Update route access control to exclude SuperAdmin from operational routes (POS, KDS, Waitstaff, Delivery) while maintaining access to admin routes.
todos:
  - id: update-pos-route
    content: Update /pos route to include Cashier, Waitstaff, Manager, Admin and exclude SuperAdmin
    status: completed
  - id: update-kds-route
    content: Remove SuperAdmin from /kds route access (keep KitchenStaff, Manager)
    status: completed
  - id: update-waitstaff-route
    content: Remove SuperAdmin from /waitstaff route access (keep Waitstaff, Manager)
    status: completed
  - id: update-delivery-route
    content: Update /delivery route to Manager and DeliveryStaff only (exclude SuperAdmin)
    status: completed
  - id: update-role-guard
    content: Update role.guard.ts to support excludeRoles and handle SuperAdmin exclusion
    status: completed
  - id: test-access-control
    content: Verify access control works correctly for all roles and routes
    status: completed
    dependencies:
      - update-pos-route
      - update-kds-route
      - update-waitstaff-route
      - update-delivery-route
      - update-role-guard
---

# Update Route Access Control

## Requirements

Update the route access control to implement the following changes:

### Route Access Changes

1. **`/dashboard`** - No change (All authenticated users)

2. **`/pos`** - Change to: `Cashier, Waitstaff, Manager, Admin` (exclude SuperAdmin)

3. **`/kds`** - Change to: `KitchenStaff, Manager` (exclude SuperAdmin)

4. **`/waitstaff`** - Change to: `Waitstaff, Manager` (exclude SuperAdmin)

5. **`/delivery`** - Change to: `Manager, DeliveryStaff` (exclude SuperAdmin)

6. **`/admin`** - No change (`Manager, SuperAdmin`)

7. **`/qr`** - No change (Public, no auth required)

8. **`/auth`** - No change (Public)

## Implementation Plan

### Files to Modify

1. **`UI/src/app/app.routes.ts`**

- Update `/pos` route: Add `roleGuard` with roles `['Cashier', 'Waitstaff', 'Manager', 'Admin']` and exclude `SuperAdmin`
- Update `/kds` route: Remove `SuperAdmin` from roles array
- Update `/waitstaff` route: Remove `SuperAdmin` from roles array  
- Update `/delivery` route: Change to `['Manager', 'DeliveryStaff']` (remove SuperAdmin)

2. **`UI/src/app/core/guards/role.guard.ts`**

- Add support for `excludeRoles` in route data
- Check excluded roles before allowing access
- Redirect SuperAdmin to `/admin` when trying to access excluded routes

### Implementation Details

The role guard will need to:

- Check if the route has `excludeRoles` in its data
- If user has an excluded role, deny access and redirect appropriately
- Otherwise, check if user has any of the required roles

### Testing Considerations

- Verify SuperAdmin cannot access `/pos`, `/kds`, `/waitstaff`, `/delivery`
- Verify SuperAdmin is redirected to `/admin` when attempting to access excluded routes
- Verify all other roles can access their designated routes correctly
- Verify authentication still works as expected