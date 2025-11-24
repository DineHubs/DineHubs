namespace OrderManagement.Domain.Identity;

public static class SystemRoles
{
    public const string SuperAdmin = "SuperAdmin";
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string Waiter = "Waiter";
    public const string Kitchen = "Kitchen";
    public const string InventoryManager = "InventoryManager";

    public static readonly IReadOnlyCollection<string> All =
    [
        SuperAdmin,
        Admin,
        Manager,
        Waiter,
        Kitchen,
        InventoryManager
    ];
}


