using Application.Abstractions.Messaging;
using Application.Gaming.Dtos;

namespace Application.Gaming.TicketClaimEvents.GetById;

public sealed record GetTicketClaimEventQuery(Guid TenantId, Guid EventId) : IQuery<TicketClaimEventDetailDto>;
