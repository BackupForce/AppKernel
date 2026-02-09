using System.Data;
using System.Globalization;
using System.Text;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Dapper;
using Domain.Users;
using SharedKernel;

namespace Application.Users.GetTenantUsers;

internal sealed class GetTenantUsersQueryHandler(IDbConnectionFactory factory)
    : IQueryHandler<GetTenantUsersQuery, PagedResult<TenantUserListItemDto>>
{
    public async Task<Result<PagedResult<TenantUserListItemDto>>> Handle(
        GetTenantUsersQuery query,
        CancellationToken cancellationToken)
    {
        var builder = new StringBuilder(
            """
            SELECT
                u.id AS Id,
                u.name AS Name,
                u.email AS Email,
                u.type AS UserType
            FROM users u
            WHERE u.tenant_id = @TenantId
              AND u.type = @UserType
            """;

        const string countSql = "SELECT COUNT(*) FROM ({0}) AS counted";
        string baseSql = builder.ToString();
        string finalSql = $"{baseSql} ORDER BY u.name LIMIT @PageSize OFFSET @Offset";

        var parameters = new DynamicParameters();
        parameters.Add("TenantId", query.TenantId);
        parameters.Add("UserType", UserType.Tenant);
        parameters.Add("PageSize", query.PageSize);
        parameters.Add("Offset", (query.Page - 1) * query.PageSize);

        using IDbConnection connection = factory.GetOpenConnection();

        IEnumerable<TenantUserListItemDto> users = await connection.QueryAsync<TenantUserListItemDto>(
            new CommandDefinition(finalSql, parameters, cancellationToken: cancellationToken));
        int totalCount = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(
                string.Format(CultureInfo.InvariantCulture, countSql, baseSql),
                parameters,
                cancellationToken: cancellationToken));

        return PagedResult<TenantUserListItemDto>.Create(users, totalCount, query.Page, query.PageSize);
    }
}
