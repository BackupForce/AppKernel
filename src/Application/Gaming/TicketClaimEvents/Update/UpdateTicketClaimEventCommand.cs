using Application.Abstractions.Messaging;
using Domain.Gaming.TicketClaimEvents;

namespace Application.Gaming.TicketClaimEvents.Update;

public sealed record UpdateTicketClaimEventCommand(
    Guid TenantId,
    Guid EventId,
    string Name,
    DateTime StartsAtUtc,
    DateTime EndsAtUtc,
    int TotalQuota,
    int PerMemberQuota,
    TicketClaimEventScopeType ScopeType,
    Guid ScopeId,
    Guid? TicketTemplateId) : ICommand;
