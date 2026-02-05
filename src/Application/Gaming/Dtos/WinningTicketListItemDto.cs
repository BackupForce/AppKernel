using Domain.Gaming.Tickets;

namespace Application.Gaming.Dtos;

public sealed record WinningTicketListItemDto(
    Guid DrawId,
    string DrawCode,
    DateTime DrawAtUtc,
    Guid? DrawGroupId,
    string? DrawGroupName,
    string GameCode,
    Guid TicketId,
    Guid MemberId,
    string MemberNo,
    string? MemberDisplayName,
    TicketDrawParticipationStatus ParticipationStatus,
    string? WinningTier,
    decimal? PrizeAmount,
    DateTime? RedeemedAtUtc,
    DateTime IssuedAtUtc,
    DateTime? SubmittedAtUtc);
