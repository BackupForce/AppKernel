using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Gaming.Dtos;

namespace Application.Gaming.DrawGroups.RemoteSearch;

public sealed record RemoteSearchDrawGroupsQuery(
    Guid TenantId,
    string? Query,
    int Page,
    int PageSize) : IQuery<PagedResult<DrawGroupRemoteSearchDto>>;
