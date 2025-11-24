using OrderManagement.Application.Navigation.Models;

namespace OrderManagement.Application.Navigation;

public interface INavigationMenuService
{
    Task<IReadOnlyCollection<NavigationMenuItem>> GetMenuForRolesAsync(IEnumerable<string> roles, CancellationToken cancellationToken = default);
}

