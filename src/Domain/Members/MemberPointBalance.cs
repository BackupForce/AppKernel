using SharedKernel;

namespace Domain.Members;

public sealed class MemberPointBalance : Entity
{
    private MemberPointBalance(Guid id, Guid memberId, long balance, DateTime updatedAt) : base(id)
    {
        MemberId = memberId;
        Balance = balance;
        UpdatedAt = updatedAt;
    }

    private MemberPointBalance()
    {
    }

    public Guid MemberId { get; private set; }

    public long Balance { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public static MemberPointBalance Create(Guid memberId, DateTime utcNow)
    {
        return new MemberPointBalance(Guid.NewGuid(), memberId, 0, utcNow);
    }

    public Result<long> Adjust(MemberPointLedgerType type, long amount, DateTime utcNow, bool allowNegative)
    {
        long signedAmount = type switch
        {
            MemberPointLedgerType.Spend or MemberPointLedgerType.AdjustSub => -amount,
            _ => amount
        };

        long before = Balance;
        long after = before + signedAmount;

        if (!allowNegative && after < 0)
        {
            return Result.Failure<long>(MemberErrors.NegativePointBalance);
        }

        Balance = after;
        UpdatedAt = utcNow;

        return after;
    }
}
