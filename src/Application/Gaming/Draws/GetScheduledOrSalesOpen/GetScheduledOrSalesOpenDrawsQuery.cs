using Application.Abstractions.Messaging;

namespace Application.Gaming.Draws.GetScheduledOrSalesOpen;

/// <summary>
/// 取得目前有效狀態為 Scheduled 或 SalesOpen 的期數清單。
/// </summary>
public sealed record GetScheduledOrSalesOpenDrawsQuery : IQuery<IReadOnlyCollection<ScheduledOrSalesOpenDrawDto>>;
