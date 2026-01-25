using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Time;
using Application.Gaming.Dtos;
using Domain.Gaming.Tickets;
using Domain.Gaming.Shared;
using Domain.Members;
using Dapper;
using SharedKernel;

namespace Application.Gaming.Tickets.AvailableForBet;

internal sealed class GetAvailableTicketsForBetQueryHandler(
    IDbConnectionFactory dbConnectionFactory,
    IMemberRepository memberRepository,
    ITenantContext tenantContext,
    IUserContext userContext,
    IDateTimeProvider dateTimeProvider)
    : IQueryHandler<GetAvailableTicketsForBetQuery, AvailableTicketsResponse>
{
    private const int DefaultLimit = 200;

    private sealed record TicketRow(
        Guid TicketId,
        string GameCode,
        string? PlayTypeCode,
        Guid DrawId,
        DateTime SalesCloseAtUtc);

    public async Task<Result<AvailableTicketsResponse>> Handle(
        GetAvailableTicketsForBetQuery request,
        CancellationToken cancellationToken)
    {
        Member? member = await memberRepository.GetByUserIdAsync(
            tenantContext.TenantId,
            userContext.UserId,
            cancellationToken);
        if (member is null)
        {
            return Result.Failure<AvailableTicketsResponse>(GamingErrors.MemberNotFound);
        }

        int limit = request.Limit switch
        {
            > 0 and <= DefaultLimit => request.Limit.Value,
            > DefaultLimit => DefaultLimit,
            _ => DefaultLimit
        };

        const string sql = """
            SELECT
                t.id AS TicketId,
                t.game_code AS GameCode,
                t.play_type_code AS PlayTypeCode,
                t.draw_id AS DrawId,
                d.sales_close_at AS SalesCloseAtUtc
            FROM gaming.tickets t
            INNER JOIN gaming.draws d ON d.id = t.draw_id
            WHERE t.tenant_id = @TenantId
              AND t.member_id = @MemberId
              AND t.submission_status = @SubmissionStatus
              AND d.is_manually_closed = FALSE
              AND d.sales_close_at > @NowUtc
              AND (@DrawId IS NULL OR t.draw_id = @DrawId)
              AND NOT EXISTS (
                  SELECT 1
                  FROM gaming.ticket_lines l
                  WHERE l.ticket_id = t.id
              )
            ORDER BY d.sales_close_at ASC, t.created_at DESC
            LIMIT @Limit
            """;

        using System.Data.IDbConnection connection = dbConnectionFactory.GetOpenConnection();

        IEnumerable<TicketRow> rows = await connection.QueryAsync<TicketRow>(
            sql,
            new
            {
                tenantContext.TenantId,
                MemberId = member.Id,
                SubmissionStatus = TicketSubmissionStatus.NotSubmitted,
                NowUtc = dateTimeProvider.UtcNow,
                request.DrawId,
                Limit = limit
            });

        List<AvailableTicketItemDto> items = new List<AvailableTicketItemDto>();
        foreach (TicketRow row in rows)
        {
            string displayText = BuildDisplayText(row);
            items.Add(new AvailableTicketItemDto(
                row.TicketId,
                displayText,
                row.GameCode,
                row.PlayTypeCode,
                row.DrawId,
                row.SalesCloseAtUtc,
                null));
        }

        return new AvailableTicketsResponse(items);
    }

    private static string BuildDisplayText(TicketRow row)
    {
        List<string> segments = new List<string>
        {
            $"Ticket {row.TicketId:N}",
            row.GameCode
        };

        if (!string.IsNullOrWhiteSpace(row.PlayTypeCode))
        {
            segments.Add(row.PlayTypeCode);
        }

        segments.Add($"Close {row.SalesCloseAtUtc:O}");

        return string.Join(" | ", segments);
    }
}
