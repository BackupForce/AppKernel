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
    Guid PrizeId,
    string PrizeName,
    string Status,
    DateTime AwardedAt,
    DateTime? RedeemedAt,
    decimal? CostSnapshot);
