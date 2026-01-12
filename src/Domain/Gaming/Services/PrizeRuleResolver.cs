namespace Domain.Gaming.Services;

public static class PrizeRuleResolver
{
    // 中文註解：MVP 只取命中顆數對應的單一獎品規則。
    public static PrizeRule? Resolve(IReadOnlyCollection<PrizeRule> rules, int matchedCount, DateTime utcNow)
    {
        return rules
            .Where(rule => rule.MatchCount == matchedCount)
            .FirstOrDefault(rule => rule.IsEffective(utcNow));
    }
}
