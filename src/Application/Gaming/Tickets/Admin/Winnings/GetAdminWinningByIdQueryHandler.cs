using System.Data;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Gaming.Dtos;
using Dapper;
using Domain.Gaming.Shared;
using SharedKernel;

namespace Application.Gaming.Tickets.Admin.Winnings;

internal sealed class GetAdminWinningByIdQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetAdminWinningByIdQuery, AdminWinningDetailDto>
{
    public async Task<Result<AdminWinningDetailDto>> Handle(GetAdminWinningByIdQuery request, CancellationToken cancellationToken)
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

        AdminWinningDetailDto? item = await connection.QuerySingleOrDefaultAsync<AdminWinningDetailDto>(
            sql,
            new { request.TenantId, request.WinningId });

        if (item is null)
        {
            return Result.Failure<AdminWinningDetailDto>(GamingErrors.TicketLineResultNotFound);
        }

        return Result.Success(item);
    }
}
