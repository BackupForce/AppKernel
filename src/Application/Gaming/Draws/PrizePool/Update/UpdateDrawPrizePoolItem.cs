namespace Application.Gaming.Draws.PrizePool.Update;

public sealed record UpdateDrawPrizePoolItem(
    string PlayTypeCode,
    string Tier,
    PrizeOptionDto Option);
