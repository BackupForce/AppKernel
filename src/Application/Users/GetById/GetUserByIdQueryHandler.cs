using System.Data;
using System.Text.Json;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Dapper;
using Domain.Users;
using SharedKernel;

namespace Application.Users.GetById;

internal sealed class GetUserByIdQueryHandler(IDbConnectionFactory factory)
    : IQueryHandler<GetUserByIdQuery, UserResponse>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<Result<UserResponse>> Handle(GetUserByIdQuery query, CancellationToken cancellationToken)
    {
        const string sql =
            """
            SELECT
                u.id AS Id,
                u.email AS Email,
                u.name AS Name,
                u.has_public_profile AS HasPublicProfile,
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
            GROUP BY u.id, u.email, u.name, u.has_public_profile
            """;

        using IDbConnection connection = factory.GetOpenConnection();

        UserDataModel? data = await connection.QuerySingleOrDefaultAsync<UserDataModel>(sql, query);

        if (data is null)
        {
            return Result.Failure<UserResponse>(UserErrors.NotFound(query.UserId));
        }

        List<UserRoleDto> roles = DeserializeRoles(data.RolesJson);

        return new UserResponse
        {
            Id = data.Id,
            Email = data.Email,
            Name = data.Name,
            HasPublicProfile = data.HasPublicProfile,
            Roles = roles
        };
    }

    private static List<UserRoleDto> DeserializeRoles(string? rolesJson)
    {
        if (string.IsNullOrWhiteSpace(rolesJson))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<UserRoleDto>>(rolesJson, JsonOptions) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private sealed record UserDataModel(
        Guid Id,
        string Email,
        string Name,
        bool HasPublicProfile,
        string RolesJson);
}
