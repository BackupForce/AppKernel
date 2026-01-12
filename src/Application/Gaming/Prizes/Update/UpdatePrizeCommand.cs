using Application.Abstractions.Messaging;

namespace Application.Gaming.Prizes.Update;

public sealed record UpdatePrizeCommand(
    Guid PrizeId,
    string Name,
    string? Description,
    decimal Cost) : ICommand;
