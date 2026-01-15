using Application.Abstractions.Gaming;
using Domain.Gaming.Shared;
using Domain.Members;
using SharedKernel;

namespace Infrastructure.Gaming;

/// <summary>
/// 點數帳本的基礎設施實作，將 Gaming 的扣點轉成會員帳本操作。
/// </summary>
internal sealed class WalletLedgerService(
    IMemberRepository memberRepository,
    IDateTimeProvider dateTimeProvider,
    Application.Abstractions.Authentication.IUserContext userContext) : IWalletLedgerService
{
    /// <summary>
    /// 扣點並寫入帳本流水，referenceId 由上層傳入以避免重複扣點。
    /// </summary>
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
            return Result.Failure<long>(GamingErrors.MemberNotFound);
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

    /// <summary>
    /// 取得會員餘額。
    /// </summary>
    public async Task<long?> GetBalanceAsync(Guid memberId, CancellationToken cancellationToken = default)
    {
        MemberPointBalance? balance = await memberRepository.GetPointBalanceAsync(memberId, cancellationToken);
        return balance?.Balance;
    }
}
