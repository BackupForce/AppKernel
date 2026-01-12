using Application.Abstractions.Messaging;

namespace Application.Gaming.Draws.Settle;

public sealed record SettleDrawCommand(Guid DrawId) : ICommand;
