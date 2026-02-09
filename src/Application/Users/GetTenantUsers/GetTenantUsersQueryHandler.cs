using System.Data;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Dapper;
using Domain.Users;
using SharedKernel;

namespace Application.Users.GetTenantUsers;

internal sealed class GetTenantUsersQueryHandler(IDbConnectionFactory factory)
    : IQueryHandler<GetTenantUsersQuery, IReadOnlyList<TenantUserListItemDto>>
{
    public async Task<Result<IReadOnlyList<TenantUserListItemDto>>> Handle(
        GetTenantUsersQuery query,
        CancellationToken cancellationToken)
    {
        const string sql =
            """
            SELECT
                u.id AS Id,
                u.name AS Name,
                u.email AS Email,
                u.type AS UserType
            FROM users u
            WHERE u.tenant_id = @TenantId
              AND u.type = @UserType
            ORDER BY u.name
            """;

        using IDbConnection connection = factory.GetOpenConnection();

        IReadOnlyList<TenantUserListItemDto> users = (await connection.QueryAsync<TenantUserListItemDto>(
            new CommandDefinition(
                sql,
                new
                {
                    query.TenantId,
                    UserType = UserType.Tenant
                },
                cancellationToken: cancellationToken))).ToList();

        return users;
    }
}
