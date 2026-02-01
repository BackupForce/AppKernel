namespace Web.Api.Endpoints.Admin.Requests;

public sealed record CreateTicketClaimEventRequest(
    string Name,
    DateTime StartsAtUtc,
    DateTime EndsAtUtc,
    int TotalQuota,
    int PerMemberQuota,
    string ScopeType,
    Guid ScopeId,
    Guid? TicketTemplateId);

public sealed record UpdateTicketClaimEventRequest(
    string Name,
    DateTime StartsAtUtc,
    DateTime EndsAtUtc,
    int TotalQuota,
    int PerMemberQuota,
    string ScopeType,
    Guid ScopeId,
    Guid? TicketTemplateId);

public sealed record ListTicketClaimEventsRequest(
    string? Status,
    DateTime? StartsFromUtc,
    DateTime? StartsToUtc,
    DateTime? EndsFromUtc,
    DateTime? EndsToUtc,
    string? Keyword,
    int Page = 1,
    int PageSize = 20);

public sealed record GetTicketClaimEventClaimsRequest(
    Guid? MemberId,
    DateTime? ClaimedFromUtc,
    DateTime? ClaimedToUtc,
    int Page = 1,
    int PageSize = 20);
