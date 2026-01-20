using Application.Abstractions.Messaging;

namespace Application.Gaming.Campaigns.Draws.Remove;

/// <summary>
/// 移除活動期數命令。
/// </summary>
public sealed record RemoveCampaignDrawCommand(
    Guid TenantId,
    Guid CampaignId,
    Guid DrawId) : ICommand;
