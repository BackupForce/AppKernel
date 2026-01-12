using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Gaming.Dtos;
using Domain.Gaming;
using Domain.Members;
using Dapper;
using SharedKernel;

namespace Application.Gaming.Awards.GetMy;

internal sealed class GetMyAwardsQueryHandler(
    IDbConnectionFactory dbConnectionFactory,
    IMemberRepository memberRepository,
    ITenantContext tenantContext,
    IUserContext userContext) : IQueryHandler<GetMyAwardsQuery, IReadOnlyCollection<PrizeAwardDto>>
{
    public async Task<Result<IReadOnlyCollection<PrizeAwardDto>>> Handle(
        GetMyAwardsQuery request,
        CancellationToken cancellationToken)
    {
        Member? member = await memberRepository.GetByUserIdAsync(tenantContext.TenantId, userContext.UserId, cancellationToken);
        if (member is null)
        {
            return Result.Failure<IReadOnlyCollection<PrizeAwardDto>>(GamingErrors.MemberNotFound);
        }

        string? statusFilter = request.Status?.Trim();
        int? statusValue = statusFilter?.ToLowerInvariant() switch
        {
            "awarded" => (int)AwardStatus.Awarded,
            "redeemed" => (int)AwardStatus.Redeemed,
            _ => null
        };

        const string sql = """
            SELECT
                a.id AS AwardId,
                a.draw_id AS DrawId,
                a.ticket_id AS TicketId,
                a.line_index AS LineIndex,
                a.matched_count AS MatchedCount,
                a.prize_id AS PrizeId,
                p.name AS PrizeName,
                CASE
                    WHEN a.status = 0 THEN 'Awarded'
                    WHEN a.status = 1 THEN 'Redeemed'
                    WHEN a.status = 2 THEN 'Expired'
                    WHEN a.status = 3 THEN 'Cancelled'
                    ELSE 'Awarded'
                END AS Status,
                a.awarded_at AS AwardedAt,
                a.expires_at AS ExpiresAt,
                r.redeemed_at AS RedeemedAt,
                r.cost_snapshot AS CostSnapshot,
                o.prize_id AS OptionPrizeId,
                o.prize_name_snapshot AS OptionPrizeName,
                o.prize_cost_snapshot AS OptionPrizeCost
            FROM gaming_prize_awards a
            INNER JOIN gaming_prizes p ON p.id = a.prize_id
            LEFT JOIN gaming_redeem_records r ON r.prize_award_id = a.id
            LEFT JOIN gaming_prize_award_options o ON o.prize_award_id = a.id
            WHERE a.tenant_id = @TenantId
              AND a.member_id = @MemberId
              AND (@Status IS NULL OR a.status = @Status)
            ORDER BY a.awarded_at DESC
            """;

        using System.Data.IDbConnection connection = dbConnectionFactory.GetOpenConnection();

        IEnumerable<PrizeAwardRow> rows = await connection.QueryAsync<PrizeAwardRow>(
            sql,
            new { TenantId = tenantContext.TenantId, MemberId = member.Id, Status = statusValue });

        Dictionary<Guid, PrizeAwardDto> awardMap = new Dictionary<Guid, PrizeAwardDto>();
        Dictionary<Guid, List<PrizeAwardOptionDto>> optionMap = new Dictionary<Guid, List<PrizeAwardOptionDto>>();

        foreach (PrizeAwardRow row in rows)
        {
            if (!awardMap.ContainsKey(row.AwardId))
            {
                awardMap[row.AwardId] = new PrizeAwardDto(
                    row.AwardId,
                    row.DrawId,
                    row.TicketId,
                    row.LineIndex,
                    row.MatchedCount,
                    row.PrizeId,
                    row.PrizeName,
                    row.Status,
                    row.AwardedAt,
                    row.ExpiresAt,
                    row.RedeemedAt,
                    row.CostSnapshot,
                    Array.Empty<PrizeAwardOptionDto>());
                optionMap[row.AwardId] = new List<PrizeAwardOptionDto>();
            }

            if (row.OptionPrizeId.HasValue && row.OptionPrizeName is not null && row.OptionPrizeCost.HasValue)
            {
                optionMap[row.AwardId].Add(new PrizeAwardOptionDto(
                    row.OptionPrizeId.Value,
                    row.OptionPrizeName,
                    row.OptionPrizeCost.Value));
            }
        }

        List<PrizeAwardDto> result = new List<PrizeAwardDto>();
        foreach (KeyValuePair<Guid, PrizeAwardDto> entry in awardMap)
        {
            IReadOnlyCollection<PrizeAwardOptionDto> options = optionMap[entry.Key];
            PrizeAwardDto award = entry.Value with { Options = options };
            result.Add(award);
        }

        return result;
    }

    private sealed record PrizeAwardRow(
        Guid AwardId,
        Guid DrawId,
        Guid TicketId,
        int LineIndex,
        int MatchedCount,
        Guid PrizeId,
        string PrizeName,
        string Status,
        DateTime AwardedAt,
        DateTime? ExpiresAt,
        DateTime? RedeemedAt,
        decimal? CostSnapshot,
        Guid? OptionPrizeId,
        string? OptionPrizeName,
        decimal? OptionPrizeCost);
}
