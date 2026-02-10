namespace Application.Admin.Dashboard;

public sealed record OnlineMemberDto(
    Guid UserId,
    string UserName,
    string UserType,
    DateTime LastUsedAtUtc,
    string? Email);
