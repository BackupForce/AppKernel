using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Gaming.Dtos;
using Dapper;
using Domain.Gaming.Shared;
using Domain.Gaming.Tickets;
using SharedKernel;

namespace Application.Gaming.Tickets.Admin;

internal sealed class GetDrawTicketsQueryHandler(
    IDbConnectionFactory dbConnectionFactory,
    ITenantContext tenantContext)
    : IQueryHandler<GetDrawTicketsQuery, PagedResult<DrawTicketBetDto>>
{
    private sealed record TicketRow(
        Guid TicketId,
        Guid MemberId,
        string MemberNo,
        string DisplayName,
        string GameCode,
        TicketSubmissionStatus SubmissionStatus,
        DateTime IssuedAtUtc,
        DateTime? SubmittedAtUtc,
        TicketDrawParticipationStatus ParticipationStatus,
        int? LineIndex,
        string? PlayTypeCode,
        string? Numbers);

    public async Task<Result<PagedResult<DrawTicketBetDto>>> Handle(
        GetDrawTicketsQuery request,
        CancellationToken cancellationToken)
    {
        const string drawSql = """
            SELECT d.id
            FROM gaming.draws d
            WHERE d.tenant_id = @TenantId AND d.id = @DrawId
            """;

        const string countSql = """
            SELECT COUNT(*)
            FROM gaming.ticket_draws td
            JOIN gaming.tickets t ON t.id = td.ticket_id
            WHERE td.tenant_id = @TenantId
              AND td.draw_id = @DrawId
              AND t.tenant_id = @TenantId
              AND t.submission_status = @SubmissionStatus
            """;

        const string sql = """
            WITH paged_tickets AS (
                SELECT
                    t.id AS TicketId,
                    t.member_id AS MemberId,
                    t.game_code AS GameCode,
                    t.submission_status AS SubmissionStatus,
                    t.issued_at_utc AS IssuedAtUtc,
                    t.submitted_at_utc AS SubmittedAtUtc,
                    td.participation_status AS ParticipationStatus
                FROM gaming.ticket_draws td
                JOIN gaming.tickets t ON t.id = td.ticket_id
                WHERE td.tenant_id = @TenantId
                  AND td.draw_id = @DrawId
                  AND t.tenant_id = @TenantId
                  AND t.submission_status = @SubmissionStatus
                ORDER BY t.submitted_at_utc DESC NULLS LAST, t.issued_at_utc DESC
                LIMIT @PageSize OFFSET @Offset
            )
            SELECT
                pt.TicketId,
                pt.MemberId,
                m.member_no AS MemberNo,
                m.display_name AS DisplayName,
                pt.GameCode,
                pt.SubmissionStatus,
                pt.IssuedAtUtc,
                pt.SubmittedAtUtc,
                pt.ParticipationStatus,
                l.line_index AS LineIndex,
                l.play_type_code AS PlayTypeCode,
                l.numbers AS Numbers
            FROM paged_tickets pt
            JOIN members m ON m.id = pt.MemberId AND m.tenant_id = @TenantId
            LEFT JOIN gaming.ticket_lines l ON l.ticket_id = pt.TicketId
            ORDER BY pt.SubmittedAtUtc DESC NULLS LAST, pt.IssuedAtUtc DESC, l.line_index ASC
            """;

        using System.Data.IDbConnection connection = dbConnectionFactory.GetOpenConnection();

        Guid? drawId = await connection.QueryFirstOrDefaultAsync<Guid?>(
            drawSql,
            new { tenantContext.TenantId, request.DrawId });

        if (!drawId.HasValue)
        {
            return Result.Failure<PagedResult<DrawTicketBetDto>>(GamingErrors.DrawNotFound);
        }

        int totalCount = await connection.ExecuteScalarAsync<int>(
            countSql,
            new
            {
                tenantContext.TenantId,
                request.DrawId,
                SubmissionStatus = TicketSubmissionStatus.Submitted
            });

        if (totalCount == 0)
        {
            return PagedResult<DrawTicketBetDto>.Create(
                Array.Empty<DrawTicketBetDto>(),
                0,
                request.Page,
                request.PageSize);
        }

        IEnumerable<TicketRow> rows = await connection.QueryAsync<TicketRow>(
            sql,
            new
            {
                tenantContext.TenantId,
                request.DrawId,
                SubmissionStatus = TicketSubmissionStatus.Submitted,
                request.PageSize,
                Offset = (request.Page - 1) * request.PageSize
            });

        Dictionary<Guid, DrawTicketBetDto> ticketMap = new();
        Dictionary<Guid, List<TicketLineDetailDto>> lineMap = new();
        List<Guid> ticketOrder = new();

        foreach (TicketRow row in rows)
        {
            if (!ticketMap.ContainsKey(row.TicketId))
            {
                ticketMap[row.TicketId] = new DrawTicketBetDto(
                    row.TicketId,
                    row.MemberId,
                    row.MemberNo,
                    row.DisplayName,
                    row.GameCode,
                    row.SubmissionStatus,
                    row.IssuedAtUtc,
                    row.SubmittedAtUtc,
                    row.ParticipationStatus,
                    Array.Empty<TicketLineDetailDto>());
                lineMap[row.TicketId] = new List<TicketLineDetailDto>();
                ticketOrder.Add(row.TicketId);
            }

            if (row.LineIndex.HasValue
                && !string.IsNullOrWhiteSpace(row.PlayTypeCode)
                && !string.IsNullOrWhiteSpace(row.Numbers)
                && lineMap[row.TicketId].TrueForAll(item => item.LineIndex != row.LineIndex.Value))
            {
                lineMap[row.TicketId].Add(new TicketLineDetailDto(
                    row.LineIndex.Value,
                    row.PlayTypeCode,
                    row.Numbers));
            }
        }

        List<DrawTicketBetDto> items = new();
        foreach (Guid ticketId in ticketOrder)
        {
            DrawTicketBetDto ticket = ticketMap[ticketId];
            IReadOnlyCollection<TicketLineDetailDto> lines = lineMap[ticketId];
            items.Add(ticket with { Lines = lines });
        }

        return PagedResult<DrawTicketBetDto>.Create(items, totalCount, request.Page, request.PageSize);
    }
}
