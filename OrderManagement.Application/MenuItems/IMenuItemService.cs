using OrderManagement.Application.MenuItems.Models;

namespace OrderManagement.Application.MenuItems;

public interface IMenuItemService
{
    Task<IReadOnlyCollection<MenuItemDto>> GetMenuItemsAsync(Guid? branchId, CancellationToken cancellationToken = default);
    Task<MenuItemDto?> GetMenuItemByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<MenuItemDto> CreateMenuItemAsync(CreateMenuItemDto dto, CancellationToken cancellationToken = default);
    Task UpdateMenuItemAsync(Guid id, UpdateMenuItemDto dto, CancellationToken cancellationToken = default);
    Task DeleteMenuItemAsync(Guid id, CancellationToken cancellationToken = default);
}

