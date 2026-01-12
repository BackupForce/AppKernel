using Application.Abstractions.Messaging;

namespace Application.Gaming.Prizes.Activate;

public sealed record ActivatePrizeCommand(Guid PrizeId) : ICommand;
