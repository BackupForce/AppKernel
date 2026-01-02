using System.Data;
using Application.Abstractions.Authentication;
using Application.Abstractions.Caching;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Members;
using SharedKernel;

namespace Application.Members.Points.Adjust;

internal sealed class AdjustMemberPointsCommandHandler(
    IMemberRepository memberRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    IUserContext userContext,
    ICacheService cacheService) : ICommandHandler<AdjustMemberPointsCommand, long>
{
    private const string MemberCacheKeyPrefix = "members:id:";
    private const string PointBalanceCacheKeyPrefix = "members:points:balance:";

    public async Task<Result<long>> Handle(AdjustMemberPointsCommand request, CancellationToken cancellationToken)
    {
        Member? member = await memberRepository.GetByIdAsync(request.MemberId, cancellationToken);
        if (member is null)
        {
            return Result.Failure<long>(MemberErrors.MemberNotFound);
        }

        if (member.Status == MemberStatus.Suspended)
        {
            return Result.Failure<long>(MemberErrors.MemberSuspended);
        }

        MemberPointBalance? balance = await memberRepository.GetPointBalanceAsync(request.MemberId, cancellationToken)
            ?? MemberPointBalance.Create(request.MemberId, dateTimeProvider.UtcNow);

        MemberPointLedgerType ledgerType = request.Delta >= 0
            ? MemberPointLedgerType.AdjustAdd
            : MemberPointLedgerType.AdjustSub;

        long amount = Math.Abs(request.Delta);

        long beforeBalance = balance.Balance;

        Result<long> adjustResult = balance.Adjust(ledgerType, amount, dateTimeProvider.UtcNow, request.AllowNegative);
        if (adjustResult.IsFailure)
        {
            return Result.Failure<long>(adjustResult.Error);
        }

        long afterBalance = adjustResult.Value;

        var ledger = MemberPointLedger.Create(
            request.MemberId,
            ledgerType,
            amount,
            beforeBalance,
            afterBalance,
            request.ReferenceType,
            request.ReferenceId,
            userContext.UserId,
            request.Remark,
            dateTimeProvider.UtcNow);

        member.RegisterPointsAdjusted(ledgerType, amount, beforeBalance, afterBalance, userContext.UserId);

        var activityLog = MemberActivityLog.Create(
            request.MemberId,
            "points.adjust",
            null,
            null,
            userContext.UserId,
            $"{{\"delta\":{request.Delta},\"remark\":\"{request.Remark}\"}}",
            dateTimeProvider.UtcNow);

        using IDbTransaction transaction = await unitOfWork.BeginTransactionAsync();

        memberRepository.UpsertPointBalance(balance);
        memberRepository.InsertPointLedger(ledger);
        memberRepository.InsertActivity(activityLog);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        transaction.Commit();

        await cacheService.RemoveAsync($"{PointBalanceCacheKeyPrefix}{request.MemberId}", cancellationToken);
        await cacheService.RemoveAsync($"{MemberCacheKeyPrefix}{request.MemberId}", cancellationToken);

        return afterBalance;
    }
}
