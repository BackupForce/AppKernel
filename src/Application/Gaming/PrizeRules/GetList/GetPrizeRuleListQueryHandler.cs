using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Gaming.Dtos;
using Dapper;
using SharedKernel;

namespace Application.Gaming.PrizeRules.GetList;

internal sealed class GetPrizeRuleListQueryHandler(
    IDbConnectionFactory dbConnectionFactory,
    ITenantContext tenantContext) : IQueryHandler<GetPrizeRuleListQuery, IReadOnlyCollection<PrizeRuleDto>>
{
    public async Task<Result<IReadOnlyCollection<PrizeRuleDto>>> Handle(
        GetPrizeRuleListQuery request,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                r.id AS Id,
                r.match_count AS MatchCount,
                r.prize_id AS PrizeId,
                p.name AS PrizeName,
                r.is_active AS IsActive,
                r.effective_from AS EffectiveFrom,
                r.effective_to AS EffectiveTo,
                r.redeem_valid_days AS RedeemValidDays
            FROM gaming.prize_rules r
            INNER JOIN gaming.prizes p ON p.id = r.prize_id
            WHERE r.tenant_id = @TenantId
            ORDER BY r.match_count ASC
            """;

        using System.Data.IDbConnection connection = dbConnectionFactory.GetOpenConnection();

        IEnumerable<PrizeRuleDto> items = await connection.QueryAsync<PrizeRuleDto>(
            sql,
            new { tenantContext.TenantId });

        return items.ToList();
    }
}
