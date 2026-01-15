using Application.Abstractions.Messaging;
using Application.Gaming.Draws.PrizePool;
using Domain.Gaming.Draws;

namespace Application.Gaming.Draws.PrizePool.Commands;

public sealed record ApplyPrizePoolTemplateCommand(
    Guid DrawId,
    Guid TemplateId,
    ApplyMode Mode) : ICommand<DrawPrizePoolDto>;
