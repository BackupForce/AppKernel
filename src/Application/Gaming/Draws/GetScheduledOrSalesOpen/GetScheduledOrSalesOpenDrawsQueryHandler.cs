using System.Data;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Dapper;
using Domain.Gaming.Draws;
using SharedKernel;

namespace Application.Gaming.Draws.GetScheduledOrSalesOpen;

internal sealed class GetScheduledOrSalesOpenDrawsQueryHandler(
    IDbConnectionFactory dbConnectionFactory,
    ITenantContext tenantContext,
    IDateTimeProvider dateTimeProvider)
    : IQueryHandler<GetScheduledOrSalesOpenDrawsQuery, IReadOnlyCollection<ScheduledOrSalesOpenDrawDto>>
{
    public async Task<Result<IReadOnlyCollection<ScheduledOrSalesOpenDrawDto>>> Handle(
        GetScheduledOrSalesOpenDrawsQuery request,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                d.id AS Id,
                d.game_code AS GameCode,
                d.draw_code AS DrawCode,
                d.sales_open_at AS SalesOpenAt,
                d.sales_close_at AS SalesCloseAt,
                d.draw_at AS DrawAt,
                CASE
                    WHEN @NowUtc < d.sales_open_at THEN 'Scheduled'
                    ELSE 'SalesOpen'
                END AS EffectiveStatus
            FROM gaming.draws d
            WHERE d.tenant_id = @TenantId
              AND d.status <> @CancelledStatus
              AND d.settled_at_utc IS NULL
              AND d.settled_at IS NULL
              AND (d.winning_numbers_raw IS NULL OR BTRIM(d.winning_numbers_raw) = '')
              AND d.is_manually_closed = FALSE
              AND @NowUtc < d.sales_close_at
            ORDER BY d.sales_open_at ASC, d.draw_at ASC;
            """;

        using IDbConnection connection = dbConnectionFactory.GetOpenConnection();

        IEnumerable<ScheduledOrSalesOpenDrawDto> items = await connection.QueryAsync<ScheduledOrSalesOpenDrawDto>(
            sql,
            new
            {
                tenantContext.TenantId,
                NowUtc = dateTimeProvider.UtcNow,
                CancelledStatus = (int)DrawStatus.Cancelled
            });

        return items.ToList();
    }
}
