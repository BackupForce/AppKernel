namespace Application.Gaming.Dtos;

/// <summary>
/// 活動綁定期數資料。
/// </summary>
public sealed record CampaignDrawDto(
    Guid DrawId,
    DateTime CreatedAtUtc);
