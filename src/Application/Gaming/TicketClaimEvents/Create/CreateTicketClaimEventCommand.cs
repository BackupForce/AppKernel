using Application.Abstractions.Messaging;
using Domain.Gaming.TicketClaimEvents;

namespace Application.Gaming.TicketClaimEvents.Create;

public sealed record CreateTicketClaimEventCommand(
    Guid TenantId,
    string Name,
    DateTime StartsAtUtc,
    DateTime EndsAtUtc,
    int TotalQuota,
    int PerMemberQuota,
    TicketClaimEventScopeType ScopeType,
    Guid ScopeId,
    Guid? TicketTemplateId) : ICommand<Guid>;
