using Application.Abstractions.Messaging;

namespace Application.Gaming.Prizes.Create;

public sealed record CreatePrizeCommand(
    string Name,
    string? Description,
    decimal Cost) : ICommand<Guid>;
