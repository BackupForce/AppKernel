using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Gaming.Dtos;

namespace Application.Gaming.Campaigns.List;

/// <summary>
/// 取得活動列表查詢。
/// </summary>
public sealed record ListCampaignsQuery(
    Guid TenantId,
    string? Status,
    string? GameCode,
    string? Keyword,
    int Page,
    int PageSize) : IQuery<PagedResult<CampaignSummaryDto>>;
