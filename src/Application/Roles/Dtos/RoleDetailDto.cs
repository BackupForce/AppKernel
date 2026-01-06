namespace Application.Roles.Dtos;

public sealed record RoleDetailDto(
    int Id,
    string Name,
    IReadOnlyList<string> PermissionCodes);
