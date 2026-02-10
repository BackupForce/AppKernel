using Application.Abstractions.Data;
using Application.Abstractions.Messaging;

namespace Application.Admin.Dashboard;

public sealed record GetOnlineMembersQuery(
    int Page,
    int PageSize,
    int WindowMinutes,
    string? Q) : IQuery<PagedResponse<OnlineMemberDto>>;
