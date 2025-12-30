using System.Data;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Members.Dtos;
using Dapper;

namespace Application.Members.Assets.GetAssets;

internal sealed class GetMemberAssetsQueryHandler(IDbConnectionFactory factory)
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
            WHERE mab.member_id = @MemberId
            ORDER BY mab.asset_code
            """;

        using IDbConnection connection = factory.GetOpenConnection();

        IEnumerable<MemberAssetBalanceDto> assets = await connection.QueryAsync<MemberAssetBalanceDto>(
            sql,
            new { request.MemberId });

        return assets.ToArray();
    }
}
