using Application.Abstractions.Data;
using Application.Abstractions.Messaging;

namespace Application.Users.GetTenantUsers;

public sealed record GetTenantUsersQuery(Guid TenantId, int Page, int PageSize) : IQuery<PagedResult<TenantUserListItemDto>>;
