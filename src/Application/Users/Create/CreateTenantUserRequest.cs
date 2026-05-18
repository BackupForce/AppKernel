namespace Application.Users.Create;

public sealed record CreateTenantUserRequest(
    string Email,
    string Name,
    string Password,
    bool HasPublicProfile);
