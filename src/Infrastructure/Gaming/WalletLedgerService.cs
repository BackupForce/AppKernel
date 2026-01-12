using Application.Abstractions.Gaming;
using Domain.Members;
using SharedKernel;

namespace Infrastructure.Gaming;

internal sealed class WalletLedgerService(
    IMemberRepository memberRepository,
    IDateTimeProvider dateTimeProvider,
    Application.Abstractions.Authentication.IUserContext userContext) : IWalletLedgerService
{
    public async Task<Result<long>> DebitAsync(
        Guid tenantId,
        Guid memberId,
        long amount,
        string referenceType,
        string? referenceId,
        string remark,
        CancellationToken cancellationToken = default)
    {
        Member? member = await memberRepository.GetByIdAsync(tenantId, memberId, cancellationToken);
        if (member is null)
        {
            return Result.Failure<long>(Domain.Gaming.GamingErrors.MemberNotFound);
        }

        MemberPointBalance balance = await memberRepository.GetPointBalanceAsync(memberId, cancellationToken)
            ?? MemberPointBalance.Create(memberId, dateTimeProvider.UtcNow);

        long beforeBalance = balance.Balance;
        Result<long> adjustResult = balance.Adjust(MemberPointLedgerType.Spend, amount, dateTimeProvider.UtcNow, false);
        if (adjustResult.IsFailure)
        {
            return Result.Failure<long>(adjustResult.Error);
        }

        long afterBalance = adjustResult.Value;

        MemberPointLedger ledger = MemberPointLedger.Create(
            memberId,
            MemberPointLedgerType.Spend,
            amount,
            beforeBalance,
            afterBalance,
            referenceType,
            referenceId,
            userContext.UserId,
            remark,
            dateTimeProvider.UtcNow);

        MemberActivityLog activityLog = MemberActivityLog.Create(
            memberId,
            "gaming.ticket.debit",
            null,
            null,
            userContext.UserId,
            $"{{\"amount\":{amount},\"referenceId\":\"{referenceId}\"}}",
            dateTimeProvider.UtcNow);

        member.RegisterPointsAdjusted(MemberPointLedgerType.Spend, amount, beforeBalance, afterBalance, userContext.UserId);

        memberRepository.UpsertPointBalance(balance);
        memberRepository.InsertPointLedger(ledger);
        memberRepository.InsertActivity(activityLog);

        return afterBalance;
    }

    public async Task<long?> GetBalanceAsync(Guid memberId, CancellationToken cancellationToken = default)
    {
        MemberPointBalance? balance = await memberRepository.GetPointBalanceAsync(memberId, cancellationToken);
        return balance?.Balance;
    }
}
