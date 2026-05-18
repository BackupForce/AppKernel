using Application.Abstractions.Messaging;
using Application.Gaming.Draws.PrizePool;
using Domain.Gaming.Draws;

namespace Application.Gaming.Draws.PrizePool.Commands;

public sealed record CloneDrawPrizePoolFromCommand(
    Guid DrawId,
    Guid SourceDrawId,
    ApplyMode Mode) : ICommand<DrawPrizePoolDto>;
