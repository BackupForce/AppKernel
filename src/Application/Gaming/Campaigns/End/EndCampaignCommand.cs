using Application.Abstractions.Messaging;

namespace Application.Gaming.Campaigns.End;

/// <summary>
/// 結束活動命令。
/// </summary>
public sealed record EndCampaignCommand(Guid TenantId, Guid CampaignId) : ICommand;
