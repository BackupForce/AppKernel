using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Dapper;
using Domain.Gaming.Tickets;
using SharedKernel;

namespace Application.Gaming.Draws.GetDrawBetNumberStats;

internal sealed class GetDrawBetNumberStatsQueryHandler(
    IDbConnectionFactory dbConnectionFactory,
    ITenantContext tenantContext)
    : IQueryHandler<GetDrawBetNumberStatsQuery, IReadOnlyCollection<DrawBetNumberStatDto>>
{
    public async Task<Result<IReadOnlyCollection<DrawBetNumberStatDto>>> Handle(
        GetDrawBetNumberStatsQuery request,
        CancellationToken cancellationToken)
    {
        const string sql = """
            WITH selected_draw AS (
                SELECT d.id
                FROM gaming.draws d
                WHERE d.tenant_id = @TenantId
                  AND d.id = @DrawId
            ),
            line_numbers AS (
                SELECT CAST(TRIM(number_token) AS INTEGER) AS number
                FROM selected_draw sd
                JOIN gaming.ticket_draws td
                  ON td.draw_id = sd.id
                 AND td.tenant_id = @TenantId
                JOIN gaming.tickets t
                  ON t.id = td.ticket_id
                 AND t.tenant_id = @TenantId
                JOIN gaming.ticket_lines tl
                  ON tl.ticket_id = t.id
                CROSS JOIN LATERAL regexp_split_to_table(tl.numbers_raw, ',') AS number_token
                WHERE t.submission_status = @SubmittedStatus
            )
            SELECT
                ln.number AS Number,
                COUNT(*)::int AS BetCount
            FROM line_numbers ln
            GROUP BY ln.number
            ORDER BY BetCount DESC, Number ASC;
            """;

        using System.Data.IDbConnection connection = dbConnectionFactory.GetOpenConnection();

        List<DrawBetNumberStatDto> items = (await connection.QueryAsync<DrawBetNumberStatDto>(new CommandDefinition(
            sql,
            new
            {
                TenantId = tenantContext.TenantId,
                request.DrawId,
                SubmittedStatus = TicketSubmissionStatus.Submitted
            },
            cancellationToken: cancellationToken))).AsList();

        return items;
    }
}
