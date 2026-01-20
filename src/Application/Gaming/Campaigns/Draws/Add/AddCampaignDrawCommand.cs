using Application.Abstractions.Messaging;

namespace Application.Gaming.Campaigns.Draws.Add;

/// <summary>
/// 綁定活動期數命令。
/// </summary>
public sealed record AddCampaignDrawCommand(
    Guid TenantId,
    Guid CampaignId,
    Guid DrawId) : ICommand;
