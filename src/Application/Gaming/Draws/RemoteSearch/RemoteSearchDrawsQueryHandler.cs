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

namespace Application.Gaming.Draws.RemoteSearch;

internal sealed class RemoteSearchDrawsQueryHandler(
    IDbConnectionFactory dbConnectionFactory,
    ITenantContext tenantContext) : IQueryHandler<RemoteSearchDrawsQuery, PagedResult<DrawRemoteSearchDto>>
{
    public async Task<Result<PagedResult<DrawRemoteSearchDto>>> Handle(RemoteSearchDrawsQuery request, CancellationToken cancellationToken)
    {
        if (request.TenantId != tenantContext.TenantId)
        {
            return Result.Failure<PagedResult<DrawRemoteSearchDto>>(GamingErrors.DrawTenantMismatch);
        }

        var builder = new StringBuilder(
            """
            SELECT
                d.id AS Id,
                d.draw_code AS Name
            FROM gaming.draws d
            WHERE d.tenant_id = @TenantId
            """);

        var parameters = new DynamicParameters();
        parameters.Add("TenantId", request.TenantId);

        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            builder.Append(" AND d.draw_code ILIKE @Keyword");
            parameters.Add("Keyword", $"%{request.Query.Trim()}%");
        }

        const string countSql = "SELECT COUNT(*) FROM ({0}) AS counted";
        string baseSql = builder.ToString();
        string finalSql = $"{baseSql} ORDER BY d.draw_code ASC LIMIT @PageSize OFFSET @Offset";

        parameters.Add("PageSize", request.PageSize);
        parameters.Add("Offset", (request.Page - 1) * request.PageSize);

        using IDbConnection connection = dbConnectionFactory.GetOpenConnection();

        IEnumerable<DrawRemoteSearchDto> items = await connection.QueryAsync<DrawRemoteSearchDto>(finalSql, parameters);
        int totalCount = await connection.ExecuteScalarAsync<int>(
            string.Format(CultureInfo.InvariantCulture, countSql, baseSql),
            parameters);

        return PagedResult<DrawRemoteSearchDto>.Create(items, totalCount, request.Page, request.PageSize);
    }
}
