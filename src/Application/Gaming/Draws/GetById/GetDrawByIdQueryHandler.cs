using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Gaming;
using Application.Abstractions.Messaging;
using Application.Gaming.Dtos;
using Dapper;
using SharedKernel;
using Domain.Gaming.Catalog;
using Domain.Gaming.Shared;

namespace Application.Gaming.Draws.GetById;

internal sealed class GetDrawByIdQueryHandler(
    IDbConnectionFactory dbConnectionFactory,
    ITenantContext tenantContext,
    IDateTimeProvider dateTimeProvider,
    IEntitlementChecker entitlementChecker) : IQueryHandler<GetDrawByIdQuery, DrawDetailDto>
{
    public async Task<Result<DrawDetailDto>> Handle(GetDrawByIdQuery request, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                d.id AS Id,
                d.game_code AS GameCode,
                d.sales_open_at AS SalesStartAt,
                d.sales_close_at AS SalesCloseAt,
                d.draw_at AS DrawAt,
                CASE
                    WHEN d.status = 4 THEN 'Cancelled'
                    WHEN d.settled_at IS NOT NULL THEN 'Settled'
                    WHEN d.is_manually_closed = TRUE THEN 'SalesClosed'
                    WHEN @Now < d.sales_open_at THEN 'Scheduled'
                    WHEN @Now >= d.sales_open_at AND @Now < d.sales_close_at THEN 'SalesOpen'
                    ELSE 'SalesClosed'
                END AS Status,
                d.is_manually_closed AS IsManuallyClosed,
                d.manual_close_at AS ManualCloseAt,
                d.manual_close_reason AS ManualCloseReason,
                d.redeem_valid_days AS RedeemValidDays,
                d.winning_numbers_raw AS WinningNumbers,
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
            new { tenantContext.TenantId, request.DrawId, Now = dateTimeProvider.UtcNow });

        if (draw is null)
        {
            return Result.Failure<DrawDetailDto>(GamingErrors.DrawNotFound);
        }

        Result<GameCode> gameCodeResult = GameCode.Create(draw.GameCode);
        if (gameCodeResult.IsFailure)
        {
            return Result.Failure<DrawDetailDto>(gameCodeResult.Error);
        }

        Result entitlementResult = await entitlementChecker.EnsureGameEnabledAsync(
            tenantContext.TenantId,
            gameCodeResult.Value,
            cancellationToken);
        if (entitlementResult.IsFailure)
        {
            return Result.Failure<DrawDetailDto>(entitlementResult.Error);
        }

        return draw;
    }
}
