namespace Application.Roles.Dtos;

public sealed record RoleListItemDto(
    int Id,
    string Name,
    int PermissionCount);
