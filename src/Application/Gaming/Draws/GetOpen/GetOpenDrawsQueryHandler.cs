using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Gaming;
using Application.Abstractions.Messaging;
using Application.Gaming.Dtos;
using Dapper;
using SharedKernel;
using Domain.Gaming.Catalog;

namespace Application.Gaming.Draws.GetOpen;

internal sealed class GetOpenDrawsQueryHandler(
    IDbConnectionFactory dbConnectionFactory,
    ITenantContext tenantContext,
    IDateTimeProvider dateTimeProvider,
    IEntitlementChecker entitlementChecker) : IQueryHandler<GetOpenDrawsQuery, IReadOnlyCollection<DrawSummaryDto>>
{
    public async Task<Result<IReadOnlyCollection<DrawSummaryDto>>> Handle(
        GetOpenDrawsQuery request,
        CancellationToken cancellationToken)
    {
        Result<GameCode> gameCodeResult = GameCode.Create(request.GameCode);
        if (gameCodeResult.IsFailure)
        {
            return Result.Failure<IReadOnlyCollection<DrawSummaryDto>>(gameCodeResult.Error);
        }

        Result entitlementResult = await entitlementChecker.EnsureGameEnabledAsync(
            tenantContext.TenantId,
            gameCodeResult.Value,
            cancellationToken);
        if (entitlementResult.IsFailure)
        {
            return Result.Failure<IReadOnlyCollection<DrawSummaryDto>>(entitlementResult.Error);
        }

        const string sql = """
            WITH draws AS (
                SELECT
                    d.id,
                    d.game_code,
                    d.draw_code,
                    d.sales_open_at,
                    d.sales_close_at,
                    d.draw_at,
                    d.status,
                    d.is_manually_closed,
                    CASE
                        WHEN d.status = 4 THEN 'Cancelled'
                        WHEN d.settled_at IS NOT NULL THEN 'Settled'
                        WHEN d.is_manually_closed = TRUE THEN 'SalesClosed'
                        WHEN @Now < d.sales_open_at THEN 'Scheduled'
                        WHEN @Now >= d.sales_open_at AND @Now < d.sales_close_at THEN 'SalesOpen'
                        ELSE 'SalesClosed'
                    END AS effective_status
                FROM gaming.draws d
                WHERE d.tenant_id = @TenantId
                  AND d.game_code = @GameCode
                  AND d.status <> 4
            )
            SELECT
                id AS Id,
                game_code AS GameCode,
                draw_code AS DrawCode,
                sales_open_at AS SalesStartAt,
                sales_close_at AS SalesCloseAt,
                draw_at AS DrawAt,
                effective_status AS Status
            FROM draws
            WHERE
                @Status IS NULL
                OR effective_status = @Status
            ORDER BY sales_open_at ASC;
            """;

        using System.Data.IDbConnection connection = dbConnectionFactory.GetOpenConnection();

        string? status = string.IsNullOrWhiteSpace(request.Status) ? null : request.Status.Trim();
        IEnumerable<DrawSummaryDto> items = await connection.QueryAsync<DrawSummaryDto>(
            sql,
            new
            {
                tenantContext.TenantId,
                GameCode = gameCodeResult.Value.Value,
                Now = dateTimeProvider.UtcNow,
                Status = status
            });

        return items.ToList();
    }
}
