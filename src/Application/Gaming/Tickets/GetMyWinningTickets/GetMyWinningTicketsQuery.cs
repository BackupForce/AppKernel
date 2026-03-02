using Application.Abstractions.Data;
using Application.Abstractions.Messaging;

namespace Application.Gaming.Tickets.GetMyWinningTickets;

public sealed record GetMyWinningTicketsQuery(int PageNumber, int PageSize) : IQuery<PagedResult<MyWinningTicketItemDto>>;
