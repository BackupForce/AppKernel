using SharedKernel;

namespace Domain.Gaming.DrawTemplates;

/// <summary>
/// 模板允許使用的票種模板。
/// </summary>
public sealed class DrawTemplateAllowedTicketTemplate : Entity
{
    private DrawTemplateAllowedTicketTemplate(
        Guid id,
        Guid tenantId,
        Guid templateId,
        Guid ticketTemplateId,
        DateTime createdAtUtc) : base(id)
    {
        TenantId = tenantId;
        TemplateId = templateId;
        TicketTemplateId = ticketTemplateId;
        CreatedAtUtc = createdAtUtc;
    }

    private DrawTemplateAllowedTicketTemplate()
    {
    }

    public Guid TenantId { get; private set; }

    public Guid TemplateId { get; private set; }

    public Guid TicketTemplateId { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public static DrawTemplateAllowedTicketTemplate Create(
        Guid tenantId,
        Guid templateId,
        Guid ticketTemplateId,
        DateTime utcNow)
    {
        return new DrawTemplateAllowedTicketTemplate(Guid.NewGuid(), tenantId, templateId, ticketTemplateId, utcNow);
    }
}
