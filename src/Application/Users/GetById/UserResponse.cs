namespace Application.Users.GetById;

public sealed record UserResponse
{
    public Guid Id { get; init; }

    public string Email { get; init; }

    public string Name { get; init; }

    public bool HasPublicProfile { get; init; }

    public bool IsEnabled { get; init; }

    public List<UserRoleDto> Roles { get; set; } = new();
}

public sealed record UserRoleDto
{
    public int RoleId { get; init; }

    public string RoleName { get; init; } = string.Empty;
}
