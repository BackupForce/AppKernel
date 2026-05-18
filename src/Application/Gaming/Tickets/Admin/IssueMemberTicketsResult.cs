namespace Application.Gaming.Tickets.Admin;

public sealed record IssueMemberTicketsResult(
    IReadOnlyCollection<IssuedTicketDto> Tickets);

public sealed record IssuedTicketDto(
    Guid TicketId,
    string Status,
    DateTime IssuedAtUtc,
    Guid DrawId,
    string GameCode,
    Guid IssuedByStaffUserId,
    string? Reason,
    string? Note);
