using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Time;
using Domain.Gaming.Shared;
using Domain.Members;
using SharedKernel;

namespace Application.Members.Tags.Create;

internal sealed class CreateMemberTagCommandHandler(
    IMemberTagCatalogRepository memberTagCatalogRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ITenantContext tenantContext) : ICommandHandler<CreateMemberTagCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateMemberTagCommand request, CancellationToken cancellationToken)
    {
        if (request.TenantId != tenantContext.TenantId)
        {
            return Result.Failure<Guid>(GamingErrors.MemberTagTenantMismatch);
        }

        string normalizedCode = request.TagCode.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(normalizedCode) || normalizedCode.Length > 64)
        {
            return Result.Failure<Guid>(GamingErrors.MemberTagCodeInvalid);
        }

        if (string.IsNullOrWhiteSpace(request.DisplayName) || request.DisplayName.Trim().Length > 128)
        {
            return Result.Failure<Guid>(GamingErrors.MemberTagDisplayNameInvalid);
        }

        MemberTag? existing = await memberTagCatalogRepository.GetByCodeAsync(
            request.TenantId,
            normalizedCode,
            cancellationToken);

        if (existing is not null)
        {
            return Result.Failure<Guid>(GamingErrors.MemberTagCodeDuplicated);
        }

        MemberTag tag = MemberTag.Create(request.TenantId, normalizedCode, request.DisplayName.Trim(), dateTimeProvider.UtcNow);
        memberTagCatalogRepository.Insert(tag);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return tag.Id;
    }
}
