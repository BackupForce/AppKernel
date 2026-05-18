using Domain.Gaming.Catalog;
using SharedKernel;

namespace Domain.Gaming.Draws;

/// <summary>
/// Draw 內啟用玩法的實體，支援持久化與稽核。
/// </summary>
public sealed class DrawEnabledPlayType : Entity
{
    private DrawEnabledPlayType(Guid id, Guid tenantId, Guid drawId, PlayTypeCode playTypeCode) : base(id)
    {
        TenantId = tenantId;
        DrawId = drawId;
        PlayTypeCode = playTypeCode;
    }

    private DrawEnabledPlayType()
    {
    }

    public Guid TenantId { get; private set; }

    public Guid DrawId { get; private set; }

    public PlayTypeCode PlayTypeCode { get; private set; }

    public static DrawEnabledPlayType Create(Guid tenantId, Guid drawId, PlayTypeCode playTypeCode)
    {
        return new DrawEnabledPlayType(Guid.NewGuid(), tenantId, drawId, playTypeCode);
    }
}
