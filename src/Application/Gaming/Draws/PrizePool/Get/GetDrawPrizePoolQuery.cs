using Application.Abstractions.Messaging;

namespace Application.Gaming.Draws.PrizePool.Get;

public sealed record GetDrawPrizePoolQuery(Guid DrawId) : IQuery<DrawPrizePoolDto>;
