using System.Data;
using System.Text;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Members.Dtos;
using Dapper;
using SharedKernel;
using System.Globalization;

namespace Application.Members.Activity.GetActivity;

internal sealed class GetMemberActivityLogQueryHandler(
    IDbConnectionFactory factory,
    ITenantContext tenantContext)
    : IQueryHandler<GetMemberActivityLogQuery, PagedResult<MemberActivityLogDto>>
{
    public async Task<Result<PagedResult<MemberActivityLogDto>>> Handle(
        GetMemberActivityLogQuery request,
        CancellationToken cancellationToken)
    {
        var builder = new StringBuilder(
            """
            SELECT
                mal.id AS Id,
                mal.member_id AS MemberId,
                mal.action AS Action,
                mal.ip AS Ip,
                mal.user_agent AS UserAgent,
                mal.operator_user_id AS OperatorUserId,
                mal.payload AS Payload,
                mal.created_at AS CreatedAt
            FROM member_activity_log mal
            INNER JOIN members m ON m.id = mal.member_id
            WHERE mal.member_id = @MemberId
              AND m.tenant_id = @TenantId
            """);

        var parameters = new DynamicParameters(new
        {
            request.MemberId,
            TenantId = tenantContext.TenantId
        });

        if (request.StartDate.HasValue)
        {
            builder.Append(" AND mal.created_at >= @StartDate");
            parameters.Add("StartDate", request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            builder.Append(" AND mal.created_at <= @EndDate");
            parameters.Add("EndDate", request.EndDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Action))
        {
            builder.Append(" AND mal.action = @Action");
            parameters.Add("Action", request.Action);
        }

        const string countSql = "SELECT COUNT(*) FROM ({0}) AS counted";
        string baseSql = builder.ToString();
        string finalSql = $"{baseSql} ORDER BY mal.created_at DESC LIMIT @PageSize OFFSET @Offset";

        parameters.Add("PageSize", request.PageSize);
        parameters.Add("Offset", (request.Page - 1) * request.PageSize);

        using IDbConnection connection = factory.GetOpenConnection();

        IEnumerable<MemberActivityLogDto> items = await connection.QueryAsync<MemberActivityLogDto>(
            finalSql,
            parameters);
        int totalCount = await connection.ExecuteScalarAsync<int>(string.Format(CultureInfo.InvariantCulture, countSql, baseSql), parameters);

        return PagedResult<MemberActivityLogDto>.Create(items, totalCount, request.Page, request.PageSize);
    }
}
