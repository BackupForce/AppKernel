using SharedKernel;

namespace Domain.Gaming;

/// <summary>
/// 期數獎項對應設定，描述 matchCount 對應可兌換的獎品。
/// </summary>
public sealed class DrawPrizeMapping : Entity
{
    private DrawPrizeMapping(
        Guid id,
        Guid tenantId,
        Guid drawId,
        int matchCount,
        Guid prizeId,
        DateTime createdAt) : base(id)
    {
        TenantId = tenantId;
        DrawId = drawId;
        MatchCount = matchCount;
        PrizeId = prizeId;
        CreatedAt = createdAt;
    }

    private DrawPrizeMapping()
    {
    }

    public Guid TenantId { get; private set; }

    public Guid DrawId { get; private set; }

    public int MatchCount { get; private set; }

    public Guid PrizeId { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public static Result<DrawPrizeMapping> Create(
        Guid tenantId,
        Guid drawId,
        int matchCount,
        Guid prizeId,
        DateTime utcNow)
    {
        if (matchCount < 0 || matchCount > 5)
        {
            return Result.Failure<DrawPrizeMapping>(GamingErrors.LotteryNumbersCountInvalid);
        }

        DrawPrizeMapping mapping = new DrawPrizeMapping(
            Guid.NewGuid(),
            tenantId,
            drawId,
            matchCount,
            prizeId,
            utcNow);

        return mapping;
    }
}
