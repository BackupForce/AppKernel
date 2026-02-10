using System.Data;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Dapper;
using Domain.Gaming.Shared;
using Domain.Gaming.Tickets;
using SharedKernel;

namespace Application.Gaming.Reports.Admin;

internal sealed class GetWinningCostByDrawQueryHandler(
    IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetWinningCostByDrawQuery, WinningCostPerDrawDto>
{
    private static readonly int[] WinningStatuses =
    [
        (int)TicketDrawParticipationStatus.Settled,
        (int)TicketDrawParticipationStatus.Redeemed
    ];

    public async Task<Result<WinningCostPerDrawDto>> Handle(
        GetWinningCostByDrawQuery request,
        CancellationToken cancellationToken)
    {
        const string summarySql =
            """
            SELECT
                d.id AS DrawId,
                d.draw_code AS DrawCode,
                d.draw_at AS DrawAt,
                d.game_code AS GameCode,
                COALESCE(SUM(CASE WHEN td.participation_status = ANY(@WinningStatuses) THEN tlr.payout ELSE 0 END), 0) AS TotalPayout,
                COUNT(DISTINCT t.id) AS TicketCount,
                COUNT(DISTINCT CASE WHEN td.participation_status = ANY(@WinningStatuses) THEN t.id END) AS WinningTicketCount,
                COUNT(DISTINCT CASE WHEN td.participation_status = ANY(@WinningStatuses) THEN tlr.id END) AS WinningCount
            FROM gaming.draws d
            LEFT JOIN gaming.ticket_draws td
                ON td.draw_id = d.id
               AND td.tenant_id = @TenantId
            LEFT JOIN gaming.tickets t
                ON t.id = td.ticket_id
               AND t.tenant_id = @TenantId
            LEFT JOIN gaming.ticket_line_results tlr
                ON tlr.ticket_id = t.id
               AND tlr.draw_id = d.id
               AND tlr.tenant_id = @TenantId
            WHERE d.tenant_id = @TenantId
              AND d.id = @DrawId
            GROUP BY d.id, d.draw_code, d.draw_at, d.game_code;
            """;

        const string detailCountSql =
            """
            SELECT COUNT(*)
            FROM gaming.ticket_draws td
            JOIN gaming.tickets t
                ON t.id = td.ticket_id
               AND t.tenant_id = @TenantId
            JOIN gaming.ticket_line_results tlr
                ON tlr.ticket_id = t.id
               AND tlr.draw_id = td.draw_id
               AND tlr.tenant_id = @TenantId
            WHERE td.tenant_id = @TenantId
              AND td.draw_id = @DrawId
              AND td.participation_status = ANY(@WinningStatuses);
            """;

        const string detailsSql =
            """
            SELECT
                t.id AS TicketId,
                tlr.line_index AS TicketLineIndex,
                m.user_id AS MemberUserId,
                m.display_name AS MemberName,
                tl.numbers_raw AS Numbers,
                d.winning_numbers_raw AS WinningNumbers,
                td.participation_status AS ParticipationStatus,
                tlr.payout AS PayoutAmount,
                td.settled_at_utc AS SettledAtUtc,
                td.redeemed_at_utc AS RedeemedAtUtc,
                td.created_at_utc AS CreatedAtUtc
            FROM gaming.ticket_draws td
            JOIN gaming.tickets t
                ON t.id = td.ticket_id
               AND t.tenant_id = @TenantId
            JOIN gaming.draws d
                ON d.id = td.draw_id
               AND d.tenant_id = @TenantId
            JOIN gaming.ticket_line_results tlr
                ON tlr.ticket_id = t.id
               AND tlr.draw_id = td.draw_id
               AND tlr.tenant_id = @TenantId
            LEFT JOIN gaming.ticket_lines tl
                ON tl.ticket_id = t.id
               AND tl.line_index = tlr.line_index
            LEFT JOIN members m
                ON m.id = t.member_id
               AND m.tenant_id = @TenantId
            WHERE td.tenant_id = @TenantId
              AND td.draw_id = @DrawId
              AND td.participation_status = ANY(@WinningStatuses)
            ORDER BY td.created_at_utc DESC, t.id, tlr.line_index
            LIMIT @DetailPageSize OFFSET @DetailOffset;
            """;

        using IDbConnection connection = dbConnectionFactory.GetOpenConnection();

        var parameters = new
        {
            request.TenantId,
            request.DrawId,
            WinningStatuses,
            DetailPageSize = request.DetailPageSize,
            DetailOffset = (request.DetailPage - 1) * request.DetailPageSize
        };

        WinningCostPerDrawDto? summary = await connection.QuerySingleOrDefaultAsync<WinningCostPerDrawDto>(
            new CommandDefinition(summarySql, parameters, cancellationToken: cancellationToken));

        if (summary is null)
        {
            return Result.Failure<WinningCostPerDrawDto>(GamingErrors.DrawNotFound);
        }

        int detailTotal = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(detailCountSql, parameters, cancellationToken: cancellationToken));

        IEnumerable<WinningDetailDto> details = await connection.QueryAsync<WinningDetailDto>(
            new CommandDefinition(detailsSql, parameters, cancellationToken: cancellationToken));

        summary.Details = details.ToList();
        summary.DetailPage = request.DetailPage;
        summary.DetailPageSize = request.DetailPageSize;
        summary.DetailTotal = detailTotal;

        return summary;
    }
}
