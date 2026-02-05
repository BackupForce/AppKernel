using Application.Abstractions.Messaging;

namespace Application.Gaming.Tickets.GetMyWinningTickets;

public sealed record GetMyWinningTicketsQuery(int Page, int PageSize) : IQuery<MyWinningTicketsDto>;
