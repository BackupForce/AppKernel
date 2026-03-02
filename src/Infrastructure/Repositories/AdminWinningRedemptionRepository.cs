using System.Data;
using Application.Abstractions.Data;
using Application.Gaming.Dtos;
using Dapper;
using Application.Abstractions.Gaming;
using Domain.Gaming.Shared;
using Domain.Gaming.Tickets;
using SharedKernel;

namespace Infrastructure.Repositories;

internal sealed class AdminWinningRedemptionRepository(IDbConnectionFactory dbConnectionFactory) : IAdminWinningRedemptionRepository
{
    public async Task<AdminWinningRedemptionResult> RedeemAsync(
        Guid tenantId,
        Guid winningId,
        Guid redeemedByUserId,
        DateTime redeemedAtUtc,
        CancellationToken cancellationToken = default)
    {
        using IDbConnection connection = dbConnectionFactory.GetOpenConnection();
        using IDbTransaction transaction = connection.BeginTransaction();

        int winningUpdatedRows = await connection.ExecuteAsync(
            """
            UPDATE gaming.ticket_line_results
            SET redeemed_at_utc = @RedeemedAtUtc,
                redeemed_by_user_id = @RedeemedByUserId
            WHERE tenant_id = @TenantId
              AND id = @WinningId
              AND redeemed_at_utc IS NULL
            """,
            new { TenantId = tenantId, WinningId = winningId, RedeemedAtUtc = redeemedAtUtc, RedeemedByUserId = redeemedByUserId },
            transaction);

        if (winningUpdatedRows == 0)
        {
            bool exists = await connection.ExecuteScalarAsync<bool>(
                """
                SELECT EXISTS(
                    SELECT 1
                    FROM gaming.ticket_line_results
                    WHERE tenant_id = @TenantId
                      AND id = @WinningId
                )
                """,
                new { TenantId = tenantId, WinningId = winningId },
                transaction);

            transaction.Rollback();
            return exists ? AdminWinningRedemptionResult.Conflict : AdminWinningRedemptionResult.NotFound;
        }

        int ticketDrawUpdatedRows = await connection.ExecuteAsync(
            """
            UPDATE gaming.ticket_draws td
            SET participation_status = @RedeemedStatus,
                redeemed_at_utc = @RedeemedAtUtc
            FROM gaming.ticket_line_results tlr
            WHERE tlr.id = @WinningId
              AND tlr.tenant_id = @TenantId
              AND td.tenant_id = tlr.tenant_id
              AND td.ticket_id = tlr.ticket_id
              AND td.draw_id = tlr.draw_id
              AND td.participation_status = @SettledStatus
              AND td.redeemed_at_utc IS NULL
            """,
            new
            {
                WinningId = winningId,
                TenantId = tenantId,
                RedeemedStatus = TicketDrawParticipationStatus.Redeemed,
                SettledStatus = TicketDrawParticipationStatus.Settled,
                RedeemedAtUtc = redeemedAtUtc
            },
            transaction);

        if (ticketDrawUpdatedRows == 0)
        {
            transaction.Rollback();
            return AdminWinningRedemptionResult.Conflict;
        }

        transaction.Commit();
        return AdminWinningRedemptionResult.Success;
    }

    public async Task<Result<AdminWinningDetailDto>> GetWinningDetailAsync(
        Guid tenantId,
        Guid winningId,
        CancellationToken cancellationToken = default)
    {
        const string sql =
            """
            SELECT
                tlr.id AS WinningId,
                tlr.tenant_id AS TenantId,
                tlr.ticket_id AS TicketId,
                tlr.draw_id AS DrawId,
                d.draw_at AS DrawDateUtc,
                d.draw_code AS DrawCode,
                tlr.payout AS PayoutAmount,
                tlr.prize_tier AS PrizeTier,
                td.participation_status AS Status,
                tlr.settled_at_utc AS SettledAtUtc,
                tlr.redeemed_at_utc AS RedeemedAtUtc,
                tlr.redeemed_by_user_id AS RedeemedByUserId,
                u.name AS RedeemedByUserName
            FROM gaming.ticket_line_results tlr
            JOIN gaming.ticket_draws td
                ON td.tenant_id = tlr.tenant_id
               AND td.ticket_id = tlr.ticket_id
               AND td.draw_id = tlr.draw_id
            JOIN gaming.draws d
                ON d.id = tlr.draw_id
               AND d.tenant_id = tlr.tenant_id
            LEFT JOIN users u
                ON u.id = tlr.redeemed_by_user_id
            WHERE tlr.tenant_id = @TenantId
              AND tlr.id = @WinningId
            LIMIT 1
            """;

        using IDbConnection connection = dbConnectionFactory.GetOpenConnection();
        AdminWinningDetailDto? item = await connection.QuerySingleOrDefaultAsync<AdminWinningDetailDto>(sql, new { TenantId = tenantId, WinningId = winningId });

        if (item is null)
        {
            return Result.Failure<AdminWinningDetailDto>(GamingErrors.TicketLineResultNotFound);
        }

        return Result.Success(item);
    }
}
