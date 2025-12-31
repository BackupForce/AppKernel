using System.Data;
using Application.Abstractions.Authentication;
using Application.Abstractions.Caching;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Members;
using SharedKernel;

namespace Application.Members.Activate;

internal sealed class ActivateMemberCommandHandler(
    IMemberRepository memberRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    IUserContext userContext,
    ICacheService cacheService) : ICommandHandler<ActivateMemberCommand>
{
    private const string MemberCacheKeyPrefix = "members:id:";

    public async Task<Result> Handle(ActivateMemberCommand request, CancellationToken cancellationToken)
    {
        Member? member = await memberRepository.GetByIdAsync(request.MemberId, cancellationToken);
        if (member is null)
        {
            return Result.Failure(MemberErrors.MemberNotFound);
        }

        Result activateResult = member.Activate(dateTimeProvider.UtcNow);
        if (activateResult.IsFailure)
        {
            return activateResult;
        }

        MemberActivityLog log = MemberActivityLog.Create(
            member.Id,
            "member.activate",
            null,
            null,
            userContext.UserId,
            string.IsNullOrWhiteSpace(request.Reason) ? null : $"{{\"reason\":\"{request.Reason}\"}}",
            dateTimeProvider.UtcNow);

        using IDbTransaction transaction = await unitOfWork.BeginTransactionAsync();
        memberRepository.InsertActivity(log);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        transaction.Commit();

        await cacheService.RemoveAsync($"{MemberCacheKeyPrefix}{member.Id}", cancellationToken);

        return Result.Success();
    }
}
