namespace Application.Users.AssignRole;

public sealed record AssignRoleToUserResultDto(Guid UserId, IReadOnlyList<int> RoleIds);
