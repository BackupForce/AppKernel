using Domain.Gaming.Shared;
using Domain.Gaming.Tickets;

namespace Application.Gaming.Tickets.Services;

internal sealed record TicketIssuanceRequest(
    Guid TenantId,
    GameCode GameCode,
    Guid MemberId,
    Guid? CampaignId,
    Guid? TicketTemplateId,
    Guid? PrimaryDrawId,
    IReadOnlyCollection<Guid> DrawIds,
    IssuedByType IssuedByType,
    Guid IssuedByUserId,
    string? IssuedReason,
    string? IssuedNote,
    DateTime NowUtc);
