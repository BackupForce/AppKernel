namespace Application.Gaming.Tickets.Admin;

public sealed record PlaceTicketBetResult(
    Guid TicketId,
    string Status,
    DateTime SubmittedAtUtc,
    Guid SubmittedByStaffUserId,
    BetPayloadDto Bet);

public sealed record BetPayloadDto(
    IReadOnlyCollection<int> Numbers,
    string? ClientReference,
    string? Note);
