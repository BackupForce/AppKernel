using Application.Abstractions.Messaging;

namespace Application.Gaming.Tickets.Redeem;

public sealed record RedeemTicketLineCommand(Guid TicketLineResultId) : ICommand<Guid>;
