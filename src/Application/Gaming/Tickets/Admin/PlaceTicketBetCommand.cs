using Application.Abstractions.Messaging;

namespace Application.Gaming.Tickets.Admin;

public sealed record PlaceTicketBetCommand(
    Guid TicketId,
    IReadOnlyCollection<int> Numbers,
    string? ClientReference,
    string? Note,
    string? IdempotencyKey) : ICommand<PlaceTicketBetResult>;
