using Application.Abstractions.Messaging;
using Application.Gaming.Dtos;

namespace Application.Gaming.Tickets.Admin;

public sealed record GetMemberAvailableTicketsForBetQuery(Guid MemberId, Guid? DrawId, int? Limit)
    : IQuery<AvailableTicketsResponse>;
