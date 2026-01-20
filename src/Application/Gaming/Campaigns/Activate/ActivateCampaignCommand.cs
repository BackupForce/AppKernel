using Application.Abstractions.Messaging;

namespace Application.Gaming.Campaigns.Activate;

/// <summary>
/// 啟用活動命令。
/// </summary>
public sealed record ActivateCampaignCommand(Guid TenantId, Guid CampaignId) : ICommand;
