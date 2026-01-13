using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Gaming.Dtos;
using Dapper;
using SharedKernel;

namespace Application.Gaming.Draws.PrizeMappings.Get;

internal sealed class GetDrawPrizeMappingsQueryHandler(
    IDbConnectionFactory dbConnectionFactory,
    ITenantContext tenantContext) : IQueryHandler<GetDrawPrizeMappingsQuery, IReadOnlyCollection<DrawPrizeMappingDto>>
{
    public async Task<Result<IReadOnlyCollection<DrawPrizeMappingDto>>> Handle(
        GetDrawPrizeMappingsQuery request,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                m.match_count AS MatchCount,
                p.id AS PrizeId,
                p.name AS PrizeName,
                p.cost AS PrizeCost,
                p.is_active AS IsActive
            FROM gaming_draw_prize_mappings m
            INNER JOIN gaming_prizes p ON p.id = m.prize_id
            WHERE m.tenant_id = @TenantId
              AND m.draw_id = @DrawId
            ORDER BY m.match_count ASC, p.name ASC
            """;

        using System.Data.IDbConnection connection = dbConnectionFactory.GetOpenConnection();

        IEnumerable<DrawPrizeMappingRow> rows = await connection.QueryAsync<DrawPrizeMappingRow>(
            sql,
            new { tenantContext.TenantId, request.DrawId });

        Dictionary<int, List<DrawPrizeMappingPrizeDto>> mapping = new Dictionary<int, List<DrawPrizeMappingPrizeDto>>();
        foreach (DrawPrizeMappingRow row in rows)
        {
            if (!mapping.TryGetValue(row.MatchCount, out List<DrawPrizeMappingPrizeDto>? prizeList))
            {
                prizeList = new List<DrawPrizeMappingPrizeDto>();
                mapping[row.MatchCount] = prizeList;
            }

            prizeList.Add(new DrawPrizeMappingPrizeDto(
                row.PrizeId,
                row.PrizeName,
                row.PrizeCost,
                row.IsActive));
        }

        List<DrawPrizeMappingDto> result = new List<DrawPrizeMappingDto>();
        foreach (KeyValuePair<int, List<DrawPrizeMappingPrizeDto>> entry in mapping)
        {
            result.Add(new DrawPrizeMappingDto(entry.Key, entry.Value));
        }

        return result;
    }

    private sealed record DrawPrizeMappingRow(
        int MatchCount,
        Guid PrizeId,
        string PrizeName,
        decimal PrizeCost,
        bool IsActive);
}
