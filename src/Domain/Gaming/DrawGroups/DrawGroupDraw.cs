using SharedKernel;

namespace Domain.Gaming.DrawGroups;

public sealed class DrawGroupDraw : Entity
{
    private DrawGroupDraw(Guid tenantId, Guid drawGroupId, Guid drawId, DateTime createdAtUtc)
    {
        TenantId = tenantId;
        DrawGroupId = drawGroupId;
        DrawId = drawId;
        CreatedAtUtc = createdAtUtc;
    }

    private DrawGroupDraw()
    {
    }

    public Guid TenantId { get; private set; }

    public Guid DrawGroupId { get; private set; }

    public Guid DrawId { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public static DrawGroupDraw Create(Guid tenantId, Guid drawGroupId, Guid drawId, DateTime utcNow)
    {
        return new DrawGroupDraw(tenantId, drawGroupId, drawId, utcNow);
    }
}
