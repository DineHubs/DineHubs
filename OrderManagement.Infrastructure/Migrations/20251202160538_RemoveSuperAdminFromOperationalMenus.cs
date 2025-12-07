using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSuperAdminFromOperationalMenus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Remove SuperAdmin role from operational menu items
            // This aligns the navigation menu with the SuperAdmin role focus on tenant oversight
            
            migrationBuilder.Sql(@"
                DELETE FROM ""MenuPermissions""
                WHERE ""RoleName"" = 'SuperAdmin'
                AND ""MenuItemId"" IN (
                    SELECT ""Id"" FROM ""NavigationMenuItems""
                    WHERE ""Label"" IN ('Orders', 'Menu Management', 'Kitchen', 'Inventory')
                    OR ""ParentId"" IN (
                        SELECT ""Id"" FROM ""NavigationMenuItems""
                        WHERE ""Label"" IN ('Orders', 'Menu Management', 'Kitchen', 'Inventory')
                    )
                );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Restore SuperAdmin role to operational menu items
            // This rollback adds SuperAdmin back to Orders, Menu Management, Kitchen, and Inventory
            
            migrationBuilder.Sql(@"
                INSERT INTO ""MenuPermissions"" (""MenuItemId"", ""RoleName"")
                SELECT ""Id"", 'SuperAdmin'
                FROM ""NavigationMenuItems""
                WHERE ""Label"" IN ('Orders', 'Menu Management', 'Kitchen', 'Inventory')
                OR ""ParentId"" IN (
                    SELECT ""Id"" FROM ""NavigationMenuItems""
                    WHERE ""Label"" IN ('Orders', 'Menu Management', 'Kitchen', 'Inventory')
                )
                AND NOT EXISTS (
                    SELECT 1 FROM ""MenuPermissions"" mp
                    WHERE mp.""MenuItemId"" = ""NavigationMenuItems"".""Id""
                    AND mp.""RoleName"" = 'SuperAdmin'
                );
            ");
        }
    }
}
