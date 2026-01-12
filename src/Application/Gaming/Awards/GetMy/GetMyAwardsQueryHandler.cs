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
                r.redeemed_at AS RedeemedAt,
                r.cost_snapshot AS CostSnapshot
            FROM gaming_prize_awards a
            INNER JOIN gaming_prizes p ON p.id = a.prize_id
            LEFT JOIN gaming_redeem_records r ON r.prize_award_id = a.id
            WHERE a.tenant_id = @TenantId
              AND a.member_id = @MemberId
              AND (@Status IS NULL OR a.status = @Status)
            ORDER BY a.awarded_at DESC
            """;

        using System.Data.IDbConnection connection = dbConnectionFactory.GetOpenConnection();

        IEnumerable<PrizeAwardDto> items = await connection.QueryAsync<PrizeAwardDto>(
            sql,
            new { TenantId = tenantContext.TenantId, MemberId = member.Id, Status = statusValue });

        return items.ToList();
    }
}
