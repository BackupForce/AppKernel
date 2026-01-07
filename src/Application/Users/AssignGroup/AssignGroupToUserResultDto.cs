namespace Application.Users.AssignGroup;

public sealed record AssignGroupToUserResultDto(Guid UserId, IReadOnlyList<Guid> GroupIds);
