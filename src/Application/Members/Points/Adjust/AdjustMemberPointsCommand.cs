using Application.Abstractions.Messaging;

namespace Application.Members.Points.Adjust;

public sealed record AdjustMemberPointsCommand(
    Guid MemberId,
    long Delta,
    string Remark,
    string ReferenceType = "admin_adjust",
    string? ReferenceId = null,
    bool AllowNegative = false) : ICommand<long>;
