---
name: Remove Menu Items from SuperAdmin Dashboard
overview: Remove all menu-related UI elements from the dashboard for SuperAdmin users, as menu management should only be accessible to Admin and Manager roles per the access matrix.
todos:
  - id: remove-menu-text-superadmin
    content: Remove isSuperAdmin from 'Add menu items to your catalog' text condition in empty state (line 42)
    status: pending
  - id: remove-menu-button-superadmin
    content: Remove isSuperAdmin from 'Manage Menu' button condition in empty state (line 74)
    status: pending
---

# Remove Menu Items from SuperAdmin Dashboard

## Issue

The dashboard currently shows menu-related elements to SuperAdmin users, but according to the access matrix, SuperAdmin should NOT have access to Menu Items (CRUD). These elements should only be visible to Admin and Manager.

## Changes Required

### File: `frontend/src/app/features/dashboard/dashboard.component.html`

1. **Line 42-46**: Remove `isSuperAdmin` from the condition showing "Add menu items to your catalog"

- Change: `@if (isManager || isAdmin || isSuperAdmin)` 
- To: `@if (isManager || isAdmin)`

2. **Line 74-78**: Remove `isSuperAdmin` from the condition showing "Manage Menu" button in empty state

- Change: `@if (isManager || isAdmin || isSuperAdmin)`
- To: `@if (isManager || isAdmin)`

Note: The Quick Actions section (line 231) already correctly excludes SuperAdmin, so no change needed there.

## Expected Outcome

- SuperAdmin users will not see "Add menu items to your catalog" in the empty state
- SuperAdmin users will not see "Manage Menu" button in the empty state
- Admin and Manager users will continue to see these menu-related elements
- Quick Actions section remains unchanged (already correct)