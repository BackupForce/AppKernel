using System.Data;
using System.Globalization;
using System.Text;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Gaming.Dtos;
using Dapper;
using Domain.Gaming.Tickets;
using SharedKernel;

namespace Application.Gaming.Tickets.Admin;

internal sealed class GetWinningTicketsQueryHandler(
    IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetWinningTicketsQuery, PagedResult<WinningTicketListItemDto>>
{
    public async Task<Result<PagedResult<WinningTicketListItemDto>>> Handle(
        GetWinningTicketsQuery request,
        CancellationToken cancellationToken)
    {
        var builder = new StringBuilder(
            """
            SELECT
                d.id AS DrawId,
                d.draw_code AS DrawCode,
                d.draw_at AS DrawAtUtc,
                t.draw_group_id AS DrawGroupId,
                dg.name AS DrawGroupName,
                t.game_code AS GameCode,
                t.id AS TicketId,
                t.member_id AS MemberId,
                m.member_no AS MemberNo,
                m.display_name AS MemberDisplayName,
                td.participation_status AS ParticipationStatus,
                tlr.prize_tier AS WinningTier,
                tlr.payout AS PrizeAmount,
                td.redeemed_at_utc AS RedeemedAtUtc,
                t.issued_at_utc AS IssuedAtUtc,
                t.submitted_at_utc AS SubmittedAtUtc
            FROM gaming.ticket_draws td
            JOIN gaming.tickets t
                ON t.id = td.ticket_id
               AND t.tenant_id = @TenantId
            JOIN gaming.draws d
                ON d.id = td.draw_id
               AND d.tenant_id = @TenantId
            JOIN members m
                ON m.id = t.member_id
               AND m.tenant_id = @TenantId
            JOIN gaming.ticket_line_results tlr
                ON tlr.ticket_id = t.id
               AND tlr.draw_id = d.id
               AND tlr.tenant_id = @TenantId
            LEFT JOIN gaming.draw_groups dg
                ON dg.id = t.draw_group_id
               AND dg.tenant_id = @TenantId
            WHERE td.tenant_id = @TenantId
              AND td.participation_status IN (@SettledStatus, @RedeemedStatus)
              AND d.settled_at_utc IS NOT NULL
            """);

        var parameters = new DynamicParameters();
        parameters.Add("TenantId", request.TenantId);
        parameters.Add("SettledStatus", TicketDrawParticipationStatus.Settled);
        parameters.Add("RedeemedStatus", TicketDrawParticipationStatus.Redeemed);

        if (!string.IsNullOrWhiteSpace(request.GameCode))
        {
            builder.Append(" AND t.game_code = @GameCode");
            parameters.Add("GameCode", request.GameCode.Trim());
        }

        if (request.DrawId.HasValue)
        {
            builder.Append(" AND d.id = @DrawId");
            parameters.Add("DrawId", request.DrawId.Value);
        }

        if (request.DrawGroupId.HasValue)
        {
            builder.Append(" AND t.draw_group_id = @DrawGroupId");
            parameters.Add("DrawGroupId", request.DrawGroupId.Value);
        }

        if (request.DrawAtFromUtc.HasValue)
        {
            builder.Append(" AND d.draw_at >= @DrawAtFromUtc");
            parameters.Add("DrawAtFromUtc", request.DrawAtFromUtc.Value);
        }

        if (request.DrawAtToUtc.HasValue)
        {
            builder.Append(" AND d.draw_at <= @DrawAtToUtc");
            parameters.Add("DrawAtToUtc", request.DrawAtToUtc.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            builder.Append(
                """
                 AND (
                     m.member_no ILIKE @Keyword
                     OR m.display_name ILIKE @Keyword
                 )
                """);
            parameters.Add("Keyword", $"%{request.Keyword.Trim()}%");
        }

        switch (request.RedemptionStatus)
        {
            case RedemptionStatusFilter.RedeemedOnly:
                builder.Append(" AND td.participation_status = @RedeemedStatus");
                break;
            case RedemptionStatusFilter.UnredeemedOnly:
                builder.Append(" AND td.participation_status = @SettledStatus");
                break;
            case RedemptionStatusFilter.All:
            default:
                break;
        }

        const string countSql = "SELECT COUNT(*) FROM ({0}) AS counted";
        string baseSql = builder.ToString();
        string finalSql = $"""
            {baseSql}
            ORDER BY d.draw_at DESC, tlr.payout DESC, t.issued_at_utc DESC
            LIMIT @PageSize OFFSET @Offset
            """;

        parameters.Add("PageSize", request.PageSize);
        parameters.Add("Offset", (request.Page - 1) * request.PageSize);

        using IDbConnection connection = dbConnectionFactory.GetOpenConnection();

        IEnumerable<WinningTicketListItemDto> items = await connection.QueryAsync<WinningTicketListItemDto>(
            finalSql,
            parameters);
        int totalCount = await connection.ExecuteScalarAsync<int>(
            string.Format(CultureInfo.InvariantCulture, countSql, baseSql),
            parameters);

        return PagedResult<WinningTicketListItemDto>.Create(items, totalCount, request.Page, request.PageSize);
    }
}
