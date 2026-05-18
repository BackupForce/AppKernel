using Domain.Gaming.TicketClaimEvents;

namespace Domain.Gaming.Repositories;

public interface ITicketClaimEventTagRuleRepository
{
    Task<IReadOnlyCollection<Guid>> GetTagIdsByEventIdAsync(
        Guid tenantId,
        Guid eventId,
        CancellationToken cancellationToken = default);

    Task ReplaceTagRulesAsync(
        Guid tenantId,
        Guid eventId,
        IReadOnlyCollection<Guid> tagIds,
        Guid? operatorUserId,
        DateTime nowUtc,
        CancellationToken cancellationToken = default);
}
