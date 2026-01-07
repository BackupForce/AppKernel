using System.Data;
using Application.Abstractions.Authentication;
using Application.Abstractions.Caching;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Members;
using SharedKernel;

namespace Application.Members.Update;

internal sealed class UpdateMemberProfileCommandHandler(
    IMemberRepository memberRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ITenantContext tenantContext,
    IUserContext userContext,
    ICacheService cacheService) : ICommandHandler<UpdateMemberProfileCommand>
{
    private const string MemberCacheKeyPrefix = "members:id:";

    public async Task<Result> Handle(UpdateMemberProfileCommand request, CancellationToken cancellationToken)
    {
        Member? member = await memberRepository.GetByIdAsync(tenantContext.TenantId, request.MemberId, cancellationToken);
        if (member is null)
        {
            return Result.Failure(MemberErrors.MemberNotFound);
        }

        Result updateResult = member.UpdateProfile(request.DisplayName, dateTimeProvider.UtcNow);
        if (updateResult.IsFailure)
        {
            return updateResult;
        }

        var log = MemberActivityLog.Create(
            member.Id,
            "member.update_profile",
            null,
            null,
            userContext.UserId,
            $"{{\"displayName\":\"{request.DisplayName}\"}}",
            dateTimeProvider.UtcNow);

        using IDbTransaction transaction = await unitOfWork.BeginTransactionAsync();
        memberRepository.InsertActivity(log);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        transaction.Commit();

        await cacheService.RemoveAsync($"{MemberCacheKeyPrefix}{member.Id}", cancellationToken);

        return Result.Success();
    }
}
