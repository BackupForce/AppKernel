namespace Application.Gaming.Dtos;

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
