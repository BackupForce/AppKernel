using Application.Abstractions.Messaging;
using Application.Gaming.Dtos;

namespace Application.Gaming.Campaigns.GetById;

/// <summary>
/// 取得活動詳細資訊查詢。
/// </summary>
public sealed record GetCampaignByIdQuery(Guid TenantId, Guid CampaignId) : IQuery<CampaignDetailDto>;
