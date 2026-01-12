using SharedKernel;

namespace Domain.Gaming;

public sealed class PrizeAward : Entity
{
    private PrizeAward(
        Guid id,
        Guid tenantId,
        Guid memberId,
        Guid drawId,
        Guid ticketId,
        int lineIndex,
        int matchedCount,
        Guid prizeId,
        AwardStatus status,
        DateTime awardedAt) : base(id)
    {
        TenantId = tenantId;
        MemberId = memberId;
        DrawId = drawId;
        TicketId = ticketId;
        LineIndex = lineIndex;
        MatchedCount = matchedCount;
        PrizeId = prizeId;
        Status = status;
        AwardedAt = awardedAt;
    }

    private PrizeAward()
    {
    }

    public Guid TenantId { get; private set; }

    public Guid MemberId { get; private set; }

    public Guid DrawId { get; private set; }

    public Guid TicketId { get; private set; }

    public int LineIndex { get; private set; }

    public int MatchedCount { get; private set; }

    public Guid PrizeId { get; private set; }

    public AwardStatus Status { get; private set; }

    public DateTime AwardedAt { get; private set; }

    public static PrizeAward Create(
        Guid tenantId,
        Guid memberId,
        Guid drawId,
        Guid ticketId,
        int lineIndex,
        int matchedCount,
        Guid prizeId,
        DateTime awardedAt)
    {
        return new PrizeAward(
            Guid.NewGuid(),
            tenantId,
            memberId,
            drawId,
            ticketId,
            lineIndex,
            matchedCount,
            prizeId,
            AwardStatus.Awarded,
            awardedAt);
    }

    public void Redeem(DateTime utcNow)
    {
        Status = AwardStatus.Redeemed;
    }
}
