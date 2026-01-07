using System.Data;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Members.Dtos;
using Dapper;
using SharedKernel;

namespace Application.Members.Assets.GetAssets;

internal sealed class GetMemberAssetsQueryHandler(
    IDbConnectionFactory factory,
    ITenantContext tenantContext)
    : IQueryHandler<GetMemberAssetsQuery, IReadOnlyCollection<MemberAssetBalanceDto>>
{
    public async Task<Result<IReadOnlyCollection<MemberAssetBalanceDto>>> Handle(
        GetMemberAssetsQuery request,
        CancellationToken cancellationToken)
    {
        const string sql =
            """
            SELECT
                mab.member_id AS MemberId,
                mab.asset_code AS AssetCode,
                mab.balance AS Balance,
                mab.updated_at AS UpdatedAt
            FROM member_asset_balance mab
            INNER JOIN members m ON m.id = mab.member_id
            WHERE mab.member_id = @MemberId
              AND m.tenant_id = @TenantId
            ORDER BY mab.asset_code
            """;

        using IDbConnection connection = factory.GetOpenConnection();

        IEnumerable<MemberAssetBalanceDto> assets = await connection.QueryAsync<MemberAssetBalanceDto>(
            sql,
            new
            {
                request.MemberId,
                TenantId = tenantContext.TenantId
            });

        return assets.ToArray();
    }
}
