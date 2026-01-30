using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Gaming.Dtos;

namespace Application.Gaming.DrawGroups.List;

/// <summary>
/// 取得期數群組列表查詢。
/// </summary>
public sealed record ListDrawGroupsQuery(
    Guid TenantId,
    string? Status,
    string? GameCode,
    string? Keyword,
    int Page,
    int PageSize) : IQuery<PagedResult<DrawGroupSummaryDto>>;
