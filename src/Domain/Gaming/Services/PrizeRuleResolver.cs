namespace Domain.Gaming.Services;

/// <summary>
/// 中獎規則解析器，用於從有效規則中找出對應的獎品。
/// </summary>
public static class PrizeRuleResolver
{
    /// <summary>
    /// 依命中數找出單一有效規則（MVP 假設一個命中數對應一個獎品）。
    /// </summary>
    public static PrizeRule? Resolve(IReadOnlyCollection<PrizeRule> rules, int matchedCount, DateTime utcNow)
    {
        return rules
            .Where(rule => rule.MatchCount == matchedCount)
            .FirstOrDefault(rule => rule.IsEffective(utcNow));
    }
}
