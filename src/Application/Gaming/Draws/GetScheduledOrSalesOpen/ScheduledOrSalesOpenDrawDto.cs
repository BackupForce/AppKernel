namespace Application.Gaming.Draws.GetScheduledOrSalesOpen;

/// <summary>
/// Scheduled / SalesOpen 期數清單項目。
/// </summary>
public sealed record ScheduledOrSalesOpenDrawDto(
    Guid Id,
    string GameCode,
    string DrawCode,
    DateTime SalesOpenAt,
    DateTime SalesCloseAt,
    DateTime DrawAt,
    string EffectiveStatus);
