using Application.Abstractions.Messaging;

namespace Application.Gaming.Tickets.Admin;

public sealed record IssueMemberTicketsCommand(
    Guid MemberId,
    string GameCode,
    string PlayTypeCode,
    Guid DrawId,
    int Quantity,
    string? Reason,
    string? Note,
    string? IdempotencyKey) : ICommand<IssueMemberTicketsResult>;
