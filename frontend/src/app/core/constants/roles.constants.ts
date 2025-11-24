export const AppRoles = {
    SuperAdmin: 'SuperAdmin',
    Admin: 'Admin',
    Manager: 'Manager',
    Waiter: 'Waiter',
    Kitchen: 'Kitchen',
    InventoryManager: 'InventoryManager'
} as const;

export type AppRole = typeof AppRoles[keyof typeof AppRoles];
