using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Time;
using Domain.Gaming.Shared;
using Domain.Members;
using SharedKernel;

namespace Application.Members.Tags.Update;

internal sealed class UpdateMemberTagCommandHandler(
    IMemberTagCatalogRepository memberTagCatalogRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ITenantContext tenantContext) : ICommandHandler<UpdateMemberTagCommand>
{
    public async Task<Result> Handle(UpdateMemberTagCommand request, CancellationToken cancellationToken)
    {
        if (request.TenantId != tenantContext.TenantId)
        {
            return Result.Failure(GamingErrors.MemberTagTenantMismatch);
        }

        if (string.IsNullOrWhiteSpace(request.DisplayName) || request.DisplayName.Trim().Length > 128)
        {
            return Result.Failure(GamingErrors.MemberTagDisplayNameInvalid);
        }

        MemberTag? tag = await memberTagCatalogRepository.GetByIdAsync(request.TenantId, request.TagId, cancellationToken);
        if (tag is null)
        {
            return Result.Failure(GamingErrors.MemberTagNotFound);
        }

        tag.UpdateDisplayName(request.DisplayName.Trim(), dateTimeProvider.UtcNow);
        memberTagCatalogRepository.Update(tag);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
