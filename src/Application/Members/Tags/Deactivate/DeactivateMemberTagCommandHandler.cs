using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Time;
using Domain.Gaming.Shared;
using Domain.Members;
using SharedKernel;

namespace Application.Members.Tags.Deactivate;

internal sealed class DeactivateMemberTagCommandHandler(
    IMemberTagCatalogRepository memberTagCatalogRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ITenantContext tenantContext) : ICommandHandler<DeactivateMemberTagCommand>
{
    public async Task<Result> Handle(DeactivateMemberTagCommand request, CancellationToken cancellationToken)
    {
        if (request.TenantId != tenantContext.TenantId)
        {
            return Result.Failure(GamingErrors.MemberTagTenantMismatch);
        }

        MemberTag? tag = await memberTagCatalogRepository.GetByIdAsync(request.TenantId, request.TagId, cancellationToken);
        if (tag is null)
        {
            return Result.Failure(GamingErrors.MemberTagNotFound);
        }

        tag.Deactivate(dateTimeProvider.UtcNow);
        memberTagCatalogRepository.Update(tag);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
