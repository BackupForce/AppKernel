using Application.Abstractions.Messaging;

namespace Application.Gaming.Draws.PrizePool.Update;

public sealed record UpdateDrawPrizePoolCommand(
    Guid DrawId,
    IReadOnlyCollection<UpdateDrawPrizePoolItem> Items) : ICommand<DrawPrizePoolDto>;
