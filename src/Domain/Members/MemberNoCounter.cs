namespace Domain.Members;

public sealed class MemberNoCounter
{
    public Guid TenantId { get; set; }

    public int LastValue { get; set; }
}
