using Application.Abstractions.Messaging;
using Application.Gaming.Dtos;
using SharedKernel;

namespace Application.Gaming.TicketClaimEvents.List;

public sealed record ListTicketClaimEventsQuery(
    Guid TenantId,
    string? Status,
    DateTime? StartsFromUtc,
    DateTime? StartsToUtc,
    DateTime? EndsFromUtc,
    DateTime? EndsToUtc,
    string? Keyword,
    int Page,
    int PageSize) : IQuery<PagedResult<TicketClaimEventSummaryDto>>;
