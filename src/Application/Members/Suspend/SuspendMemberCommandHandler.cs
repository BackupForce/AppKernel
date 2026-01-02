using System.Data;
using Application.Abstractions.Authentication;
using Application.Abstractions.Caching;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Members;
using SharedKernel;

namespace Application.Members.Suspend;

internal sealed class SuspendMemberCommandHandler(
    IMemberRepository memberRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    IUserContext userContext,
    ICacheService cacheService) : ICommandHandler<SuspendMemberCommand>
{
    private const string MemberCacheKeyPrefix = "members:id:";

    public async Task<Result> Handle(SuspendMemberCommand request, CancellationToken cancellationToken)
    {
        Member? member = await memberRepository.GetByIdAsync(request.MemberId, cancellationToken);
        if (member is null)
        {
            return Result.Failure(MemberErrors.MemberNotFound);
        }

        Result suspendResult = member.Suspend(dateTimeProvider.UtcNow);
        if (suspendResult.IsFailure)
        {
            return suspendResult;
        }

        var log = MemberActivityLog.Create(
            member.Id,
            "member.suspend",
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
