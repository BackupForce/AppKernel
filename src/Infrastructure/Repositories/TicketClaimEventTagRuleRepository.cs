using Domain.Gaming.Repositories;
using Domain.Gaming.TicketClaimEvents;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class TicketClaimEventTagRuleRepository(ApplicationDbContext context) : ITicketClaimEventTagRuleRepository
{
    public async Task<IReadOnlyCollection<Guid>> GetTagIdsByEventIdAsync(
        Guid tenantId,
        Guid eventId,
        CancellationToken cancellationToken = default)
    {
        return await context.TicketClaimEventTagRules
            .Where(rule => rule.TenantId == tenantId && rule.EventId == eventId)
            .Select(rule => rule.TagId)
            .ToListAsync(cancellationToken);
    }

    public async Task ReplaceTagRulesAsync(
        Guid tenantId,
        Guid eventId,
        IReadOnlyCollection<Guid> tagIds,
        Guid? operatorUserId,
        DateTime nowUtc,
        CancellationToken cancellationToken = default)
    {
        List<TicketClaimEventTagRule> existing = await context.TicketClaimEventTagRules
            .Where(rule => rule.TenantId == tenantId && rule.EventId == eventId)
            .ToListAsync(cancellationToken);

        context.TicketClaimEventTagRules.RemoveRange(existing);

        foreach (Guid tagId in tagIds.Distinct())
        {
            context.TicketClaimEventTagRules.Add(TicketClaimEventTagRule.Create(tenantId, eventId, tagId, nowUtc, operatorUserId));
        }
    }
}
