namespace Application.Gaming.Draws.PrizePool;

public sealed record DrawPrizePoolDto(
    Guid DrawId,
    string GameCode,
    IReadOnlyCollection<DrawPrizePoolItemDto> Items);
