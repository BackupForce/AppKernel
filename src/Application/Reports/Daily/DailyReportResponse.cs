namespace Application.Reports.Daily;

public sealed record DailyReportResponse(
    Guid TenantId,
    DateOnly LocalDate,
    string TimeZoneId,
    DateTimeOffset StartUtc,
    DateTimeOffset EndUtc,
    int NewMembers,
    int TicketsCreated);
