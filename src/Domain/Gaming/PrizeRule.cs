using SharedKernel;

namespace Domain.Gaming;

public sealed class PrizeRule : Entity
{
    private PrizeRule(
        Guid id,
        Guid tenantId,
        GameType gameType,
        int matchCount,
        Guid prizeId,
        bool isActive,
        DateTime? effectiveFrom,
        DateTime? effectiveTo,
        DateTime createdAt,
        DateTime updatedAt) : base(id)
    {
        TenantId = tenantId;
        GameType = gameType;
        MatchCount = matchCount;
        PrizeId = prizeId;
        IsActive = isActive;
        EffectiveFrom = effectiveFrom;
        EffectiveTo = effectiveTo;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    private PrizeRule()
    {
    }

    public Guid TenantId { get; private set; }

    public GameType GameType { get; private set; }

    public int MatchCount { get; private set; }

    public Guid PrizeId { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime? EffectiveFrom { get; private set; }

    public DateTime? EffectiveTo { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public static Result<PrizeRule> Create(
        Guid tenantId,
        GameType gameType,
        int matchCount,
        Guid prizeId,
        DateTime? effectiveFrom,
        DateTime? effectiveTo,
        DateTime utcNow)
    {
        if (matchCount < 0 || matchCount > 5)
        {
            return Result.Failure<PrizeRule>(GamingErrors.LotteryNumbersCountInvalid);
        }

        PrizeRule rule = new PrizeRule(
            Guid.NewGuid(),
            tenantId,
            gameType,
            matchCount,
            prizeId,
            true,
            effectiveFrom,
            effectiveTo,
            utcNow,
            utcNow);

        return rule;
    }

    public void Update(
        int matchCount,
        Guid prizeId,
        DateTime? effectiveFrom,
        DateTime? effectiveTo,
        DateTime utcNow)
    {
        MatchCount = matchCount;
        PrizeId = prizeId;
        EffectiveFrom = effectiveFrom;
        EffectiveTo = effectiveTo;
        UpdatedAt = utcNow;
    }

    public void Activate(DateTime utcNow)
    {
        IsActive = true;
        UpdatedAt = utcNow;
    }

    public void Deactivate(DateTime utcNow)
    {
        IsActive = false;
        UpdatedAt = utcNow;
    }

    public bool IsEffective(DateTime utcNow)
    {
        if (!IsActive)
        {
            return false;
        }

        if (EffectiveFrom.HasValue && utcNow < EffectiveFrom.Value)
        {
            return false;
        }

        if (EffectiveTo.HasValue && utcNow > EffectiveTo.Value)
        {
            return false;
        }

        return true;
    }
}
