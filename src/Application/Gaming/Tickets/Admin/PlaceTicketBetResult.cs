namespace Application.Gaming.Tickets.Admin;

public sealed record PlaceTicketBetResult(
    Guid TicketId,
    string Status,
    DateTime SubmittedAtUtc,
    Guid SubmittedByStaffUserId,
    BetPayloadDto Bet);

public sealed record BetPayloadDto(
    string PlayTypeCode,
    IReadOnlyCollection<int> Numbers,
    string? ClientReference,
    string? Note);
