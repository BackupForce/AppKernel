using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Time;
using Dapper;
using Domain.Gaming.Tickets;
using SharedKernel;

namespace Application.Gaming.Draws.GetCurrentDrawHotBalls;

internal sealed class GetCurrentDrawHotBallsQueryHandler(
    IDbConnectionFactory dbConnectionFactory,
    ITenantContext tenantContext,
    IDateTimeProvider dateTimeProvider)
    : IQueryHandler<GetCurrentDrawHotBallsQuery, IReadOnlyCollection<HotBallDto>>
{
    private static readonly int[] ValidParticipationStatuses =
    [
        (int)TicketDrawParticipationStatus.Active,
        (int)TicketDrawParticipationStatus.Settled,
        (int)TicketDrawParticipationStatus.Redeemed
    ];

    public async Task<Result<IReadOnlyCollection<HotBallDto>>> Handle(
        GetCurrentDrawHotBallsQuery request,
        CancellationToken cancellationToken)
    {
        const string sql = """
            WITH current_draw AS (
                SELECT d.id
                FROM gaming.draws d
                WHERE d.tenant_id = @TenantId
                  AND d.status <> 4
                  AND d.is_manually_closed = FALSE
                  AND @NowUtc >= d.sales_open_at
                  AND @NowUtc < d.sales_close_at
                ORDER BY d.sales_open_at DESC
                LIMIT 1
            ),
            line_numbers AS (
                SELECT CAST(TRIM(number_token) AS INTEGER) AS number
                FROM current_draw cd
                JOIN gaming.ticket_draws td
                  ON td.draw_id = cd.id
                 AND td.tenant_id = @TenantId
                JOIN gaming.tickets t
                  ON t.id = td.ticket_id
                 AND t.tenant_id = @TenantId
                JOIN gaming.ticket_lines tl
                  ON tl.ticket_id = t.id
                CROSS JOIN LATERAL regexp_split_to_table(tl.numbers_raw, ',') AS number_token
                WHERE t.submission_status = @SubmittedStatus
                  AND td.participation_status = ANY(@ValidParticipationStatuses)
            )
            SELECT
                ln.number AS Number,
                COUNT(*)::bigint AS BetCount
            FROM line_numbers ln
            GROUP BY ln.number
            ORDER BY BetCount DESC, Number ASC;
            """;

        using System.Data.IDbConnection connection = dbConnectionFactory.GetOpenConnection();

        List<HotBallDto> items = (await connection.QueryAsync<HotBallDto>(new CommandDefinition(
            sql,
            new
            {
                TenantId = tenantContext.TenantId,
                NowUtc = dateTimeProvider.UtcNow,
                SubmittedStatus = TicketSubmissionStatus.Submitted,
                ValidParticipationStatuses
            },
            cancellationToken: cancellationToken))).AsList();

        return items;
    }
}
