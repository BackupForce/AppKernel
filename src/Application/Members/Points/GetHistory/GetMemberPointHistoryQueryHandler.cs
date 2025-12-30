using System.Data;
using System.Text;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Members.Dtos;
using Dapper;

namespace Application.Members.Points.GetHistory;

internal sealed class GetMemberPointHistoryQueryHandler(IDbConnectionFactory factory)
    : IQueryHandler<GetMemberPointHistoryQuery, PagedResult<MemberPointLedgerDto>>
{
    public async Task<Result<PagedResult<MemberPointLedgerDto>>> Handle(
        GetMemberPointHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var builder = new StringBuilder(
            """
            SELECT
                mpl.id AS Id,
                mpl.member_id AS MemberId,
                mpl.type AS Type,
                mpl.amount AS Amount,
                mpl.before_balance AS BeforeBalance,
                mpl.after_balance AS AfterBalance,
                mpl.reference_type AS ReferenceType,
                mpl.reference_id AS ReferenceId,
                mpl.operator_user_id AS OperatorUserId,
                mpl.remark AS Remark,
                mpl.created_at AS CreatedAt
            FROM member_point_ledger mpl
            WHERE mpl.member_id = @MemberId
            """);

        var parameters = new DynamicParameters(new { request.MemberId });

        if (request.StartDate.HasValue)
        {
            builder.Append(" AND mpl.created_at >= @StartDate");
            parameters.Add("StartDate", request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            builder.Append(" AND mpl.created_at <= @EndDate");
            parameters.Add("EndDate", request.EndDate.Value);
        }

        if (request.Type.HasValue)
        {
            builder.Append(" AND mpl.type = @Type");
            parameters.Add("Type", request.Type.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.ReferenceType))
        {
            builder.Append(" AND mpl.reference_type = @ReferenceType");
            parameters.Add("ReferenceType", request.ReferenceType);
        }

        if (!string.IsNullOrWhiteSpace(request.ReferenceId))
        {
            builder.Append(" AND mpl.reference_id = @ReferenceId");
            parameters.Add("ReferenceId", request.ReferenceId);
        }

        const string countSql = "SELECT COUNT(*) FROM ({0}) AS counted";
        string baseSql = builder.ToString();
        string finalSql = $"{baseSql} ORDER BY mpl.created_at DESC LIMIT @PageSize OFFSET @Offset";

        parameters.Add("PageSize", request.PageSize);
        parameters.Add("Offset", (request.Page - 1) * request.PageSize);

        using IDbConnection connection = factory.GetOpenConnection();

        IEnumerable<MemberPointLedgerDto> items = await connection.QueryAsync<MemberPointLedgerDto>(finalSql, parameters);
        int totalCount = await connection.ExecuteScalarAsync<int>(string.Format(countSql, baseSql), parameters);

        return PagedResult<MemberPointLedgerDto>.Create(items, totalCount, request.Page, request.PageSize);
    }
}
