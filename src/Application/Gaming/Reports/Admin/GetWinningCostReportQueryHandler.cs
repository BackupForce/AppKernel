using System.Data;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Dapper;
using Domain.Gaming.Tickets;
using SharedKernel;

namespace Application.Gaming.Reports.Admin;

internal sealed class GetWinningCostReportQueryHandler(
    IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetWinningCostReportQuery, WinningCostReportPageDto>
{
    private static readonly int[] WinningStatuses =
    [
        (int)TicketDrawParticipationStatus.Settled,
        (int)TicketDrawParticipationStatus.Redeemed
    ];

    public async Task<Result<WinningCostReportPageDto>> Handle(
        GetWinningCostReportQuery request,
        CancellationToken cancellationToken)
    {
        const string summarySql =
            """
            WITH base_draws AS (
                SELECT
                    d.id,
                    d.draw_code,
                    d.draw_at,
                    d.game_code
                FROM gaming.draws d
                WHERE d.tenant_id = @TenantId
                  AND d.draw_at >= @FromUtc
                  AND d.draw_at < @ToUtc
                  AND (@GameCode IS NULL OR d.game_code = @GameCode)
                  AND (
                      @DrawGroupId IS NULL
                      OR EXISTS (
                          SELECT 1
                          FROM gaming.ticket_draws td_filter
                          JOIN gaming.tickets t_filter
                            ON t_filter.id = td_filter.ticket_id
                           AND t_filter.tenant_id = @TenantId
                          WHERE td_filter.tenant_id = @TenantId
                            AND td_filter.draw_id = d.id
                            AND t_filter.draw_group_id = @DrawGroupId
                      )
                  )
            ),
            paged_draws AS (
                SELECT *
                FROM base_draws
                ORDER BY draw_at DESC
                LIMIT @PageSize OFFSET @Offset
            )
            SELECT
                p.id AS DrawId,
                p.draw_code AS DrawCode,
                p.draw_at AS DrawAt,
                p.game_code AS GameCode,
                COALESCE(SUM(CASE WHEN td.participation_status = ANY(@WinningStatuses) THEN tlr.payout ELSE 0 END), 0) AS TotalPayout,
                COUNT(DISTINCT t.id) AS TicketCount,
                COUNT(DISTINCT CASE WHEN td.participation_status = ANY(@WinningStatuses) THEN t.id END) AS ValidbetTicketCount,
                COUNT(DISTINCT CASE WHEN td.participation_status = ANY(@WinningStatuses) THEN tlr.id END) AS WinningCount
            FROM paged_draws p
            LEFT JOIN gaming.ticket_draws td
                ON td.draw_id = p.id
               AND td.tenant_id = @TenantId
            LEFT JOIN gaming.tickets t
                ON t.id = td.ticket_id
               AND t.tenant_id = @TenantId
               AND (@DrawGroupId IS NULL OR t.draw_group_id = @DrawGroupId)
            LEFT JOIN gaming.ticket_line_results tlr
                ON tlr.ticket_id = t.id
               AND tlr.draw_id = p.id
               AND tlr.tenant_id = @TenantId
            GROUP BY p.id, p.draw_code, p.draw_at, p.game_code
            ORDER BY p.draw_at DESC;
            """;

        const string totalSql =
            """
            SELECT COUNT(*)
            FROM gaming.draws d
            WHERE d.tenant_id = @TenantId
              AND d.draw_at >= @FromUtc
              AND d.draw_at < @ToUtc
              AND (@GameCode IS NULL OR d.game_code = @GameCode)
              AND (
                  @DrawGroupId IS NULL
                  OR EXISTS (
                      SELECT 1
                      FROM gaming.ticket_draws td_filter
                      JOIN gaming.tickets t_filter
                        ON t_filter.id = td_filter.ticket_id
                       AND t_filter.tenant_id = @TenantId
                      WHERE td_filter.tenant_id = @TenantId
                        AND td_filter.draw_id = d.id
                        AND t_filter.draw_group_id = @DrawGroupId
                  )
              );
            """;

        using IDbConnection connection = dbConnectionFactory.GetOpenConnection();

        var parameters = new
        {
            request.TenantId,
            request.FromUtc,
            request.ToUtc,
            GameCode = string.IsNullOrWhiteSpace(request.GameCode) ? null : request.GameCode.Trim(),
            request.DrawGroupId,
            request.PageSize,
            Offset = (request.Page - 1) * request.PageSize,
            WinningStatuses
        };

        List<WinningCostPerDrawDto> items = (await connection.QueryAsync<WinningCostPerDrawDto>(
            new CommandDefinition(summarySql, parameters, cancellationToken: cancellationToken))).AsList();

        int total = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(totalSql, parameters, cancellationToken: cancellationToken));

        if (request.IncludeDetails && items.Count > 0)
        {
            IReadOnlyDictionary<Guid, List<WinningDetailDto>> detailsByDraw = await LoadWinningDetailsByDrawAsync(
                connection,
                request.TenantId,
                items.Select(x => x.DrawId).ToArray(),
                cancellationToken);

            foreach (WinningCostPerDrawDto item in items)
            {
                item.Details = detailsByDraw.TryGetValue(item.DrawId, out List<WinningDetailDto>? details) ? details : Array.Empty<WinningDetailDto>();
            }
        }

        return new WinningCostReportPageDto
        {
            Page = request.Page,
            PageSize = request.PageSize,
            Total = total,
            Items = items
        };
    }

    private static async Task<IReadOnlyDictionary<Guid, List<WinningDetailDto>>> LoadWinningDetailsByDrawAsync(
        IDbConnection connection,
        Guid tenantId,
        IReadOnlyCollection<Guid> drawIds,
        CancellationToken cancellationToken)
    {
        const string detailsSql =
            """
            SELECT
                td.draw_id AS DrawId,
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
              AND td.draw_id = ANY(@DrawIds)
              AND td.participation_status = ANY(@WinningStatuses)
            ORDER BY td.draw_id, td.created_at_utc DESC, t.id, tlr.line_index;
            """;

        IEnumerable<WinningDetailRow> rows = await connection.QueryAsync<WinningDetailRow>(
            new CommandDefinition(
                detailsSql,
                new { TenantId = tenantId, DrawIds = drawIds.ToArray(), WinningStatuses },
                cancellationToken: cancellationToken));

        return rows
            .GroupBy(x => x.DrawId)
            .ToDictionary(
                x => x.Key,
                x => x.Select(
                        y => new WinningDetailDto
                        {
                            TicketId = y.TicketId,
                            TicketLineIndex = y.TicketLineIndex,
                            MemberUserId = y.MemberUserId,
                            MemberName = y.MemberName,
                            Numbers = y.Numbers,
                            WinningNumbers = y.WinningNumbers,
                            ParticipationStatus = y.ParticipationStatus,
                            PayoutAmount = y.PayoutAmount,
                            SettledAtUtc = y.SettledAtUtc,
                            RedeemedAtUtc = y.RedeemedAtUtc,
                            CreatedAtUtc = y.CreatedAtUtc
                        })
                    .ToList());
    }

    public sealed class WinningDetailRow
    {
        public Guid DrawId { get; set; }
        public Guid TicketId { get; set; }
        public int? TicketLineIndex { get; set; }
        public Guid? MemberUserId { get; set; }
        public string? MemberName { get; set; }
        public string? Numbers { get; set; }
        public string? WinningNumbers { get; set; }
        public int ParticipationStatus { get; set; }
        public decimal PayoutAmount { get; set; }
        public DateTime? SettledAtUtc { get; set; }
        public DateTime? RedeemedAtUtc { get; set; }
        public DateTime? CreatedAtUtc { get; set; }
    }
}
