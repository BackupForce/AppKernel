using SharedKernel;

namespace Domain.Gaming.TicketClaimEvents;

public sealed class TicketClaimEventTagRule : Entity
{
    private TicketClaimEventTagRule(
        Guid id,
        Guid tenantId,
        Guid eventId,
        Guid tagId,
        DateTime createdAtUtc,
        Guid? createdByUserId) : base(id)
    {
        TenantId = tenantId;
        EventId = eventId;
        TagId = tagId;
        CreatedAtUtc = createdAtUtc;
        CreatedByUserId = createdByUserId;
    }

    private TicketClaimEventTagRule()
    {
    }

    public Guid TenantId { get; private set; }

    public Guid EventId { get; private set; }

    public Guid TagId { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public Guid? CreatedByUserId { get; private set; }

    public static TicketClaimEventTagRule Create(Guid tenantId, Guid eventId, Guid tagId, DateTime nowUtc, Guid? createdByUserId)
    {
        return new TicketClaimEventTagRule(Guid.NewGuid(), tenantId, eventId, tagId, nowUtc, createdByUserId);
    }
}
