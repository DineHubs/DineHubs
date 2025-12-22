---
name: Comprehensive Test Data Seeding
overview: Enhance the database seeder to include test users for all roles, additional food items, and drinks to enable comprehensive workflow testing.
todos:
  - id: add-delivery-staff-role-constant
    content: Add DeliveryStaff constant to RoleConstants.cs
    status: completed
  - id: add-delivery-staff-role
    content: Add DeliveryStaff role to SeedRolesAsync() method
    status: completed
    dependencies:
      - add-delivery-staff-role-constant
  - id: add-delivery-staff-permissions
    content: Add DeliveryStaff role permissions to SeedRolePermissionsAsync() method
    status: completed
    dependencies:
      - add-delivery-staff-role
  - id: add-all-role-users
    content: Add test users for Manager, Waitstaff, KitchenStaff, and DeliveryStaff roles
    status: completed
    dependencies:
      - add-delivery-staff-role
  - id: add-more-food-items
    content: Add more main course and appetizer items (Char Kway Teow, Laksa, Mee Goreng, Rendang, Satay, Keropok Lekor, Roti Jala)
    status: completed
  - id: add-more-drinks
    content: Add more beverage items (Kopi O, Sirap Bandung, Lime Juice, Soft Drinks, Fresh Juices)
    status: completed
  - id: add-test-customers
    content: Add SeedCustomersAsync() method with test customers
    status: completed
  - id: update-seeder-methods
    content: Update SeedAsync() to call SeedCustomersAsync()
    status: completed
    dependencies:
      - add-test-customers
---

# Comprehensive Test Data Seeding

## Objectives

Enhance the existing database seeder to include:

1. Test users for ALL roles (currently missing: Manager, Waitstaff, KitchenStaff, DeliveryStaff)
2. Additional food items (main courses, appetizers)
3. Additional drinks (hot and cold beverages)
4. Test customers for order testing
5. Ensure all users use a simple test password for easy login

## Current State

The seeder already includes:

- ✅ All roles (SuperAdmin, Admin, Manager, Cashier, Waitstaff, KitchenStaff)
- ✅ Some menu items (Nasi Lemak, Chicken Curry, Roti Canai, Spring Rolls, Teh Tarik, Ice Kacang, Cendol)
- ✅ Basic structure and permissions

Missing:

- ❌ Users for Manager, Waitstaff, KitchenStaff roles
- ❌ DeliveryStaff role (needs to be added)
- ❌ More food variety (more main courses, appetizers)
- ❌ More drink variety (coffee, juices, soft drinks)
- ❌ Test customers

## Implementation Plan

### 1. Add DeliveryStaff Role

- Add DeliveryStaff role to `SeedRolesAsync()` method
- Add DeliveryStaff role permissions to `SeedRolePermissionsAsync()` method

### 2. Enhance User Seeding

- Add users for all roles:
- Manager (manager/Manager)
- Waitstaff (waitstaff/Waitstaff)
- KitchenStaff (kitchen/Kitchen Staff)
- DeliveryStaff (delivery/Delivery Staff)
- All users should use password: `Password123!`
- Assign appropriate branches (all except SuperAdmin)

### 3. Enhance Menu Items Seeding

- Add more main course items:
- Char Kway Teow
- Laksa
- Mee Goreng
- Rendang
- Satay
- Add more appetizers:
- Keropok Lekor
- Roti Jala
- Add more drinks:
- Kopi O / Coffee
- Sirap Bandung
- Lime Juice
- Soft Drinks (Coca Cola, Sprite)
- Fresh Fruit Juices

### 4. Add Test Customers

- Create a new `SeedCustomersAsync()` method
- Add 5-10 test customers with varied data
- Include PDPA consent fields

### Files to Modify

1. **`API/RestaurantOrderManagement.Shared/Constants/RoleConstants.cs`**

- Add `DeliveryStaff` constant

2. **`API/RestaurantOrderManagement.Infrastructure/Data/Seeders/DatabaseSeeder.cs`**

- Add DeliveryStaff role
- Add DeliveryStaff role permissions
- Expand `SeedUsersAsync()` to include all roles
- Expand `SeedMenuItemsAsync()` with more food and drinks
- Add `SeedCustomersAsync()` method

## Test Credentials Summary

After seeding, the following users will be available (password: `Password123!`):

| Username | Role | Branch | Description |
|----------|------|--------|-------------|
| superadmin | SuperAdmin | None | Full system access |
| admin | Admin | KL-001 | Branch administrator |
| manager | Manager | KL-001 | Branch manager |
| cashier | Cashier | KL-001 | Process orders/payments |
| waitstaff | Waitstaff | KL-001 | Table service |
| kitchen | KitchenStaff | KL-001 | Kitchen operations |
| delivery | DeliveryStaff | KL-001 | Delivery management |

## Menu Items Summary

After seeding:

- **Main Courses**: ~9 items (Nasi Lemak, Chicken Curry, Roti Canai, Char Kway Teow, Laksa, Mee Goreng, Rendang, Satay)
- **Appetizers**: ~3 items (Spring Rolls, Keropok Lekor, Roti Jala)
- **Drinks**: ~8 items (Teh Tarik, Ice Kacang, Kopi O, Sirap Bandung, Lime Juice, Coca Cola, Sprite, Fresh Juices)
- **Desserts**: ~1 item (Cendol)