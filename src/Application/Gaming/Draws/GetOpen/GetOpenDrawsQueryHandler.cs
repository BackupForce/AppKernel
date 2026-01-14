using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Gaming.Dtos;
using Dapper;
using SharedKernel;

namespace Application.Gaming.Draws.GetOpen;

internal sealed class GetOpenDrawsQueryHandler(
    IDbConnectionFactory dbConnectionFactory,
    ITenantContext tenantContext,
    IDateTimeProvider dateTimeProvider) : IQueryHandler<GetOpenDrawsQuery, IReadOnlyCollection<DrawSummaryDto>>
{
    public async Task<Result<IReadOnlyCollection<DrawSummaryDto>>> Handle(
        GetOpenDrawsQuery request,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                d.id AS Id,
                d.game_code AS GameCode,
                d.sales_open_at AS SalesStartAt,
                d.sales_close_at AS SalesCloseAt,
                d.draw_at AS DrawAt,
                CASE
                    WHEN d.is_manually_closed = TRUE THEN 'SalesClosed'
                    WHEN @Now BETWEEN d.sales_open_at AND d.sales_close_at THEN 'SalesOpen'
                    WHEN d.status = 0 THEN 'Scheduled'
                    WHEN d.status = 1 THEN 'SalesOpen'
                    WHEN d.status = 2 THEN 'SalesClosed'
                    WHEN d.status = 3 THEN 'Settled'
                    WHEN d.status = 4 THEN 'Cancelled'
                    ELSE 'Scheduled'
                END AS Status
            FROM gaming.draws d
            WHERE d.tenant_id = @TenantId
              AND d.status <> 4
              AND (
                @Status IS NULL
                OR (@Status = 'SalesOpen' AND d.is_manually_closed = FALSE AND @Now >= d.sales_open_at AND @Now < d.sales_close_at)
              )
            ORDER BY d.sales_open_at ASC
            """;

        using System.Data.IDbConnection connection = dbConnectionFactory.GetOpenConnection();

        string? status = string.IsNullOrWhiteSpace(request.Status) ? "SalesOpen" : request.Status.Trim();
        IEnumerable<DrawSummaryDto> items = await connection.QueryAsync<DrawSummaryDto>(
            sql,
            new { tenantContext.TenantId, Now = dateTimeProvider.UtcNow, Status = status });

        return items.ToList();
    }
}
