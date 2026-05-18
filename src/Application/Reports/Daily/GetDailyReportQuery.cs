using Application.Abstractions.Messaging;

namespace Application.Reports.Daily;

public sealed record GetDailyReportQuery(Guid TenantId, DateOnly Date) : IQuery<DailyReportResponse>;
