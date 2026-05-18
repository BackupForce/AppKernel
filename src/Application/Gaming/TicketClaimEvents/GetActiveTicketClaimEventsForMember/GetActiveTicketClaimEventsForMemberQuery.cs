using Application.Abstractions.Messaging;

namespace Application.Gaming.TicketClaimEvents.GetActiveTicketClaimEventsForMember;

public sealed record GetActiveTicketClaimEventsForMemberQuery(
    Guid TenantId,
    Guid MemberId,
    DateTime? NowUtc = null) : IQuery<IReadOnlyCollection<TicketClaimEventActiveListItemDto>>;
