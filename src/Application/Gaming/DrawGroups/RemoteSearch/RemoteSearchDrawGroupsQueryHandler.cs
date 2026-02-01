using System.Data;
using System.Globalization;
using System.Text;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Gaming.Dtos;
using Dapper;
using Domain.Gaming.Shared;
using SharedKernel;

namespace Application.Gaming.DrawGroups.RemoteSearch;

internal sealed class RemoteSearchDrawGroupsQueryHandler(
    IDbConnectionFactory dbConnectionFactory,
    ITenantContext tenantContext) : IQueryHandler<RemoteSearchDrawGroupsQuery, PagedResult<DrawGroupRemoteSearchDto>>
{
    public async Task<Result<PagedResult<DrawGroupRemoteSearchDto>>> Handle(RemoteSearchDrawGroupsQuery request, CancellationToken cancellationToken)
    {
        if (request.TenantId != tenantContext.TenantId)
        {
            return Result.Failure<PagedResult<DrawGroupRemoteSearchDto>>(GamingErrors.DrawGroupTenantMismatch);
        }

        var builder = new StringBuilder(
            """
            SELECT
                c.id AS Id,
                c.name AS Name
            FROM gaming.draw_groups c
            WHERE c.tenant_id = @TenantId
            """);

        var parameters = new DynamicParameters();
        parameters.Add("TenantId", request.TenantId);

        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            builder.Append(" AND c.name ILIKE @Keyword");
            parameters.Add("Keyword", $"%{request.Query.Trim()}%");
        }

        const string countSql = "SELECT COUNT(*) FROM ({0}) AS counted";
        string baseSql = builder.ToString();
        string finalSql = $"{baseSql} ORDER BY c.name ASC LIMIT @PageSize OFFSET @Offset";

        parameters.Add("PageSize", request.PageSize);
        parameters.Add("Offset", (request.Page - 1) * request.PageSize);

        using IDbConnection connection = dbConnectionFactory.GetOpenConnection();

        IEnumerable<DrawGroupRemoteSearchDto> items = await connection.QueryAsync<DrawGroupRemoteSearchDto>(finalSql, parameters);
        int totalCount = await connection.ExecuteScalarAsync<int>(
            string.Format(CultureInfo.InvariantCulture, countSql, baseSql),
            parameters);

        return PagedResult<DrawGroupRemoteSearchDto>.Create(items, totalCount, request.Page, request.PageSize);
    }
}
