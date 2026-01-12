using SharedKernel;

namespace Domain.Gaming;

/// <summary>
/// 中獎規則，描述 MatchCount 對應的獎品與生效期間。
/// </summary>
/// <remarks>
/// 規則是否生效由 IsActive 與有效期間共同決定，避免結算引用不該生效的設定。
/// </remarks>
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

    /// <summary>
    /// 租戶識別。
    /// </summary>
    public Guid TenantId { get; private set; }

    /// <summary>
    /// 遊戲類型，確保不同玩法規則互不干擾。
    /// </summary>
    public GameType GameType { get; private set; }

    /// <summary>
    /// 命中數，範圍 0~5。
    /// </summary>
    public int MatchCount { get; private set; }

    /// <summary>
    /// 對應的獎品識別。
    /// </summary>
    public Guid PrizeId { get; private set; }

    /// <summary>
    /// 是否啟用。
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// 生效起始時間（UTC）。
    /// </summary>
    public DateTime? EffectiveFrom { get; private set; }

    /// <summary>
    /// 生效結束時間（UTC）。
    /// </summary>
    public DateTime? EffectiveTo { get; private set; }

    /// <summary>
    /// 建立時間（UTC）。
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// 更新時間（UTC）。
    /// </summary>
    public DateTime UpdatedAt { get; private set; }

    /// <summary>
    /// 建立規則，MatchCount 超出範圍時回傳錯誤。
    /// </summary>
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

    /// <summary>
    /// 更新規則內容與有效期間，需搭配規則衝突檢查由應用層負責。
    /// </summary>
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

    /// <summary>
    /// 啟用規則。
    /// </summary>
    public void Activate(DateTime utcNow)
    {
        IsActive = true;
        UpdatedAt = utcNow;
    }

    /// <summary>
    /// 停用規則。
    /// </summary>
    public void Deactivate(DateTime utcNow)
    {
        IsActive = false;
        UpdatedAt = utcNow;
    }

    /// <summary>
    /// 判斷規則在指定時間是否有效（啟用且介於有效期間內）。
    /// </summary>
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
