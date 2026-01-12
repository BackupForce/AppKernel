using Domain.Gaming;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class PrizeRuleRepository(ApplicationDbContext context) : IPrizeRuleRepository
{
    public async Task<PrizeRule?> GetByIdAsync(Guid tenantId, Guid ruleId, CancellationToken cancellationToken = default)
    {
        return await context.PrizeRules
            .FirstOrDefaultAsync(rule => rule.TenantId == tenantId && rule.Id == ruleId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<PrizeRule>> GetActiveRulesAsync(
        Guid tenantId,
        GameType gameType,
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        List<PrizeRule> rules = await context.PrizeRules
            .Where(rule => rule.TenantId == tenantId && rule.GameType == gameType)
            .ToListAsync(cancellationToken);

        return rules.Where(rule => rule.IsEffective(utcNow)).ToList();
    }

    public async Task<bool> HasActiveRuleAsync(
        Guid tenantId,
        GameType gameType,
        int matchCount,
        Guid? excludeRuleId,
        CancellationToken cancellationToken = default)
    {
        return await context.PrizeRules.AnyAsync(
            rule => rule.TenantId == tenantId
                    && rule.GameType == gameType
                    && rule.MatchCount == matchCount
                    && rule.IsActive
                    && (!excludeRuleId.HasValue || rule.Id != excludeRuleId.Value),
            cancellationToken);
    }

    public void Insert(PrizeRule rule)
    {
        context.PrizeRules.Add(rule);
    }

    public void Update(PrizeRule rule)
    {
        context.PrizeRules.Update(rule);
    }
}
