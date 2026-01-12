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
    IPrizeAwardOptionRepository prizeAwardOptionRepository,
    IRedeemRecordRepository redeemRecordRepository,
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

        DateTime now = dateTimeProvider.UtcNow;

        if (award.ExpiresAt.HasValue && now > award.ExpiresAt.Value)
        {
            if (award.Status != AwardStatus.Expired)
            {
                // 中文註解：兌獎過期時標記為 Expired，避免重送請求造成狀態混亂。
                award.Expire(now);
                prizeAwardRepository.Update(award);
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return Result.Failure<Guid>(GamingErrors.PrizeAwardExpired);
        }

        if (award.Status == AwardStatus.Expired)
        {
            return Result.Failure<Guid>(GamingErrors.PrizeAwardExpired);
        }

        if (award.Status != AwardStatus.Awarded)
        {
            return Result.Failure<Guid>(GamingErrors.PrizeAwardAlreadyRedeemed);
        }

        IReadOnlyCollection<PrizeAwardOption> options = await prizeAwardOptionRepository.GetByAwardIdAsync(
            tenantContext.TenantId,
            award.Id,
            cancellationToken);

        PrizeAwardOption? selectedOption = options.FirstOrDefault(option => option.PrizeId == request.PrizeId);
        if (selectedOption is null)
        {
            return Result.Failure<Guid>(GamingErrors.PrizeAwardOptionNotFound);
        }

        // 中文註解：兌獎使用快照資料，避免後台修改獎品資訊影響歷史紀錄。
        RedeemRecord record = RedeemRecord.Create(
            tenantContext.TenantId,
            member.Id,
            award.Id,
            selectedOption.PrizeId,
            selectedOption.PrizeNameSnapshot,
            selectedOption.PrizeCostSnapshot,
            now,
            request.Note);

        award.Redeem(now);

        redeemRecordRepository.Insert(record);
        prizeAwardRepository.Update(award);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return record.Id;
    }
}
