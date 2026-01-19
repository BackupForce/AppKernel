using Domain.Gaming.Tickets;

namespace Application.Gaming.Dtos;

public sealed record TicketDrawSummaryDto(
    Guid DrawId,
    DateTime DrawAt,
    TicketDrawParticipationStatus ParticipationStatus,
    int MatchedCount);
