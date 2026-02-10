using Application.Abstractions.Messaging;

namespace Application.Gaming.Reports.Admin;

public sealed record GetWinningCostByDrawQuery(
    Guid TenantId,
    Guid DrawId,
    int DetailPage,
    int DetailPageSize) : IQuery<WinningCostPerDrawDto>;
