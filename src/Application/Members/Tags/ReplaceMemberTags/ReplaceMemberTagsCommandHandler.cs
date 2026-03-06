using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Time;
using Domain.Gaming.Shared;
using Domain.Members;
using SharedKernel;

namespace Application.Members.Tags.ReplaceMemberTags;

internal sealed class ReplaceMemberTagsCommandHandler(
    IMemberRepository memberRepository,
    IMemberTagCatalogRepository memberTagCatalogRepository,
    IMemberTagBindingRepository memberTagBindingRepository,
    IUserContext userContext,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ITenantContext tenantContext) : ICommandHandler<ReplaceMemberTagsCommand>
{
    public async Task<Result> Handle(ReplaceMemberTagsCommand request, CancellationToken cancellationToken)
    {
        if (request.TenantId != tenantContext.TenantId)
        {
            return Result.Failure(GamingErrors.MemberTagTenantMismatch);
        }

        Member? member = await memberRepository.GetByIdAsync(request.TenantId, request.MemberId, cancellationToken);
        if (member is null)
        {
            return Result.Failure(GamingErrors.MemberNotFound);
        }

        IReadOnlyCollection<Guid> tagIds = request.TagIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToArray();

        if (tagIds.Count > 0)
        {
            IReadOnlyCollection<MemberTag> tags = await memberTagCatalogRepository.GetByIdsAsync(
                request.TenantId,
                tagIds,
                cancellationToken);

            if (tags.Count != tagIds.Count)
            {
                return Result.Failure(GamingErrors.MemberTagNotFound);
            }

            if (tags.Any(tag => !tag.IsActive))
            {
                return Result.Failure(GamingErrors.MemberTagInactive);
            }
        }

        await memberTagBindingRepository.ReplaceMemberTagsAsync(
            request.TenantId,
            request.MemberId,
            tagIds,
            userContext.UserId,
            dateTimeProvider.UtcNow,
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
