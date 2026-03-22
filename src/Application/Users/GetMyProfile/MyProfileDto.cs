using Domain.Users;

namespace Application.Users.GetMyProfile;

public sealed record MyProfileDto
{
    public Guid Id { get; init; }

    public string Email { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public bool HasPublicProfile { get; init; }

    public bool IsEnabled { get; init; }

    public UserType UserType { get; init; }

    public Guid? TenantId { get; init; }

    public List<MyProfileRoleDto> Roles { get; init; } = new();
}

public sealed record MyProfileRoleDto
{
    public int RoleId { get; init; }

    public string RoleName { get; init; } = string.Empty;
}
