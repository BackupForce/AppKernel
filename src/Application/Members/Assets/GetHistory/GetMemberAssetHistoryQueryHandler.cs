using System.Data;
using System.Text;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Members.Dtos;
using Dapper;

namespace Application.Members.Assets.GetHistory;

internal sealed class GetMemberAssetHistoryQueryHandler(IDbConnectionFactory factory)
    : IQueryHandler<GetMemberAssetHistoryQuery, PagedResult<MemberAssetLedgerDto>>
{
    public async Task<Result<PagedResult<MemberAssetLedgerDto>>> Handle(
        GetMemberAssetHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var builder = new StringBuilder(
            """
            SELECT
                mal.id AS Id,
                mal.member_id AS MemberId,
                mal.asset_code AS AssetCode,
                mal.type AS Type,
                mal.amount AS Amount,
                mal.before_balance AS BeforeBalance,
                mal.after_balance AS AfterBalance,
                mal.reference_type AS ReferenceType,
                mal.reference_id AS ReferenceId,
                mal.operator_user_id AS OperatorUserId,
                mal.remark AS Remark,
                mal.created_at AS CreatedAt
            FROM member_asset_ledger mal
            WHERE mal.member_id = @MemberId AND mal.asset_code = @AssetCode
            """);

        var parameters = new DynamicParameters(new { request.MemberId, request.AssetCode });

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

        if (request.Type.HasValue)
        {
            builder.Append(" AND mal.type = @Type");
            parameters.Add("Type", request.Type.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.ReferenceType))
        {
            builder.Append(" AND mal.reference_type = @ReferenceType");
            parameters.Add("ReferenceType", request.ReferenceType);
        }

        if (!string.IsNullOrWhiteSpace(request.ReferenceId))
        {
            builder.Append(" AND mal.reference_id = @ReferenceId");
            parameters.Add("ReferenceId", request.ReferenceId);
        }

        const string countSql = "SELECT COUNT(*) FROM ({0}) AS counted";
        string baseSql = builder.ToString();
        string finalSql = $"{baseSql} ORDER BY mal.created_at DESC LIMIT @PageSize OFFSET @Offset";

        parameters.Add("PageSize", request.PageSize);
        parameters.Add("Offset", (request.Page - 1) * request.PageSize);

        using IDbConnection connection = factory.GetOpenConnection();

        IEnumerable<MemberAssetLedgerDto> items = await connection.QueryAsync<MemberAssetLedgerDto>(finalSql, parameters);
        int totalCount = await connection.ExecuteScalarAsync<int>(string.Format(countSql, baseSql), parameters);

        return PagedResult<MemberAssetLedgerDto>.Create(items, totalCount, request.Page, request.PageSize);
    }
}
