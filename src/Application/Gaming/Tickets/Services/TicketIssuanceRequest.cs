using Domain.Gaming.Catalog;
using Domain.Gaming.Shared;
using Domain.Gaming.Tickets;

namespace Application.Gaming.Tickets.Services;

internal sealed record TicketIssuanceRequest(
    Guid TenantId,
    GameCode GameCode,
    Guid MemberId,
    Guid? DrawGroupId,
    Guid? TicketTemplateId,
    Guid PrimaryDrawId,
    IssuedByType IssuedByType,
    Guid IssuedByUserId,
    string? IssuedReason,
    string? IssuedNote,
    DateTime NowUtc);
