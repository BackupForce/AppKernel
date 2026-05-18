namespace Application.Gaming.Dtos;

/// <summary>
/// 期數獎項對應資料傳輸物件。
/// </summary>
public sealed record DrawPrizeMappingDto(
    int MatchCount,
    IReadOnlyCollection<DrawPrizeMappingPrizeDto> Prizes);

/// <summary>
/// 期數獎項對應的獎品資料。
/// </summary>
public sealed record DrawPrizeMappingPrizeDto(
    Guid PrizeId,
    string PrizeName,
    decimal PrizeCost,
    bool IsActive);
