namespace Application.Admin.Dashboard;

public sealed record AdminDashboardMemberMetricsDto(
    long RegisteredToday,
    long RegisteredThisWeek,
    int OnlineMembers,
    long ActiveToday,
    long ActiveThisWeek,
    DateTime TodayStartUtc,
    DateTime TodayEndUtc,
    DateTime WeekStartUtc,
    DateTime WeekEndUtc,
    DateTime OnlineWindowStartUtc,
    DateTime OnlineWindowEndUtc);
