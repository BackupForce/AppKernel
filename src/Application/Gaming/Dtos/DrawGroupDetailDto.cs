namespace Application.Gaming.Dtos;

/// <summary>
/// 期數群組詳細資料，包含綁定期數。
/// </summary>
public sealed record DrawGroupDetailDto(
    Guid Id,
    string Name,
    string Status,
    string GameCode,
    string PlayTypeCode,
    DateTime GrantOpenAtUtc,
    DateTime GrantCloseAtUtc,
    int DrawCount,
    DateTime CreatedAtUtc,
    IReadOnlyCollection<DrawGroupDrawDto> Draws);
