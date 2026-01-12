using Application.Abstractions.Messaging;
using Application.Gaming.Dtos;

namespace Application.Gaming.Tickets.GetMy;

public sealed record GetMyTicketsQuery(DateTime? From, DateTime? To) : IQuery<IReadOnlyCollection<TicketSummaryDto>>;
