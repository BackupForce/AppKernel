namespace Application.Users.RemoveGroup;

public sealed record RemoveGroupFromUserResultDto(Guid UserId, IReadOnlyList<Guid> GroupIds);
