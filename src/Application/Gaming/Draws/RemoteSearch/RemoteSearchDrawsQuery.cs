using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Gaming.Dtos;

namespace Application.Gaming.Draws.RemoteSearch;

public sealed record RemoteSearchDrawsQuery(
    Guid TenantId,
    string? Query,
    int Page,
    int PageSize) : IQuery<PagedResult<DrawRemoteSearchDto>>;
