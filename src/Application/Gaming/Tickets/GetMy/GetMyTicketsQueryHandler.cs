using System.Data;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Gaming;
using Application.Abstractions.Messaging;
using Application.Gaming.Dtos;
using Dapper;
using Domain.Gaming.Catalog;
using Domain.Gaming.Rules;
using Domain.Gaming.Shared;
using Domain.Gaming.Tickets;
using Domain.Members;
using SharedKernel;

namespace Application.Gaming.Tickets.GetMy;

internal sealed class GetMyTicketsQueryHandler(
    IDbConnectionFactory dbConnectionFactory,
    IMemberRepository memberRepository,
    ITenantContext tenantContext,
    IUserContext userContext,
    IEntitlementChecker entitlementChecker) : IQueryHandler<GetMyTicketsQuery, PagedResult<TicketSummaryDto>>
{
    private const int MaxPageSize = 100;

    private sealed record TicketRow(
        Guid TicketId,
        Guid? DrawGroupId,
        string DrawCode,
        string GameCode,
        string? PlayTypeCode,
        TicketSubmissionStatus SubmissionStatus,
        DateTime IssuedAtUtc,
        DateTime? SubmittedAtUtc,
        DateTime? ExpiresAtUtc,
        string? ClaimEventName,
        int? LineIndex,
        string? Numbers,
        Guid? DrawId,
        TicketDrawParticipationStatus? ParticipationStatus,
        DateTime? DrawAt,
        string? WinningNumbers);

    public async Task<Result<PagedResult<TicketSummaryDto>>> Handle(
        GetMyTicketsQuery request,
        CancellationToken cancellationToken)
    {
        if (request.PageNumber < 1)
        {
            return Result.Failure<PagedResult<TicketSummaryDto>>(
                Error.Validation("GetMyTickets.InvalidPageNumber", "PageNumber must be greater than or equal to 1."));
        }

        if (request.PageSize <= 0)
        {
            return Result.Failure<PagedResult<TicketSummaryDto>>(
                Error.Validation("GetMyTickets.InvalidPageSize", "PageSize must be greater than 0."));
        }

        int pageSize = request.PageSize;
        if (pageSize > MaxPageSize)
        {
            pageSize = MaxPageSize;
        }

        int offset = (request.PageNumber - 1) * pageSize;

        Result<GameCode> gameCodeResult = GameCode.Create(request.GameCode);
        if (gameCodeResult.IsFailure)
        {
            return Result.Failure<PagedResult<TicketSummaryDto>>(gameCodeResult.Error);
        }

        Result entitlementResult = await entitlementChecker.EnsureGameEnabledAsync(
            tenantContext.TenantId,
            gameCodeResult.Value,
            cancellationToken);
        if (entitlementResult.IsFailure)
        {
            return Result.Failure<PagedResult<TicketSummaryDto>>(entitlementResult.Error);
        }

        Member? member = await memberRepository.GetByUserIdAsync(tenantContext.TenantId, userContext.UserId, cancellationToken);
        if (member is null)
        {
            return Result.Failure<PagedResult<TicketSummaryDto>>(GamingErrors.MemberNotFound);
        }

        const string countSql = """
            SELECT COUNT(*)
            FROM gaming.tickets t
            WHERE t.tenant_id = @TenantId
              AND t.member_id = @MemberId
              AND t.game_code = @GameCode::varchar(32)
              AND (@From::timestamptz IS NULL OR t.issued_at_utc >= @From::timestamptz)
              AND (@To::timestamptz IS NULL OR t.issued_at_utc <= @To::timestamptz)
            """;

        const string dataSql = """
            WITH paged_tickets AS (
                SELECT
                    t.id,
                    t.draw_group_id,
                    t.game_code,
                    t.submission_status,
                    t.issued_at_utc,
                    t.submitted_at_utc,
                    t.draw_id,
                    t.tenant_id
                FROM gaming.tickets t
                WHERE t.tenant_id = @TenantId
                  AND t.member_id = @MemberId
                  AND t.game_code = @GameCode::varchar(32)
                  AND (@From::timestamptz IS NULL OR t.issued_at_utc >= @From::timestamptz)
                  AND (@To::timestamptz IS NULL OR t.issued_at_utc <= @To::timestamptz)
                ORDER BY t.issued_at_utc DESC
                OFFSET @Offset
                LIMIT @PageSize
            )
            SELECT
                pt.id AS TicketId,
                pt.draw_group_id AS DrawGroupId,
                d.draw_code AS DrawCode,
                pt.game_code AS GameCode,
                l.play_type_code AS PlayTypeCode,
                pt.submission_status AS SubmissionStatus,
                pt.issued_at_utc AS IssuedAtUtc,
                pt.submitted_at_utc AS SubmittedAtUtc,
                CASE
                    WHEN d_exp.id IS NOT NULL THEN COALESCE(d_exp.manual_close_at, d_exp.sales_close_at)
                    ELSE NULL
                END AS ExpiresAtUtc,
                tce.name AS ClaimEventName,
                l.line_index AS LineIndex,
                l.numbers_raw AS Numbers,
                td.draw_id AS DrawId,
                td.participation_status AS ParticipationStatus,
                d.draw_at AS DrawAt,
                d.winning_numbers_raw AS WinningNumbers
            FROM paged_tickets pt
            LEFT JOIN LATERAL (
                SELECT tce.name
                FROM gaming.ticket_claim_records tcr
                JOIN gaming.ticket_claim_events tce
                  ON tce.tenant_id = tcr.tenant_id
                 AND tce.id = tcr.event_id
                WHERE tcr.tenant_id = pt.tenant_id
                  AND tcr.member_id = @MemberId
                  AND tcr.issued_ticket_ids @> to_jsonb(ARRAY[pt.id::text])
                ORDER BY tcr.claimed_at_utc DESC
                LIMIT 1
            ) tce ON TRUE
            LEFT JOIN gaming.ticket_lines l ON l.ticket_id = pt.id
            LEFT JOIN gaming.ticket_draws td ON td.ticket_id = pt.id
            LEFT JOIN gaming.draws d ON d.id = td.draw_id
            LEFT JOIN gaming.draws d_exp
                   ON d_exp.id = pt.draw_id
                  AND d_exp.tenant_id = pt.tenant_id
            ORDER BY pt.issued_at_utc DESC
            """;

        DynamicParameters parameters = new DynamicParameters();
        parameters.Add("TenantId", tenantContext.TenantId);
        parameters.Add("MemberId", member.Id);
        parameters.Add("GameCode", gameCodeResult.Value.Value);
        parameters.Add("From", request.From);
        parameters.Add("To", request.To);
        parameters.Add("Offset", offset);
        parameters.Add("PageSize", pageSize);

        using IDbConnection connection = dbConnectionFactory.GetOpenConnection();

        int totalCount = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(countSql, parameters, cancellationToken: cancellationToken));

        IEnumerable<TicketRow> rows = await connection.QueryAsync<TicketRow>(
            new CommandDefinition(dataSql, parameters, cancellationToken: cancellationToken));

        Dictionary<Guid, TicketSummaryDto> ticketMap = new Dictionary<Guid, TicketSummaryDto>();
        Dictionary<Guid, List<TicketLineSummaryDto>> lineMap = new Dictionary<Guid, List<TicketLineSummaryDto>>();
        Dictionary<Guid, List<TicketDrawSummaryDto>> drawMap = new Dictionary<Guid, List<TicketDrawSummaryDto>>();

        foreach (TicketRow row in rows)
        {
            if (!ticketMap.ContainsKey(row.TicketId))
            {
                ticketMap[row.TicketId] = new TicketSummaryDto(
                    row.TicketId,
                    row.DrawGroupId,
                    row.DrawCode,
                    row.GameCode,
                    row.PlayTypeCode,
                    row.SubmissionStatus,
                    row.IssuedAtUtc,
                    row.SubmittedAtUtc,
                    row.ExpiresAtUtc,
                    row.ClaimEventName,
                    Array.Empty<TicketLineSummaryDto>(),
                    Array.Empty<TicketDrawSummaryDto>());
                lineMap[row.TicketId] = new List<TicketLineSummaryDto>();
                drawMap[row.TicketId] = new List<TicketDrawSummaryDto>();
            }

            if (row.LineIndex.HasValue && !string.IsNullOrWhiteSpace(row.Numbers))
            {
                bool hasLine = lineMap[row.TicketId].TrueForAll(item => item.LineIndex != row.LineIndex.Value);
                if (hasLine)
                {
                    lineMap[row.TicketId].Add(new TicketLineSummaryDto(row.LineIndex.Value, row.Numbers));
                }
            }

            if (row.DrawId.HasValue && row.ParticipationStatus.HasValue && row.DrawAt.HasValue)
            {
                int matchedCount = 0;
                if (!string.IsNullOrWhiteSpace(row.WinningNumbers) && !string.IsNullOrWhiteSpace(row.Numbers))
                {
                    Result<LotteryNumbers> winningResult = LotteryNumbers.Parse(row.WinningNumbers);
                    Result<LotteryNumbers> lineResult = LotteryNumbers.Parse(row.Numbers);
                    if (winningResult.IsSuccess && lineResult.IsSuccess)
                    {
                        matchedCount = Lottery539MatchCalculator.CalculateMatchedCount(
                            winningResult.Value.Numbers,
                            lineResult.Value.Numbers);
                    }
                }

                bool hasDraw = drawMap[row.TicketId].TrueForAll(item => item.DrawId != row.DrawId.Value);
                if (hasDraw)
                {
                    drawMap[row.TicketId].Add(new TicketDrawSummaryDto(
                        row.DrawId.Value,
                        row.DrawAt.Value,
                        row.ParticipationStatus.Value,
                        matchedCount));
                }
            }
        }

        List<TicketSummaryDto> result = new List<TicketSummaryDto>();
        foreach (KeyValuePair<Guid, TicketSummaryDto> entry in ticketMap)
        {
            IReadOnlyCollection<TicketLineSummaryDto> lines = lineMap[entry.Key];
            IReadOnlyCollection<TicketDrawSummaryDto> draws = drawMap[entry.Key];
            TicketSummaryDto ticket = entry.Value with { Lines = lines, Draws = draws };
            result.Add(ticket);
        }

        PagedResult<TicketSummaryDto> pagedResult = PagedResult<TicketSummaryDto>.Create(
            result,
            totalCount,
            request.PageNumber,
            pageSize);

        return pagedResult;
    }
}
