using Application.Abstractions.Messaging;

namespace Application.Gaming.Tickets.Cancel;

public sealed record CancelTicketCommand(Guid TicketId, string? Reason) : ICommand;
