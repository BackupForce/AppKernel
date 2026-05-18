namespace Application.Gaming.Draws.PrizePool;

public sealed record MissingPrizePoolSlotDto(
    string PlayTypeCode,
    string Tier);
