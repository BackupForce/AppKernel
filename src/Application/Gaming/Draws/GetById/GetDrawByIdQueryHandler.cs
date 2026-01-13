using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Gaming.Dtos;
using Dapper;
using SharedKernel;

namespace Application.Gaming.Draws.GetById;

internal sealed class GetDrawByIdQueryHandler(
    IDbConnectionFactory dbConnectionFactory,
    ITenantContext tenantContext) : IQueryHandler<GetDrawByIdQuery, DrawDetailDto>
{
    public async Task<Result<DrawDetailDto>> Handle(GetDrawByIdQuery request, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                d.id AS Id,
                d.sales_open_at AS SalesStartAt,
                d.sales_close_at AS SalesCloseAt,
                d.draw_at AS DrawAt,
                CASE
                    WHEN d.status = 0 THEN 'Scheduled'
                    WHEN d.status = 1 THEN 'SalesOpen'
                    WHEN d.status = 2 THEN 'SalesClosed'
                    WHEN d.status = 3 THEN 'Settled'
                    WHEN d.status = 4 THEN 'Cancelled'
                    ELSE 'Scheduled'
                END AS Status,
                d.is_manually_closed AS IsManuallyClosed,
                d.manual_close_at AS ManualCloseAt,
                d.manual_close_reason AS ManualCloseReason,
                d.redeem_valid_days AS RedeemValidDays,
                d.winning_numbers AS WinningNumbers,
                d.server_seed_hash AS ServerSeedHash,
                d.server_seed AS ServerSeed,
                d.algorithm AS Algorithm,
                d.derived_input AS DerivedInput
            FROM gaming.draws d
            WHERE d.tenant_id = @TenantId AND d.id = @DrawId
            """;

        using System.Data.IDbConnection connection = dbConnectionFactory.GetOpenConnection();

        DrawDetailDto? draw = await connection.QueryFirstOrDefaultAsync<DrawDetailDto>(
            sql,
            new { tenantContext.TenantId, request.DrawId });

        if (draw is null)
        {
            return Result.Failure<DrawDetailDto>(Domain.Gaming.GamingErrors.DrawNotFound);
        }

        return draw;
    }
}
