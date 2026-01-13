namespace Application.Gaming.Dtos;

/// <summary>
/// 中獎規則資料傳輸物件。
/// </summary>
public sealed record PrizeRuleDto(
    Guid Id,
    int MatchCount,
    Guid PrizeId,
    string PrizeName,
    bool IsActive,
    DateTime? EffectiveFrom,
    DateTime? EffectiveTo,
    int? RedeemValidDays);
