using Domain.Users;

namespace Application.Users.GetTenantUsers;

public sealed record TenantUserListItemDto(Guid Id, string Name, string Email, UserType UserType, bool IsEnabled);
