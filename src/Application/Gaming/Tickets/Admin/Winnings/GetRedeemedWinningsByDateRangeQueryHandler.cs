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

internal sealed class GetRedeemedWinningsByDateRangeQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetRedeemedWinningsByDateRangeQuery, PagedResult<RedeemedWinningListItemDto>>
{
    public async Task<Result<PagedResult<RedeemedWinningListItemDto>>> Handle(
        GetRedeemedWinningsByDateRangeQuery request,
        CancellationToken cancellationToken)
    {
        StringBuilder builder = new StringBuilder(
            """
            SELECT
                tlr.id AS WinningId,
                tlr.ticket_id AS TicketId,
                t.member_id AS MemberId,
                m.member_no AS MemberCode,
                m.display_name AS MemberDisplayName,
                mp.phone_number AS MemberPhoneNumber,
                d.draw_code AS DrawCode,
                ppi.prize_name_snapshot AS PrizeName,
                tlr.payout AS PrizeAmount,
                td.participation_status AS Status,
                tlr.redeemed_at_utc AS RedeemedAtUtc,
                u.name AS RedeemedBy
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
            LEFT JOIN member_profiles mp
                ON mp.member_id = m.id
            LEFT JOIN gaming.draw_prize_pool_items ppi
                ON ppi.draw_id = tlr.draw_id
               AND ppi.tier = tlr.prize_tier
               AND ppi.tenant_id = tlr.tenant_id
            LEFT JOIN users u
                ON u.id = tlr.redeemed_by_user_id
            WHERE tlr.tenant_id = @TenantId
              AND td.participation_status = @RedeemedStatus
              AND tlr.redeemed_at_utc IS NOT NULL
            """);

        DynamicParameters parameters = new DynamicParameters();
        parameters.Add("TenantId", request.TenantId);
        parameters.Add("RedeemedStatus", TicketDrawParticipationStatus.Redeemed);

        if (request.RedeemedFromUtc.HasValue)
        {
            builder.Append(" AND tlr.redeemed_at_utc >= @RedeemedFromUtc");
            parameters.Add("RedeemedFromUtc", request.RedeemedFromUtc.Value);
        }

        if (request.RedeemedToUtc.HasValue)
        {
            builder.Append(" AND tlr.redeemed_at_utc <= @RedeemedToUtc");
            parameters.Add("RedeemedToUtc", request.RedeemedToUtc.Value);
        }

        string baseSql = builder.ToString();
        string listSql = $"""
            {baseSql}
            ORDER BY tlr.redeemed_at_utc DESC, tlr.payout DESC
            LIMIT @PageSize OFFSET @Offset
            """;

        parameters.Add("PageSize", request.PageSize);
        parameters.Add("Offset", (request.Page - 1) * request.PageSize);

        using IDbConnection connection = dbConnectionFactory.GetOpenConnection();

        IEnumerable<RedeemedWinningListItemDto> items = await connection.QueryAsync<RedeemedWinningListItemDto>(listSql, parameters);
        int totalCount = await connection.ExecuteScalarAsync<int>(
            string.Format(CultureInfo.InvariantCulture, "SELECT COUNT(*) FROM ({0}) AS counted", baseSql),
            parameters);

        return PagedResult<RedeemedWinningListItemDto>.Create(items, totalCount, request.Page, request.PageSize);
    }
}
