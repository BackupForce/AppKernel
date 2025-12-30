using Application.Abstractions.Authentication;
using Application.Abstractions.Caching;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Members;
using SharedKernel;

namespace Application.Members.Assets.Adjust;

internal sealed class AdjustMemberAssetCommandHandler(
    IMemberRepository memberRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    IUserContext userContext,
    ICacheService cacheService) : ICommandHandler<AdjustMemberAssetCommand, decimal>
{
    private const string MemberCacheKeyPrefix = "members:id:";
    private const string AssetBalanceCacheKeyPrefix = "members:assets:balance:";

    public async Task<Result<decimal>> Handle(AdjustMemberAssetCommand request, CancellationToken cancellationToken)
    {
        Member? member = await memberRepository.GetByIdAsync(request.MemberId, cancellationToken);
        if (member is null)
        {
            return Result.Failure<decimal>(MemberErrors.MemberNotFound);
        }

        if (member.Status == MemberStatus.Suspended)
        {
            return Result.Failure<decimal>(MemberErrors.MemberSuspended);
        }

        if (string.IsNullOrWhiteSpace(request.AssetCode))
        {
            return Result.Failure<decimal>(MemberErrors.AssetCodeRequired);
        }

        MemberAssetBalance? balance = await memberRepository.GetAssetBalanceAsync(
                request.MemberId,
                request.AssetCode,
                cancellationToken)
            ?? MemberAssetBalance.Create(request.MemberId, request.AssetCode, dateTimeProvider.UtcNow).Value;

        MemberAssetLedgerType ledgerType = request.Delta >= 0
            ? MemberAssetLedgerType.AdjustAdd
            : MemberAssetLedgerType.AdjustSub;

        decimal amount = Math.Abs(request.Delta);

        decimal beforeBalance = balance.Balance;

        Result<decimal> adjustResult = balance.Adjust(ledgerType, amount, dateTimeProvider.UtcNow, request.AllowNegative);
        if (adjustResult.IsFailure)
        {
            return Result.Failure<decimal>(adjustResult.Error);
        }

        decimal afterBalance = adjustResult.Value;

        Result<MemberAssetLedger> ledgerResult = MemberAssetLedger.Create(
            request.MemberId,
            request.AssetCode,
            ledgerType,
            amount,
            beforeBalance,
            afterBalance,
            request.ReferenceType,
            request.ReferenceId,
            userContext.UserId,
            request.Remark,
            dateTimeProvider.UtcNow);

        if (ledgerResult.IsFailure)
        {
            return Result.Failure<decimal>(ledgerResult.Error);
        }

        MemberAssetLedger ledger = ledgerResult.Value;

        member.RegisterAssetAdjusted(
            request.AssetCode,
            ledgerType,
            amount,
            beforeBalance,
            afterBalance,
            userContext.UserId);

        MemberActivityLog activityLog = MemberActivityLog.Create(
            request.MemberId,
            "assets.adjust",
            null,
            null,
            userContext.UserId,
            $"{{\"assetCode\":\"{request.AssetCode}\",\"delta\":{request.Delta}}}",
            dateTimeProvider.UtcNow);

        using var transaction = await unitOfWork.BeginTransactionAsync();

        memberRepository.UpsertAssetBalance(balance);
        memberRepository.InsertAssetLedger(ledger);
        memberRepository.InsertActivity(activityLog);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        transaction.Commit();

        await cacheService.RemoveAsync($"{AssetBalanceCacheKeyPrefix}{request.MemberId}:{request.AssetCode}", cancellationToken);
        await cacheService.RemoveAsync($"{MemberCacheKeyPrefix}{request.MemberId}", cancellationToken);

        return afterBalance;
    }
}
