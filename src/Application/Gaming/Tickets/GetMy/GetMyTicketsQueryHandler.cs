using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Gaming.Dtos;
using Domain.Gaming;
using Domain.Gaming.Services;
using Domain.Members;
using Dapper;
using SharedKernel;

namespace Application.Gaming.Tickets.GetMy;

internal sealed class GetMyTicketsQueryHandler(
    IDbConnectionFactory dbConnectionFactory,
    IMemberRepository memberRepository,
    ITenantContext tenantContext,
    IUserContext userContext) : IQueryHandler<GetMyTicketsQuery, IReadOnlyCollection<TicketSummaryDto>>
{
    private sealed record TicketLineRow(
        Guid TicketId,
        Guid DrawId,
        long TotalCost,
        DateTime CreatedAt,
        int LineIndex,
        string Numbers,
        string? WinningNumbers);

    public async Task<Result<IReadOnlyCollection<TicketSummaryDto>>> Handle(
        GetMyTicketsQuery request,
        CancellationToken cancellationToken)
    {
        Member? member = await memberRepository.GetByUserIdAsync(tenantContext.TenantId, userContext.UserId, cancellationToken);
        if (member is null)
        {
            return Result.Failure<IReadOnlyCollection<TicketSummaryDto>>(GamingErrors.MemberNotFound);
        }

        const string sql = """
            SELECT
                t.id AS TicketId,
                t.draw_id AS DrawId,
                t.total_cost AS TotalCost,
                t.created_at AS CreatedAt,
                l.line_index AS LineIndex,
                l.numbers AS Numbers,
                d.winning_numbers AS WinningNumbers
            FROM gaming_tickets t
            INNER JOIN gaming_ticket_lines l ON l.ticket_id = t.id
            INNER JOIN gaming_draws d ON d.id = t.draw_id
            WHERE t.tenant_id = @TenantId
              AND t.member_id = @MemberId
              AND (@From IS NULL OR t.created_at >= @From)
              AND (@To IS NULL OR t.created_at <= @To)
            ORDER BY t.created_at DESC, l.line_index ASC
            """;

        using System.Data.IDbConnection connection = dbConnectionFactory.GetOpenConnection();

        IEnumerable<TicketLineRow> rows = await connection.QueryAsync<TicketLineRow>(
            sql,
            new
            {
                TenantId = tenantContext.TenantId,
                MemberId = member.Id,
                From = request.From,
                To = request.To
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
