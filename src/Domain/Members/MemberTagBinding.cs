using SharedKernel;

namespace Domain.Members;

public sealed class MemberTagBinding : Entity
{
    private MemberTagBinding(
        Guid id,
        Guid tenantId,
        Guid memberId,
        Guid tagId,
        DateTime createdAtUtc,
        Guid? createdByUserId) : base(id)
    {
        TenantId = tenantId;
        MemberId = memberId;
        TagId = tagId;
        CreatedAtUtc = createdAtUtc;
        CreatedByUserId = createdByUserId;
    }

    private MemberTagBinding()
    {
    }

    public Guid TenantId { get; private set; }

    public Guid MemberId { get; private set; }

    public Guid TagId { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public Guid? CreatedByUserId { get; private set; }

    public static MemberTagBinding Create(Guid tenantId, Guid memberId, Guid tagId, DateTime nowUtc, Guid? createdByUserId)
    {
        return new MemberTagBinding(Guid.NewGuid(), tenantId, memberId, tagId, nowUtc, createdByUserId);
    }
}
