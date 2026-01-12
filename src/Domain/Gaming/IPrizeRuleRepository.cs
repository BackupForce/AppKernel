namespace Domain.Gaming;

public interface IPrizeRuleRepository
{
    Task<PrizeRule?> GetByIdAsync(Guid tenantId, Guid ruleId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<PrizeRule>> GetActiveRulesAsync(
        Guid tenantId,
        GameType gameType,
        DateTime utcNow,
        CancellationToken cancellationToken = default);

    Task<bool> HasActiveRuleAsync(
        Guid tenantId,
        GameType gameType,
        int matchCount,
        Guid? excludeRuleId,
        CancellationToken cancellationToken = default);

    void Insert(PrizeRule rule);

    void Update(PrizeRule rule);
}
