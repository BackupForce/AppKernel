using SharedKernel;

namespace Domain.Members;

public sealed class MemberAssetLedger : Entity
{
    private MemberAssetLedger(
        Guid id,
        Guid memberId,
        string assetCode,
        MemberAssetLedgerType type,
        decimal amount,
        decimal beforeBalance,
        decimal afterBalance,
        string? referenceType,
        string? referenceId,
        Guid? operatorUserId,
        string? remark,
        DateTime createdAt) : base(id)
    {
        MemberId = memberId;
        AssetCode = assetCode;
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

    private MemberAssetLedger()
    {
    }

    public Guid MemberId { get; private set; }

    public string AssetCode { get; private set; } = string.Empty;

    public MemberAssetLedgerType Type { get; private set; }

    public decimal Amount { get; private set; }

    public decimal BeforeBalance { get; private set; }

    public decimal AfterBalance { get; private set; }

    public string? ReferenceType { get; private set; }

    public string? ReferenceId { get; private set; }

    public Guid? OperatorUserId { get; private set; }

    public string? Remark { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public static Result<MemberAssetLedger> Create(
        Guid memberId,
        string assetCode,
        MemberAssetLedgerType type,
        decimal amount,
        decimal beforeBalance,
        decimal afterBalance,
        string? referenceType,
        string? referenceId,
        Guid? operatorUserId,
        string? remark,
        DateTime createdAt)
    {
        if (string.IsNullOrWhiteSpace(assetCode))
        {
            return Result.Failure<MemberAssetLedger>(MemberErrors.AssetCodeRequired);
        }

        return Result.Success(new MemberAssetLedger(
            Guid.NewGuid(),
            memberId,
            assetCode,
            type,
            amount,
            beforeBalance,
            afterBalance,
            referenceType,
            referenceId,
            operatorUserId,
            remark,
            createdAt));
    }
}
