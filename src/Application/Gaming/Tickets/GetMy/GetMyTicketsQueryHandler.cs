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

namespace Application.Gaming.Tickets.GetMy;

internal sealed class GetMyTicketsQueryHandler(
    IDbConnectionFactory dbConnectionFactory,
    IMemberRepository memberRepository,
    ITenantContext tenantContext,
    IUserContext userContext,
    IEntitlementChecker entitlementChecker) : IQueryHandler<GetMyTicketsQuery, IReadOnlyCollection<TicketSummaryDto>>
{
    private sealed record TicketLineRow(
        Guid TicketId,
        Guid DrawId,
        string GameCode,
        string PlayTypeCode,
        long TotalCost,
        DateTime CreatedAt,
        int LineIndex,
        string Numbers,
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
                t.draw_id AS DrawId,
                t.game_code AS GameCode,
                t.play_type_code AS PlayTypeCode,
                t.total_cost AS TotalCost,
                t.created_at AS CreatedAt,
                l.line_index AS LineIndex,
                l.numbers AS Numbers,
                d.winning_numbers AS WinningNumbers
            FROM gaming.tickets t
            INNER JOIN gaming.ticket_lines l ON l.ticket_id = t.id
            INNER JOIN gaming.draws d ON d.id = t.draw_id
            WHERE t.tenant_id = @TenantId
              AND t.member_id = @MemberId
              AND t.game_code = @GameCode
              AND (@From IS NULL OR t.created_at >= @From)
              AND (@To IS NULL OR t.created_at <= @To)
            ORDER BY t.created_at DESC, l.line_index ASC
            """;

        using System.Data.IDbConnection connection = dbConnectionFactory.GetOpenConnection();

        IEnumerable<TicketLineRow> rows = await connection.QueryAsync<TicketLineRow>(
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

        foreach (TicketLineRow row in rows)
        {
            if (!ticketMap.ContainsKey(row.TicketId))
            {
                ticketMap[row.TicketId] = new TicketSummaryDto(
                    row.TicketId,
                    row.DrawId,
                    row.GameCode,
                    row.PlayTypeCode,
                    row.TotalCost,
                    row.CreatedAt,
                    Array.Empty<TicketLineSummaryDto>());
                lineMap[row.TicketId] = new List<TicketLineSummaryDto>();
            }

            int matchedCount = 0;
            if (!string.IsNullOrWhiteSpace(row.WinningNumbers))
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

            lineMap[row.TicketId].Add(new TicketLineSummaryDto(row.LineIndex, row.Numbers, matchedCount));
        }

        List<TicketSummaryDto> result = new List<TicketSummaryDto>();
        foreach (KeyValuePair<Guid, TicketSummaryDto> entry in ticketMap)
        {
            IReadOnlyCollection<TicketLineSummaryDto> lines = lineMap[entry.Key];
            TicketSummaryDto ticket = entry.Value with { Lines = lines };
            result.Add(ticket);
        }

        return result;
    }
}
