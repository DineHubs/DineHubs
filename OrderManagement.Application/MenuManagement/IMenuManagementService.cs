using OrderManagement.Application.MenuManagement.Models;

namespace OrderManagement.Application.MenuManagement;

public interface IMenuManagementService
{
    Task<IReadOnlyCollection<MenuManagementItemDto>> GetAllMenuItemsAsync(CancellationToken cancellationToken = default);
    Task<MenuManagementItemDto?> GetMenuItemByIdAsync(Guid menuItemId, CancellationToken cancellationToken = default);
    Task UpdateMenuPermissionsAsync(Guid menuItemId, IReadOnlyCollection<string> allowedRoles, CancellationToken cancellationToken = default);
    Task UpdateDisplayOrderAsync(Guid menuItemId, int displayOrder, CancellationToken cancellationToken = default);
    Task ToggleMenuItemAsync(Guid menuItemId, bool isActive, CancellationToken cancellationToken = default);
}

