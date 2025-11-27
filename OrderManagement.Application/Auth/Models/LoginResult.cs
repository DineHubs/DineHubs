namespace OrderManagement.Application.Auth.Models;

public record LoginResult(string AccessToken, IReadOnlyCollection<string> Roles);

