using Application.Abstractions.Messaging;

namespace Application.Gaming.Prizes.Deactivate;

public sealed record DeactivatePrizeCommand(Guid PrizeId) : ICommand;
