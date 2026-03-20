using Domain.Gaming.Shared;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Gaming.Dtos;
using Dapper;
using SharedKernel;

namespace Application.Gaming.Tickets.Admin;

internal sealed class GetMemberTicketsQueryHandler(
    IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetMemberTicketsQuery, PagedResult<DrawTicketBetDto>>
{
    public async Task<Result<PagedResult<DrawTicketBetDto>>> Handle(
        GetMemberTicketsQuery request,
        CancellationToken cancellationToken)
    {
        const string memberSql = """
            SELECT m.id
            FROM members m
            WHERE m.tenant_id = @TenantId AND m.id = @MemberId
            """;

        const string countSql = """
            SELECT COUNT(*)
            FROM gaming.tickets t
            WHERE t.tenant_id = @TenantId
              AND t.member_id = @MemberId
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
                    latest_td.participation_status AS ParticipationStatus
                FROM gaming.tickets t
                LEFT JOIN LATERAL (
                    SELECT td.participation_status
                    FROM gaming.ticket_draws td
                    WHERE td.tenant_id = @TenantId
                      AND td.ticket_id = t.id
                    ORDER BY td.created_at_utc DESC, td.id DESC
                    LIMIT 1
                ) latest_td ON TRUE
                WHERE t.tenant_id = @TenantId
                  AND t.member_id = @MemberId
                ORDER BY t.issued_at_utc DESC, t.created_at DESC
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
                COALESCE(pt.ParticipationStatus, @DefaultParticipationStatus) AS ParticipationStatus,
                l.line_index AS LineIndex,
                l.play_type_code AS PlayTypeCode,
                l.numbers_raw AS Numbers
            FROM paged_tickets pt
            JOIN members m ON m.id = pt.MemberId AND m.tenant_id = @TenantId
            LEFT JOIN gaming.ticket_lines l ON l.ticket_id = pt.TicketId
            ORDER BY pt.IssuedAtUtc DESC, pt.TicketId ASC, l.line_index ASC
            """;

        using System.Data.IDbConnection connection = dbConnectionFactory.GetOpenConnection();

        Guid? memberId = await connection.QueryFirstOrDefaultAsync<Guid?>(new CommandDefinition(
            memberSql,
            new { request.TenantId, request.MemberId },
            cancellationToken: cancellationToken));

        if (!memberId.HasValue)
        {
            return Result.Failure<PagedResult<DrawTicketBetDto>>(GamingErrors.MemberNotFound);
        }

        int totalCount = await connection.ExecuteScalarAsync<int>(new CommandDefinition(
            countSql,
            new { request.TenantId, request.MemberId },
            cancellationToken: cancellationToken));

        if (totalCount == 0)
        {
            return PagedResult<DrawTicketBetDto>.Create(Array.Empty<DrawTicketBetDto>(), 0, request.Page, request.PageSize);
        }

        IEnumerable<TicketBetRow> rows = await connection.QueryAsync<TicketBetRow>(new CommandDefinition(
            sql,
            new
            {
                request.TenantId,
                request.MemberId,
                request.PageSize,
                Offset = (request.Page - 1) * request.PageSize,
                DefaultParticipationStatus = Domain.Gaming.Tickets.TicketDrawParticipationStatus.Pending
            },
            cancellationToken: cancellationToken));

        IReadOnlyList<DrawTicketBetDto> items = DrawTicketBetDtoAssembler.Assemble(rows);
        return PagedResult<DrawTicketBetDto>.Create(items, totalCount, request.Page, request.PageSize);
    }
}
