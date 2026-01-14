namespace Application.Gaming.Dtos;

/// <summary>
/// 得獎資料傳輸物件，包含兌換與成本快照資訊。
/// </summary>
public sealed record PrizeAwardDto(
    Guid AwardId,
    Guid DrawId,
    Guid TicketId,
    int LineIndex,
    int MatchedCount,
    string GameCode,
    string PlayTypeCode,
    string PrizeTier,
    Guid PrizeId,
    string PrizeName,
    decimal PrizeCost,
    int? PrizeRedeemValidDays,
    string? PrizeDescription,
    string Status,
    DateTime AwardedAt,
    DateTime? ExpiresAt,
    DateTime? RedeemedAt);
