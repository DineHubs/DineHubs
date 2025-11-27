namespace OrderManagement.Api.Contracts.Auth;

public record ResetPasswordRequest(string Email, string Token, string NewPassword);

