namespace Application.Admin.Dashboard;

public sealed record OnlineMemberDto(
    Guid UserId,
    Guid MemberId,
    string UserName,
    string UserType,
    DateTime LastUsedAtUtc,
    string? Email);
