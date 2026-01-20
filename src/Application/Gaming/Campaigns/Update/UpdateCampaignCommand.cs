using Application.Abstractions.Messaging;

namespace Application.Gaming.Campaigns.Update;

/// <summary>
/// 更新活動基本資料命令。
/// </summary>
public sealed record UpdateCampaignCommand(
    Guid TenantId,
    Guid CampaignId,
    string Name,
    DateTime GrantOpenAtUtc,
    DateTime GrantCloseAtUtc) : ICommand;
