using Application.Abstractions.Messaging;
using Application.Gaming.Dtos;
using SharedKernel;

namespace Application.Gaming.TicketClaimEvents.Claims;

public sealed record GetTicketClaimEventClaimsQuery(
    Guid TenantId,
    Guid EventId,
    Guid? MemberId,
    DateTime? ClaimedFromUtc,
    DateTime? ClaimedToUtc,
    int Page,
    int PageSize) : IQuery<PagedResult<TicketClaimRecordDto>>;
