namespace Domain.Gaming.Services;

/// <summary>
/// 539 基本玩法規則（固定 4 個獎級）。
/// </summary>
public sealed class Lottery539BasicPlayRule : IPlayRule
{
    private static readonly PrizeTier Tier1 = new("T1");
    private static readonly PrizeTier Tier2 = new("T2");
    private static readonly PrizeTier Tier3 = new("T3");
    private static readonly PrizeTier Tier4 = new("T4");
    private static readonly IReadOnlyList<PrizeTier> Tiers = new[] { Tier1, Tier2, Tier3, Tier4 };

    public GameCode GameCode => GameCodes.Lottery539;

    public PlayTypeCode PlayTypeCode => PlayTypeCodes.Basic;

    public IReadOnlyList<PrizeTier> GetTiers()
    {
        return Tiers;
    }

    /// <summary>
    /// 依命中顆數決定獎級，未達門檻則回傳 null。
    /// </summary>
    public PrizeTier? Evaluate(LotteryNumbers bet, LotteryNumbers result)
    {
        int matchedCount = Lottery539MatchCalculator.CalculateMatchedCount(result.Numbers, bet.Numbers);

        return matchedCount switch
        {
            5 => Tier1,
            4 => Tier2,
            3 => Tier3,
            2 => Tier4,
            _ => null
        };
    }
}
