using Application.Abstractions.Messaging;

namespace Application.Gaming.Campaigns.Delete;

/// <summary>
/// 刪除活動命令。
/// </summary>
public sealed record DeleteCampaignCommand(Guid TenantId, Guid CampaignId) : ICommand;
