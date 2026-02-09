using Application.Abstractions.Messaging;

namespace Application.Users.GetTenantUsers;

public sealed record GetTenantUsersQuery(Guid TenantId) : IQuery<IReadOnlyList<TenantUserListItemDto>>;
