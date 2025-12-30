using SharedKernel;

namespace Domain.Members;

public sealed class MemberPointLedger : Entity
{
    private MemberPointLedger(
        Guid id,
        Guid memberId,
        MemberPointLedgerType type,
        long amount,
        long beforeBalance,
        long afterBalance,
        string? referenceType,
        string? referenceId,
        Guid? operatorUserId,
        string? remark,
        DateTime createdAt) : base(id)
    {
        MemberId = memberId;
        Type = type;
        Amount = amount;
        BeforeBalance = beforeBalance;
        AfterBalance = afterBalance;
        ReferenceType = referenceType;
        ReferenceId = referenceId;
        OperatorUserId = operatorUserId;
        Remark = remark;
        CreatedAt = createdAt;
    }

    private MemberPointLedger()
    {
    }

    public Guid MemberId { get; private set; }

    public MemberPointLedgerType Type { get; private set; }

    public long Amount { get; private set; }

    public long BeforeBalance { get; private set; }

    public long AfterBalance { get; private set; }

    public string? ReferenceType { get; private set; }

    public string? ReferenceId { get; private set; }

    public Guid? OperatorUserId { get; private set; }

    public string? Remark { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public static MemberPointLedger Create(
        Guid memberId,
        MemberPointLedgerType type,
        long amount,
        long beforeBalance,
        long afterBalance,
        string? referenceType,
        string? referenceId,
        Guid? operatorUserId,
        string? remark,
        DateTime createdAt)
    {
        return new MemberPointLedger(
            Guid.NewGuid(),
            memberId,
            type,
            amount,
            beforeBalance,
            afterBalance,
            referenceType,
            referenceId,
            operatorUserId,
            remark,
            createdAt);
    }
}
