using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Gaming;
using Application.Abstractions.Messaging;
using Application.Gaming.Dtos;
using Domain.Members;
using Dapper;
using SharedKernel;
using Domain.Gaming.Rules;
using Domain.Gaming.Catalog;
using Domain.Gaming.Shared;
using Domain.Gaming.Tickets;

namespace Application.Gaming.Tickets.GetMy;

internal sealed class GetMyTicketsQueryHandler(
    IDbConnectionFactory dbConnectionFactory,
    IMemberRepository memberRepository,
    ITenantContext tenantContext,
    IUserContext userContext,
    IEntitlementChecker entitlementChecker) : IQueryHandler<GetMyTicketsQuery, IReadOnlyCollection<TicketSummaryDto>>
{
    private sealed record TicketRow(
        Guid TicketId,
        Guid? CampaignId,
        string GameCode,
        string PlayTypeCode,
        TicketSubmissionStatus SubmissionStatus,
        DateTime IssuedAtUtc,
        DateTime? SubmittedAtUtc,
        int? LineIndex,
        string? Numbers,
        Guid? DrawId,
        TicketDrawParticipationStatus? ParticipationStatus,
        DateTime? DrawAt,
        string? WinningNumbers);

    public async Task<Result<IReadOnlyCollection<TicketSummaryDto>>> Handle(
        GetMyTicketsQuery request,
        CancellationToken cancellationToken)
    {
        Result<GameCode> gameCodeResult = GameCode.Create(request.GameCode);
        if (gameCodeResult.IsFailure)
        {
            return Result.Failure<IReadOnlyCollection<TicketSummaryDto>>(gameCodeResult.Error);
        }

        Result entitlementResult = await entitlementChecker.EnsureGameEnabledAsync(
            tenantContext.TenantId,
            gameCodeResult.Value,
            cancellationToken);
        if (entitlementResult.IsFailure)
        {
            return Result.Failure<IReadOnlyCollection<TicketSummaryDto>>(entitlementResult.Error);
        }

        Member? member = await memberRepository.GetByUserIdAsync(tenantContext.TenantId, userContext.UserId, cancellationToken);
        if (member is null)
        {
            return Result.Failure<IReadOnlyCollection<TicketSummaryDto>>(GamingErrors.MemberNotFound);
        }

        const string sql = """
            SELECT
                t.id AS TicketId,
                t.campaign_id AS CampaignId,
                t.game_code AS GameCode,
                t.play_type_code AS PlayTypeCode,
                t.submission_status AS SubmissionStatus,
                t.issued_at_utc AS IssuedAtUtc,
                t.submitted_at_utc AS SubmittedAtUtc,
                l.line_index AS LineIndex,
                l.numbers AS Numbers,
                td.draw_id AS DrawId,
                td.participation_status AS ParticipationStatus,
                d.draw_at AS DrawAt,
                d.winning_numbers AS WinningNumbers
            FROM gaming.tickets t
            LEFT JOIN gaming.ticket_lines l ON l.ticket_id = t.id
            LEFT JOIN gaming.ticket_draws td ON td.ticket_id = t.id
            LEFT JOIN gaming.draws d ON d.id = td.draw_id
            WHERE t.tenant_id = @TenantId
              AND t.member_id = @MemberId
              AND t.game_code = @GameCode
              AND (@From IS NULL OR t.issued_at_utc >= @From)
              AND (@To IS NULL OR t.issued_at_utc <= @To)
            ORDER BY t.issued_at_utc DESC
            """;

        using System.Data.IDbConnection connection = dbConnectionFactory.GetOpenConnection();

        IEnumerable<TicketRow> rows = await connection.QueryAsync<TicketRow>(
            sql,
            new
            {
                tenantContext.TenantId,
                MemberId = member.Id,
                GameCode = gameCodeResult.Value.Value,
                request.From,
                request.To
            });

        Dictionary<Guid, TicketSummaryDto> ticketMap = new Dictionary<Guid, TicketSummaryDto>();
        Dictionary<Guid, List<TicketLineSummaryDto>> lineMap = new Dictionary<Guid, List<TicketLineSummaryDto>>();
        Dictionary<Guid, List<TicketDrawSummaryDto>> drawMap = new Dictionary<Guid, List<TicketDrawSummaryDto>>();

        foreach (TicketRow row in rows)
        {
            if (!ticketMap.ContainsKey(row.TicketId))
            {
                ticketMap[row.TicketId] = new TicketSummaryDto(
                    row.TicketId,
                    row.CampaignId,
                    row.GameCode,
                    row.PlayTypeCode,
                    row.SubmissionStatus,
                    row.IssuedAtUtc,
                    row.SubmittedAtUtc,
                    Array.Empty<TicketLineSummaryDto>(),
                    Array.Empty<TicketDrawSummaryDto>());
                lineMap[row.TicketId] = new List<TicketLineSummaryDto>();
                drawMap[row.TicketId] = new List<TicketDrawSummaryDto>();
            }

            if (row.LineIndex.HasValue && !string.IsNullOrWhiteSpace(row.Numbers)
                && lineMap[row.TicketId].TrueForAll(item => item.LineIndex != row.LineIndex.Value))
            {
                lineMap[row.TicketId].Add(new TicketLineSummaryDto(row.LineIndex.Value, row.Numbers));
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

                if (drawMap[row.TicketId].TrueForAll(item => item.DrawId != row.DrawId.Value))
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

        return result;
    }
}
