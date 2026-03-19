using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Time;
using Domain.Gaming.Shared;
using Domain.Members;
using SharedKernel;

namespace Application.Members.Tags.ActivateMemberTag;

internal sealed class ActivateMemberTagCommandHandler(
    IMemberTagCatalogRepository memberTagCatalogRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ITenantContext tenantContext) : ICommandHandler<ActivateMemberTagCommand>
{
    public async Task<Result> Handle(ActivateMemberTagCommand request, CancellationToken cancellationToken)
    {
        if (request.TenantId != tenantContext.TenantId)
        {
            return Result.Failure(GamingErrors.MemberTagTenantMismatch);
        }

        MemberTag? tag = await memberTagCatalogRepository.GetByIdAsync(request.TenantId, request.MemberTagId, cancellationToken);
        if (tag is null)
        {
            return Result.Failure(GamingErrors.MemberTagNotFound);
        }

        if (tag.IsActive)
        {
            return Result.Success();
        }

        tag.Activate(dateTimeProvider.UtcNow);
        memberTagCatalogRepository.Update(tag);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
