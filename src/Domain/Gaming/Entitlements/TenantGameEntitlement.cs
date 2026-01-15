using Domain.Gaming.Catalog;
using SharedKernel;

namespace Domain.Gaming.Entitlements;

/// <summary>
/// 租戶遊戲啟用設定，用於控制租戶可使用的遊戲範圍。
/// </summary>
public sealed class TenantGameEntitlement : Entity
{
    private TenantGameEntitlement(
        Guid id,
        Guid tenantId,
        GameCode gameCode,
        bool isEnabled,
        DateTime enabledAtUtc,
        DateTime? disabledAtUtc)
        : base(id)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        }

        TenantId = tenantId;
        GameCode = gameCode;
        IsEnabled = isEnabled;
        EnabledAtUtc = enabledAtUtc;
        DisabledAtUtc = disabledAtUtc;
    }

    private TenantGameEntitlement()
    {
    }

    public Guid TenantId { get; private set; }

    public GameCode GameCode { get; private set; }

    public bool IsEnabled { get; private set; }

    public DateTime EnabledAtUtc { get; private set; }

    public DateTime? DisabledAtUtc { get; private set; }

    public static TenantGameEntitlement Create(Guid tenantId, GameCode gameCode, DateTime utcNow)
    {
        return new TenantGameEntitlement(Guid.NewGuid(), tenantId, gameCode, true, utcNow, null);
    }

    public void Enable(DateTime utcNow)
    {
        if (IsEnabled)
        {
            return;
        }

        IsEnabled = true;
        EnabledAtUtc = utcNow;
        DisabledAtUtc = null;
    }

    public void Disable(DateTime utcNow)
    {
        if (!IsEnabled)
        {
            return;
        }

        IsEnabled = false;
        DisabledAtUtc = utcNow;
    }
}
