using System.Data;
using System.Linq;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Dapper;
using Domain.Gaming.Shared;
using Domain.Gaming.Tickets;
using Domain.Members;
using SharedKernel;

namespace Application.Gaming.Tickets.GetMyWinningTickets;

internal sealed class GetMyWinningTicketsQueryHandler(
    IDbConnectionFactory dbConnectionFactory,
    IMemberRepository memberRepository,
    ITenantContext tenantContext,
    IUserContext userContext) : IQueryHandler<GetMyWinningTicketsQuery, MyWinningTicketsDto>
{
    private sealed record WinningTicketRow(
        Guid TicketId,
        string GameCode,
        int SubmissionStatus,
        DateTime IssuedAtUtc,
        DateTime? SubmittedAtUtc,
        DateTime? ExpiresAtUtc,
        Guid DrawId,
        string DrawCode,
        DateTime DrawAtUtc,
        int ParticipationStatus,
        string? WinningNumbers,
        string PrizeName,
        string? PrizeCode,
        decimal? PrizeAmount);

    private static IReadOnlyCollection<TicketDrawParticipationStatus> GetWinningStatuses() =>
        new[] { TicketDrawParticipationStatus.Settled, TicketDrawParticipationStatus.Redeemed };

    public async Task<Result<MyWinningTicketsDto>> Handle(
        GetMyWinningTicketsQuery request,
        CancellationToken cancellationToken)
    {
        Member? member = await memberRepository.GetByUserIdAsync(
            tenantContext.TenantId,
            userContext.UserId,
            cancellationToken);
        if (member is null)
        {
            return Result.Failure<MyWinningTicketsDto>(GamingErrors.MemberNotFound);
        }

        IReadOnlyCollection<TicketDrawParticipationStatus> winningStatuses = GetWinningStatuses();

        const string sql = """
            SELECT
                t.id AS TicketId,
                t.game_code AS GameCode,
                t.submission_status AS SubmissionStatus,
                t.issued_at_utc AS IssuedAtUtc,
                t.submitted_at_utc AS SubmittedAtUtc,
                CASE
                    WHEN dg.id IS NOT NULL THEN dg.grant_close_at_utc
                    WHEN d.id IS NOT NULL THEN COALESCE(d.manual_close_at, d.sales_close_at)
                    ELSE NULL
                END AS ExpiresAtUtc,
                d.id AS DrawId,
                d.draw_code AS DrawCode,
                d.draw_at AS DrawAtUtc,
                td.participation_status AS ParticipationStatus,
                d.winning_numbers_raw AS WinningNumbers,
                ppi.prize_name_snapshot AS PrizeName,
                NULL::text AS PrizeCode,
                tlr.payout AS PrizeAmount
            FROM gaming.tickets t
            JOIN gaming.ticket_draws td
                ON td.ticket_id = t.id
               AND td.tenant_id = t.tenant_id
            JOIN gaming.draws d
                ON d.id = td.draw_id
               AND d.tenant_id = t.tenant_id
            JOIN gaming.ticket_line_results tlr
                ON tlr.ticket_id = t.id
               AND tlr.draw_id = d.id
               AND tlr.tenant_id = t.tenant_id
            JOIN gaming.draw_prize_pool_items ppi
                ON ppi.draw_id = d.id
               AND ppi.tier = tlr.prize_tier
               AND ppi.tenant_id = t.tenant_id
            LEFT JOIN gaming.draw_groups dg
                ON dg.id = t.draw_group_id
               AND dg.tenant_id = t.tenant_id
            WHERE t.tenant_id = @TenantId
              AND t.member_id = @MemberId
              AND td.participation_status = ANY(@WinningStatuses)
            ORDER BY d.draw_at DESC, t.issued_at_utc DESC
            OFFSET @Offset LIMIT @Limit
            """;

        const string countSql = """
            SELECT COUNT(DISTINCT t.id)
            FROM gaming.tickets t
            JOIN gaming.ticket_draws td
                ON td.ticket_id = t.id
               AND td.tenant_id = t.tenant_id
            JOIN gaming.ticket_line_results tlr
                ON tlr.ticket_id = t.id
               AND tlr.draw_id = td.draw_id
               AND tlr.tenant_id = t.tenant_id
            WHERE t.tenant_id = @TenantId
              AND t.member_id = @MemberId
              AND td.participation_status = ANY(@WinningStatuses)
            """;

        var parameters = new
        {
            tenantContext.TenantId,
            MemberId = member.Id,
            WinningStatuses = winningStatuses.Select(status => (int)status).ToArray(),
            Offset = (request.Page - 1) * request.PageSize,
            Limit = request.PageSize
        };

        using IDbConnection connection = dbConnectionFactory.GetOpenConnection();

        IEnumerable<WinningTicketRow> rows = await connection.QueryAsync<WinningTicketRow>(sql, parameters);
        int totalCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);

        Dictionary<Guid, MyWinningTicketItemDto> ticketMap = new();
        Dictionary<Guid, List<MyWinningTicketDrawDto>> drawMap = new();

        foreach (WinningTicketRow row in rows)
        {
            if (!ticketMap.ContainsKey(row.TicketId))
            {
                ticketMap[row.TicketId] = new MyWinningTicketItemDto(
                    row.TicketId,
                    row.GameCode,
                    row.SubmissionStatus,
                    row.IssuedAtUtc,
                    row.SubmittedAtUtc,
                    row.ExpiresAtUtc,
                    Array.Empty<MyWinningTicketDrawDto>());
                drawMap[row.TicketId] = new List<MyWinningTicketDrawDto>();
            }

            List<MyWinningTicketDrawDto> draws = drawMap[row.TicketId];
            MyWinningTicketDrawDto? existing = draws.FirstOrDefault(draw => draw.DrawId == row.DrawId);

            MyWinningTicketDrawDto candidate = new(
                row.DrawId,
                row.DrawCode,
                row.DrawAtUtc,
                row.ParticipationStatus,
                row.WinningNumbers,
                row.PrizeName,
                row.PrizeCode,
                row.PrizeAmount);

            if (existing is null)
            {
                draws.Add(candidate);
            }
            else if ((row.PrizeAmount ?? 0m) > (existing.PrizeAmount ?? 0m))
            {
                int index = draws.IndexOf(existing);
                draws[index] = candidate;
            }
        }

        List<MyWinningTicketItemDto> items = new();
        foreach (KeyValuePair<Guid, MyWinningTicketItemDto> entry in ticketMap)
        {
            IReadOnlyList<MyWinningTicketDrawDto> draws = drawMap[entry.Key];
            items.Add(entry.Value with { Draws = draws });
        }

        MyWinningTicketsDto result = new(items, request.Page, request.PageSize, totalCount);
        return result;
    }
}
