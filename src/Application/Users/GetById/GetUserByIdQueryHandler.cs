using System.Data;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Dapper;
using Domain.Users;
using SharedKernel;

namespace Application.Users.GetById;

internal sealed class GetUserByIdQueryHandler(IDbConnectionFactory factory)
    : IQueryHandler<GetUserByIdQuery, UserResponse>
{
    public async Task<Result<UserResponse>> Handle(GetUserByIdQuery query, CancellationToken cancellationToken)
    {
        const string sql =
            """
            SELECT
                u.id AS Id,
                u.email AS Email,
                u.name AS Name,
                u.has_public_profile AS HasPublicProfile,
                r.id AS RoleId,
                r.name AS RoleName
            FROM users u
            LEFT JOIN role_user ru ON ru.users_id = u.id
            LEFT JOIN role r ON r.id = ru.roles_id
            WHERE u.id = @UserId
            """;

        using IDbConnection connection = factory.GetOpenConnection();

        Dictionary<Guid, UserResponse> userLookup = new();

        await connection.QueryAsync<UserResponse, UserRoleDto, UserResponse>(
            sql,
            (user, role) =>
            {
                if (!userLookup.TryGetValue(user.Id, out UserResponse? existing))
                {
                    existing = user;
                    userLookup.Add(existing.Id, existing);
                }

                if (role is not null && role.RoleId != 0)
                {
                    if (!existing.Roles.Any(existingRole => existingRole.RoleId == role.RoleId))
                    {
                        existing.Roles.Add(role);
                    }
                }

                return existing;
            },
            query,
            splitOn: "RoleId");

        UserResponse? user = userLookup.Values.FirstOrDefault();

        if (user is null)
        {
            return Result.Failure<UserResponse>(UserErrors.NotFound(query.UserId));
        }

        return user;
    }
}
