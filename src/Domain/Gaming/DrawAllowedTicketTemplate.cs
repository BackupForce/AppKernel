using SharedKernel;

namespace Domain.Gaming;

/// <summary>
/// 期數允許的票種模板設定。
/// </summary>
/// <remarks>
/// 用於限制特定期數可使用的票種，避免跨期濫用。
/// </remarks>
public sealed class DrawAllowedTicketTemplate : Entity
{
    private DrawAllowedTicketTemplate(
        Guid id,
        Guid tenantId,
        Guid drawId,
        Guid ticketTemplateId,
        DateTime createdAt) : base(id)
    {
        TenantId = tenantId;
        DrawId = drawId;
        TicketTemplateId = ticketTemplateId;
        CreatedAt = createdAt;
    }

    private DrawAllowedTicketTemplate()
    {
    }

    public Guid TenantId { get; private set; }

    public Guid DrawId { get; private set; }

    public Guid TicketTemplateId { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public static DrawAllowedTicketTemplate Create(
        Guid tenantId,
        Guid drawId,
        Guid ticketTemplateId,
        DateTime utcNow)
    {
        return new DrawAllowedTicketTemplate(Guid.NewGuid(), tenantId, drawId, ticketTemplateId, utcNow);
    }
}
