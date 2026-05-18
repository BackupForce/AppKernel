using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Gaming.Dtos;
using Dapper;
using SharedKernel;

namespace Application.Gaming.Prizes.GetList;

internal sealed class GetPrizeListQueryHandler(
    IDbConnectionFactory dbConnectionFactory,
    ITenantContext tenantContext) : IQueryHandler<GetPrizeListQuery, IReadOnlyCollection<PrizeDto>>
{
    public async Task<Result<IReadOnlyCollection<PrizeDto>>> Handle(
        GetPrizeListQuery request,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                p.id AS Id,
                p.name AS Name,
                p.description AS Description,
                p.cost AS Cost,
                p.is_active AS IsActive,
                p.created_at AS CreatedAt,
                p.updated_at AS UpdatedAt
            FROM gaming.prizes p
            WHERE p.tenant_id = @TenantId
            ORDER BY p.created_at DESC
            """;

        using System.Data.IDbConnection connection = dbConnectionFactory.GetOpenConnection();

        IEnumerable<PrizeDto> items = await connection.QueryAsync<PrizeDto>(
            sql,
            new { tenantContext.TenantId });

        return items.ToList();
    }
}
