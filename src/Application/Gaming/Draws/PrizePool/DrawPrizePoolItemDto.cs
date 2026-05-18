namespace Application.Gaming.Draws.PrizePool;

public sealed record DrawPrizePoolItemDto(
    string PlayTypeCode,
    string Tier,
    bool IsConfigured,
    PrizeOptionDto? Option);
