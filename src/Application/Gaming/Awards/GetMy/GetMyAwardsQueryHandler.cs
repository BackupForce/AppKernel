using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Gaming;
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
    IUserContext userContext,
    IEntitlementChecker entitlementChecker) : IQueryHandler<GetMyAwardsQuery, IReadOnlyCollection<PrizeAwardDto>>
{
    public async Task<Result<IReadOnlyCollection<PrizeAwardDto>>> Handle(
        GetMyAwardsQuery request,
        CancellationToken cancellationToken)
    {
        Result<GameCode> gameCodeResult = GameCode.Create(request.GameCode);
        if (gameCodeResult.IsFailure)
        {
            return Result.Failure<IReadOnlyCollection<PrizeAwardDto>>(gameCodeResult.Error);
        }

        Result entitlementResult = await entitlementChecker.EnsureGameEnabledAsync(
            tenantContext.TenantId,
            gameCodeResult.Value,
            cancellationToken);
        if (entitlementResult.IsFailure)
        {
            return Result.Failure<IReadOnlyCollection<PrizeAwardDto>>(entitlementResult.Error);
        }

        Member? member = await memberRepository.GetByUserIdAsync(tenantContext.TenantId, userContext.UserId, cancellationToken);
        if (member is null)
        {
            return Result.Failure<IReadOnlyCollection<PrizeAwardDto>>(GamingErrors.MemberNotFound);
        }

        string? statusFilter = request.Status?.Trim();
        int? statusValue = statusFilter?.ToUpperInvariant() switch
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
                a.game_code AS GameCode,
                a.play_type_code AS PlayTypeCode,
                a.prize_tier AS PrizeTier,
                a.prize_id AS PrizeId,
                a.prize_name_snapshot AS PrizeName,
                a.prize_cost_snapshot AS PrizeCost,
                a.prize_redeem_valid_days_snapshot AS PrizeRedeemValidDays,
                a.prize_description_snapshot AS PrizeDescription,
                CASE
                    WHEN a.status = 0 THEN 'Awarded'
                    WHEN a.status = 1 THEN 'Redeemed'
                    WHEN a.status = 2 THEN 'Expired'
                    WHEN a.status = 3 THEN 'Cancelled'
                    ELSE 'Awarded'
                END AS Status,
                a.awarded_at AS AwardedAt,
                a.expires_at AS ExpiresAt,
                r.redeemed_at AS RedeemedAt
            FROM gaming.prize_awards a
            LEFT JOIN gaming.redeem_records r ON r.prize_award_id = a.id
            WHERE a.tenant_id = @TenantId
              AND a.member_id = @MemberId
              AND a.game_code = @GameCode
              AND (@Status IS NULL OR a.status = @Status)
            ORDER BY a.awarded_at DESC
            """;

        using System.Data.IDbConnection connection = dbConnectionFactory.GetOpenConnection();

        IEnumerable<PrizeAwardRow> rows = await connection.QueryAsync<PrizeAwardRow>(
            sql,
            new { tenantContext.TenantId, MemberId = member.Id, Status = statusValue, GameCode = gameCodeResult.Value.Value });

        return rows.Select(row => new PrizeAwardDto(
                row.AwardId,
                row.DrawId,
                row.TicketId,
                row.LineIndex,
                row.MatchedCount,
                row.GameCode,
                row.PlayTypeCode,
                row.PrizeTier,
                row.PrizeId,
                row.PrizeName,
                row.PrizeCost,
                row.PrizeRedeemValidDays,
                row.PrizeDescription,
                row.Status,
                row.AwardedAt,
                row.ExpiresAt,
                row.RedeemedAt))
            .ToList();
    }

    private sealed record PrizeAwardRow(
        Guid AwardId,
        Guid DrawId,
        Guid TicketId,
        int LineIndex,
        int MatchedCount,
        string GameCode,
        string PlayTypeCode,
        string PrizeTier,
        Guid PrizeId,
        string PrizeName,
        decimal PrizeCost,
        int? PrizeRedeemValidDays,
        string? PrizeDescription,
        string Status,
        DateTime AwardedAt,
        DateTime? ExpiresAt,
        DateTime? RedeemedAt);
}
