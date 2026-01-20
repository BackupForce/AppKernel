using Application.Abstractions.Messaging;

namespace Application.Gaming.Campaigns.Create;

/// <summary>
/// 建立活動的命令。
/// </summary>
public sealed record CreateCampaignCommand(
    Guid TenantId,
    string GameCode,
    string PlayTypeCode,
    string Name,
    DateTime GrantOpenAtUtc,
    DateTime GrantCloseAtUtc) : ICommand<Guid>;
