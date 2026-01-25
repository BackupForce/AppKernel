using Application.Abstractions.Messaging;
using Application.Gaming.Dtos;

namespace Application.Gaming.Tickets.AvailableForBet;

public sealed record GetAvailableTicketsForBetQuery(Guid TenantId, Guid MemberId, Guid? DrawId, int? Limit)
    : IQuery<AvailableTicketsResponse>;
