using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Gaming;
using Domain.Members;
using SharedKernel;

namespace Application.Gaming.Awards.Redeem;

internal sealed class RedeemPrizeAwardCommandHandler(
    IPrizeAwardRepository prizeAwardRepository,
    IRedeemRecordRepository redeemRecordRepository,
    IPrizeRepository prizeRepository,
    IMemberRepository memberRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ITenantContext tenantContext,
    IUserContext userContext) : ICommandHandler<RedeemPrizeAwardCommand, Guid>
{
    public async Task<Result<Guid>> Handle(RedeemPrizeAwardCommand request, CancellationToken cancellationToken)
    {
        Member? member = await memberRepository.GetByUserIdAsync(tenantContext.TenantId, userContext.UserId, cancellationToken);
        if (member is null)
        {
            return Result.Failure<Guid>(GamingErrors.MemberNotFound);
        }

        PrizeAward? award = await prizeAwardRepository.GetByIdAsync(tenantContext.TenantId, request.AwardId, cancellationToken);
        if (award is null)
        {
            return Result.Failure<Guid>(GamingErrors.PrizeAwardNotFound);
        }

        if (award.MemberId != member.Id)
        {
            return Result.Failure<Guid>(GamingErrors.PrizeAwardNotOwned);
        }

        RedeemRecord? existing = await redeemRecordRepository.GetByAwardIdAsync(award.Id, cancellationToken);
        if (existing is not null)
        {
            return existing.Id;
        }

        if (award.Status != AwardStatus.Awarded)
        {
            return Result.Failure<Guid>(GamingErrors.PrizeAwardAlreadyRedeemed);
        }

        Prize? prize = await prizeRepository.GetByIdAsync(tenantContext.TenantId, award.PrizeId, cancellationToken);
        if (prize is null)
        {
            return Result.Failure<Guid>(GamingErrors.PrizeNotFound);
        }

        if (!prize.IsActive)
        {
            return Result.Failure<Guid>(GamingErrors.PrizeInactive);
        }

        DateTime now = dateTimeProvider.UtcNow;

        // 中文註解：兌換動作必須具備 idempotency，避免重複產生兌換紀錄。
        RedeemRecord record = RedeemRecord.Create(
            tenantContext.TenantId,
            member.Id,
            award.Id,
            prize.Id,
            prize.Cost,
            now,
            request.Note);

        award.Redeem(now);

        redeemRecordRepository.Insert(record);
        prizeAwardRepository.Update(award);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return record.Id;
    }
}
