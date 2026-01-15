using Domain.Gaming.Catalog;
using SharedKernel;

namespace Domain.Gaming.Entitlements;

/// <summary>
/// 租戶玩法啟用設定，用於控制租戶可使用的玩法範圍。
/// </summary>
public sealed class TenantPlayEntitlement : Entity
{
    private TenantPlayEntitlement(
        Guid id,
        Guid tenantId,
        GameCode gameCode,
        PlayTypeCode playTypeCode,
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
        PlayTypeCode = playTypeCode;
        IsEnabled = isEnabled;
        EnabledAtUtc = enabledAtUtc;
        DisabledAtUtc = disabledAtUtc;
    }

    private TenantPlayEntitlement()
    {
    }

    public Guid TenantId { get; private set; }

    public GameCode GameCode { get; private set; }

    public PlayTypeCode PlayTypeCode { get; private set; }

    public bool IsEnabled { get; private set; }

    public DateTime EnabledAtUtc { get; private set; }

    public DateTime? DisabledAtUtc { get; private set; }

    public static TenantPlayEntitlement Create(Guid tenantId, GameCode gameCode, PlayTypeCode playTypeCode, DateTime utcNow)
    {
        return new TenantPlayEntitlement(Guid.NewGuid(), tenantId, gameCode, playTypeCode, true, utcNow, null);
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
