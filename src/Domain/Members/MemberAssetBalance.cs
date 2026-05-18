using SharedKernel;

namespace Domain.Members;

public sealed class MemberAssetBalance : Entity
{
    private MemberAssetBalance(
        Guid id,
        Guid memberId,
        string assetCode,
        decimal balance,
        DateTime updatedAt) : base(id)
    {
        MemberId = memberId;
        AssetCode = assetCode;
        Balance = balance;
        UpdatedAt = updatedAt;
    }

    private MemberAssetBalance()
    {
    }

    public Guid MemberId { get; private set; }

    public string AssetCode { get; private set; } = string.Empty;

    public decimal Balance { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public static Result<MemberAssetBalance> Create(Guid memberId, string assetCode, DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(assetCode))
        {
            return Result.Failure<MemberAssetBalance>(MemberErrors.AssetCodeRequired);
        }

        return Result.Success(new MemberAssetBalance(Guid.NewGuid(), memberId, assetCode, 0, utcNow));
    }

    public Result<decimal> Adjust(MemberAssetLedgerType type, decimal amount, DateTime utcNow, bool allowNegative)
    {
        decimal signedAmount = type switch
        {
            MemberAssetLedgerType.Debit or MemberAssetLedgerType.AdjustSub => -amount,
            _ => amount
        };

        decimal before = Balance;
        decimal after = before + signedAmount;

        if (!allowNegative && after < 0)
        {
            return Result.Failure<decimal>(MemberErrors.NegativeAssetBalance);
        }

        Balance = after;
        UpdatedAt = utcNow;

        return after;
    }
}
