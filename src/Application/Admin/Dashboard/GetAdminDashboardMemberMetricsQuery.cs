using Application.Abstractions.Messaging;

namespace Application.Admin.Dashboard;

public sealed record GetAdminDashboardMemberMetricsQuery : IQuery<AdminDashboardMemberMetricsDto>;
