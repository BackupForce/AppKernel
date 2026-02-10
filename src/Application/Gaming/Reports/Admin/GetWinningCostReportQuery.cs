using Application.Abstractions.Messaging;

namespace Application.Gaming.Reports.Admin;

public sealed record GetWinningCostReportQuery(
    Guid TenantId,
    DateTime FromUtc,
    DateTime ToUtc,
    string? GameCode,
    Guid? DrawGroupId,
    bool IncludeDetails,
    int Page,
    int PageSize) : IQuery<WinningCostReportPageDto>;
