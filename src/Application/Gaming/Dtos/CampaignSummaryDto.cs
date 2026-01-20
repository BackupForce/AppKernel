namespace Application.Gaming.Dtos;

/// <summary>
/// 活動摘要資料，用於列表顯示。
/// </summary>
public sealed record CampaignSummaryDto(
    Guid Id,
    string Name,
    string Status,
    string GameCode,
    string PlayTypeCode,
    DateTime GrantOpenAtUtc,
    DateTime GrantCloseAtUtc,
    DateTime CreatedAtUtc,
    long DrawCount);
