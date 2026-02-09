namespace Application.Users.RemoveRole;

public sealed record RemoveUserRoleResultDto(Guid UserId, IReadOnlyList<int> RoleIds);
