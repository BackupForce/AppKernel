using Application.Abstractions.Messaging;
using Application.Gaming.Dtos;

namespace Application.Gaming.Tickets.GetMy;

/// <summary>
/// 取得會員票券列表查詢。
/// </summary>
public sealed record GetMyTicketsQuery(DateTime? From, DateTime? To) : IQuery<IReadOnlyCollection<TicketSummaryDto>>;
