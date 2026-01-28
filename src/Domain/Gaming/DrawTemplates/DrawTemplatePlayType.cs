using Domain.Gaming.Catalog;
using SharedKernel;

namespace Domain.Gaming.DrawTemplates;

/// <summary>
/// 模板內啟用玩法的實體。
/// </summary>
public sealed class DrawTemplatePlayType : Entity
{
    private DrawTemplatePlayType(
        Guid id,
        Guid tenantId,
        Guid templateId,
        PlayTypeCode playTypeCode) : base(id)
    {
        TenantId = tenantId;
        TemplateId = templateId;
        PlayTypeCode = playTypeCode;
    }

    private DrawTemplatePlayType()
    {
    }

    public Guid TenantId { get; private set; }

    public Guid TemplateId { get; private set; }

    public PlayTypeCode PlayTypeCode { get; private set; }

    public static DrawTemplatePlayType Create(Guid tenantId, Guid templateId, PlayTypeCode playTypeCode)
    {
        return new DrawTemplatePlayType(Guid.NewGuid(), tenantId, templateId, playTypeCode);
    }
}
