using Application.Abstractions.Messaging;

namespace Application.Gaming.Tickets.Redeem;

public sealed record RedeemTicketDrawCommand(Guid TicketId, Guid DrawId) : ICommand;
