using SharedKernel;

namespace Domain.Members;

public sealed class MemberTag : Entity
{
    private MemberTag(
        Guid id,
        Guid tenantId,
        string tagCode,
        string displayName,
        bool isActive,
        DateTime createdAtUtc,
        DateTime updatedAtUtc) : base(id)
    {
        TenantId = tenantId;
        TagCode = tagCode;
        DisplayName = displayName;
        IsActive = isActive;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
    }

    private MemberTag()
    {
    }

    public Guid TenantId { get; private set; }

    public string TagCode { get; private set; } = string.Empty;

    public string DisplayName { get; private set; } = string.Empty;

    public bool IsActive { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public static MemberTag Create(Guid tenantId, string tagCode, string displayName, DateTime nowUtc)
    {
        return new MemberTag(
            Guid.NewGuid(),
            tenantId,
            tagCode,
            displayName,
            true,
            nowUtc,
            nowUtc);
    }


    public void UpdateDisplayName(string displayName, DateTime nowUtc)
    {
        DisplayName = displayName;
        UpdatedAtUtc = nowUtc;
    }

    public void Deactivate(DateTime nowUtc)
    {
        IsActive = false;
        UpdatedAtUtc = nowUtc;
    }

    public void Activate(DateTime nowUtc)
    {
        IsActive = true;
        UpdatedAtUtc = nowUtc;
    }
}

