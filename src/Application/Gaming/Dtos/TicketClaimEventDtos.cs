namespace Application.Gaming.Dtos;

public sealed record TicketClaimEventSummaryDto(
    Guid Id,
    string Name,
    string Status,
    DateTime StartsAtUtc,
    DateTime EndsAtUtc,
    int TotalQuota,
    int TotalClaimed,
    int PerMemberQuota,
    string ScopeType,
    Guid ScopeId,
    Guid? TicketTemplateId,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public sealed record TicketClaimEventDetailDto(
    Guid Id,
    string Name,
    string Status,
    DateTime StartsAtUtc,
    DateTime EndsAtUtc,
    int TotalQuota,
    int TotalClaimed,
    int PerMemberQuota,
    string ScopeType,
    Guid ScopeId,
    Guid? TicketTemplateId,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public sealed record TicketClaimRecordDto(
    Guid Id,
    Guid MemberId,
    int Quantity,
    DateTime ClaimedAtUtc,
    IReadOnlyCollection<Guid> IssuedTicketIds);
