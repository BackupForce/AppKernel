using System.Data;
using System.Globalization;
using System.Text;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Gaming.Dtos;
using Dapper;
using Domain.Gaming.Tickets;
using SharedKernel;

namespace Application.Gaming.Tickets.Admin.Winnings;

internal sealed class GetAdminWinningsByDrawIdQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetAdminWinningsByDrawIdQuery, PagedResult<AdminWinningListItemDto>>
{
    public async Task<Result<PagedResult<AdminWinningListItemDto>>> Handle(
        GetAdminWinningsByDrawIdQuery request,
        CancellationToken cancellationToken)
    {
        StringBuilder builder = new StringBuilder(
            """
            SELECT
                tlr.id AS WinningId,
                tlr.ticket_id AS TicketId,
                tlr.draw_id AS DrawId,
                m.member_no AS MemberNumber,
                m.display_name AS MemberDisplayName,
                d.draw_code AS DrawCode,
                ppi.prize_name_snapshot AS PrizeName,
                d.draw_at AS DrawDateUtc,
                tlr.payout AS PayoutAmount,
                td.participation_status AS Status,
                tlr.redeemed_at_utc AS RedeemedAtUtc,
                tlr.redeemed_by_user_id AS RedeemedByUserId,
                u.name AS RedeemedByUserName
            FROM gaming.ticket_line_results tlr
            JOIN gaming.tickets t
                ON t.id = tlr.ticket_id
               AND t.tenant_id = tlr.tenant_id
            JOIN gaming.ticket_draws td
                ON td.tenant_id = tlr.tenant_id
               AND td.ticket_id = tlr.ticket_id
               AND td.draw_id = tlr.draw_id
            JOIN gaming.draws d
                ON d.id = tlr.draw_id
               AND d.tenant_id = tlr.tenant_id
            LEFT JOIN members m
                ON m.id = t.member_id
               AND m.tenant_id = t.tenant_id
            LEFT JOIN gaming.draw_prize_pool_items ppi
                ON ppi.draw_id = tlr.draw_id
               AND ppi.tier = tlr.prize_tier
               AND ppi.tenant_id = tlr.tenant_id
            LEFT JOIN users u
                ON u.id = tlr.redeemed_by_user_id
            WHERE tlr.tenant_id = @TenantId
              AND tlr.draw_id = @DrawId
            """);

        DynamicParameters parameters = new DynamicParameters();
        parameters.Add("TenantId", request.TenantId);
        parameters.Add("DrawId", request.DrawId);

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            builder.Append(
                """
                 AND (
                    CAST(tlr.ticket_id AS text) ILIKE @Keyword
                    OR CAST(tlr.id AS text) ILIKE @Keyword
                    OR COALESCE(m.member_no, '') ILIKE @Keyword
                    OR COALESCE(m.display_name, '') ILIKE @Keyword
                    OR COALESCE(d.draw_code, '') ILIKE @Keyword
                    OR COALESCE(ppi.prize_name_snapshot, '') ILIKE @Keyword
                    OR COALESCE(u.name, '') ILIKE @Keyword
                 )
                """);
            parameters.Add("Keyword", $"%{request.Keyword.Trim()}%");
        }

        if (request.Redeemed.HasValue)
        {
            if (request.Redeemed.Value)
            {
                builder.Append(" AND tlr.redeemed_at_utc IS NOT NULL");
            }
            else
            {
                builder.Append(" AND tlr.redeemed_at_utc IS NULL");
            }
        }

        string baseSql = builder.ToString();
        string listSql = $"""
            {baseSql}
            ORDER BY d.draw_at DESC, tlr.payout DESC, tlr.settled_at_utc DESC
            LIMIT @PageSize OFFSET @Offset
            """;

        parameters.Add("PageSize", request.PageSize);
        parameters.Add("Offset", (request.Current - 1) * request.PageSize);

        using IDbConnection connection = dbConnectionFactory.GetOpenConnection();

        IEnumerable<AdminWinningListItemDto> items = await connection.QueryAsync<AdminWinningListItemDto>(listSql, parameters);
        int total = await connection.ExecuteScalarAsync<int>(
            string.Format(CultureInfo.InvariantCulture, "SELECT COUNT(*) FROM ({0}) AS counted", baseSql),
            parameters);

        return PagedResult<AdminWinningListItemDto>.Create(items, total, request.Current, request.PageSize);
    }
}
