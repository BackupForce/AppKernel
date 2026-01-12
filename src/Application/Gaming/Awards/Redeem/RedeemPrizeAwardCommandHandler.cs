using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Gaming;
using Domain.Members;
using SharedKernel;

namespace Application.Gaming.Awards.Redeem;

/// <summary>
/// 兌換得獎獎品，建立兌換紀錄並更新狀態。
/// </summary>
/// <remarks>
/// 兌換權限限定為本人，並以 AwardId 進行防重。
/// </remarks>
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

        // 授權限制：只能兌換自己的得獎記錄。
        if (award.MemberId != member.Id)
        {
            return Result.Failure<Guid>(GamingErrors.PrizeAwardNotOwned);
        }

        RedeemRecord? existing = await redeemRecordRepository.GetByAwardIdAsync(award.Id, cancellationToken);
        if (existing is not null)
        {
            // Idempotency：重複請求直接回傳既有兌換結果。
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

        // 成本快照寫入，避免未來獎品成本變動影響歷史報表。
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
