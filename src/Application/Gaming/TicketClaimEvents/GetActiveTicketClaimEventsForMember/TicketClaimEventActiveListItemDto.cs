namespace Application.Gaming.TicketClaimEvents.GetActiveTicketClaimEventsForMember;

public sealed record TicketClaimEventActiveListItemDto(
    Guid Id,
    string Name,
    DateTime StartsAtUtc,
    DateTime EndsAtUtc,
    int PerMemberQuota,
    int MemberClaimedCount,
    bool CanParticipate,
    string ScopeType,
    Guid ScopeId,
    Guid? TicketTemplateId);
