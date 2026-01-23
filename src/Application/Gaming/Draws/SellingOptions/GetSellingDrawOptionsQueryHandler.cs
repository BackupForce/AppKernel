using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Gaming;
using Application.Abstractions.Messaging;
using Application.Gaming.Dtos;
using Dapper;
using Domain.Gaming.Catalog;
using SharedKernel;

namespace Application.Gaming.Draws.SellingOptions;

internal sealed class GetSellingDrawOptionsQueryHandler(
    IDbConnectionFactory dbConnectionFactory,
    ITenantContext tenantContext,
    IDateTimeProvider dateTimeProvider,
    IEntitlementChecker entitlementChecker)
    : IQueryHandler<GetSellingDrawOptionsQuery, IReadOnlyList<DrawSellingOptionDto>>
{
    public async Task<Result<IReadOnlyList<DrawSellingOptionDto>>> Handle(
        GetSellingDrawOptionsQuery request,
        CancellationToken cancellationToken)
    {
        GameCode? gameCode = null;
        string? gameCodeValue = null;

        if (!string.IsNullOrWhiteSpace(request.GameCode))
        {
            Result<GameCode> gameCodeResult = GameCode.Create(request.GameCode);
            if (gameCodeResult.IsFailure)
            {
                return Result.Failure<IReadOnlyList<DrawSellingOptionDto>>(gameCodeResult.Error);
            }

            gameCode = gameCodeResult.Value;
            gameCodeValue = gameCode.Value;

            Result entitlementResult = await entitlementChecker.EnsureGameEnabledAsync(
                tenantContext.TenantId,
                gameCode,
                cancellationToken);
            if (entitlementResult.IsFailure)
            {
                return Result.Failure<IReadOnlyList<DrawSellingOptionDto>>(entitlementResult.Error);
            }
        }

        string? playTypeCodeValue = null;
        if (!string.IsNullOrWhiteSpace(request.PlayTypeCode))
        {
            Result<PlayTypeCode> playTypeCodeResult = PlayTypeCode.Create(request.PlayTypeCode);
            if (playTypeCodeResult.IsFailure)
            {
                return Result.Failure<IReadOnlyList<DrawSellingOptionDto>>(playTypeCodeResult.Error);
            }

            playTypeCodeValue = playTypeCodeResult.Value.Value;
        }

        int take = request.Take ?? 50;
        if (take <= 0)
        {
            take = 1;
        }

        if (take > 200)
        {
            take = 200;
        }

        const string sql = """
            SELECT
                d.id AS Value,
                d.game_code AS GameCode,
                d.sales_close_at AS SalesCloseAtUtc,
                d.draw_at AS DrawAtUtc
            FROM gaming.draws d
            WHERE d.tenant_id = @TenantId
              AND d.status <> 4
              AND d.is_manually_closed = FALSE
              AND @Now >= d.sales_open_at
              AND @Now < d.sales_close_at
              AND (@GameCode IS NULL OR d.game_code = @GameCode)
              AND (
                @PlayTypeCode IS NULL
                OR EXISTS (
                    SELECT 1
                    FROM gaming.draw_enabled_play_types p
                    WHERE p.tenant_id = d.tenant_id
                      AND p.draw_id = d.id
                      AND p.play_type_code = @PlayTypeCode
                )
              )
            ORDER BY d.sales_close_at ASC, d.draw_at ASC
            LIMIT @Take
            """;

        using System.Data.IDbConnection connection = dbConnectionFactory.GetOpenConnection();

        IEnumerable<DrawSellingOptionRow> rows = await connection.QueryAsync<DrawSellingOptionRow>(
            sql,
            new
            {
                tenantContext.TenantId,
                Now = dateTimeProvider.UtcNow,
                GameCode = gameCodeValue,
                PlayTypeCode = playTypeCodeValue,
                Take = take
            });

        List<DrawSellingOptionDto> options = rows
            .Select(row => new DrawSellingOptionDto(
                row.Value,
                $"{row.GameCode} | 售票至 {row.SalesCloseAtUtc:yyyy-MM-dd HH:mm} (UTC) | 開獎 {row.DrawAtUtc:yyyy-MM-dd HH:mm} (UTC)",
                row.SalesCloseAtUtc,
                row.DrawAtUtc))
            .ToList();

        return options;
    }

    private sealed record DrawSellingOptionRow(
        Guid Value,
        string GameCode,
        DateTime SalesCloseAtUtc,
        DateTime DrawAtUtc);
}
