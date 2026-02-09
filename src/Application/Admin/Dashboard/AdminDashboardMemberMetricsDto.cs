namespace Application.Admin.Dashboard;

public sealed record AdminDashboardMemberMetricsDto(
    int RegisteredToday,
    int RegisteredThisWeek,
    int OnlineMembers,
    int ActiveToday,
    int ActiveThisWeek,
    DateTime TodayStartUtc,
    DateTime TodayEndUtc,
    DateTime WeekStartUtc,
    DateTime WeekEndUtc,
    DateTime OnlineWindowStartUtc,
    DateTime OnlineWindowEndUtc);
