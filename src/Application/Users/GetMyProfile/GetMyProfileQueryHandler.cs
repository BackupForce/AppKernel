using System.Data;
using System.Text.Json;
using Application.Abstractions.Authentication;
using Application.Abstractions.Authorization;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Dapper;
using Domain.Users;
using SharedKernel;

namespace Application.Users.GetMyProfile;

internal sealed class GetMyProfileQueryHandler(
    IDbConnectionFactory factory,
    IUserContext userContext,
    ITenantContext tenantContext)
    : IQueryHandler<GetMyProfileQuery, MyProfileDto>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<Result<MyProfileDto>> Handle(GetMyProfileQuery query, CancellationToken cancellationToken)
    {
        const string sql =
            """
            SELECT
                u.id AS Id,
                u.email AS Email,
                u.name AS Name,
                u.has_public_profile AS HasPublicProfile,
                u.is_enabled AS IsEnabled,
                u.type AS UserType,
                u.tenant_id AS TenantId,
                COALESCE(
                    json_agg(
                        json_build_object(
                            'RoleId', r.id,
                            'RoleName', r.name
                        )
                    ) FILTER (WHERE r.id IS NOT NULL),
                    '[]'::json
                )::text AS RolesJson
            FROM users u
            LEFT JOIN role_user ru ON ru.users_id = u.id
            LEFT JOIN role r ON r.id = ru.roles_id
            WHERE u.id = @UserId
              AND u.tenant_id = @TenantId
              AND u.type = @UserType
            GROUP BY u.id, u.email, u.name, u.has_public_profile, u.is_enabled, u.type, u.tenant_id
            """;

        using IDbConnection connection = factory.GetOpenConnection();

        UserProfileDataModel? data = await connection.QuerySingleOrDefaultAsync<UserProfileDataModel>(
            sql,
            new
            {
                userContext.UserId,
                tenantContext.TenantId,
                UserType = UserType.Tenant
            });

        if (data is null)
        {
            return Result.Failure<MyProfileDto>(UserErrors.NotFound(userContext.UserId));
        }

        return new MyProfileDto
        {
            Id = data.Id,
            Email = data.Email,
            Name = data.Name,
            HasPublicProfile = data.HasPublicProfile,
            IsEnabled = data.IsEnabled,
            UserType = data.UserType,
            TenantId = data.TenantId,
            Roles = DeserializeRoles(data.RolesJson)
        };
    }

    private static List<MyProfileRoleDto> DeserializeRoles(string? rolesJson)
    {
        if (string.IsNullOrWhiteSpace(rolesJson))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<MyProfileRoleDto>>(rolesJson, JsonOptions) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private sealed record UserProfileDataModel(
        Guid Id,
        string Email,
        string Name,
        bool HasPublicProfile,
        bool IsEnabled,
        UserType UserType,
        Guid? TenantId,
        string RolesJson);
}
