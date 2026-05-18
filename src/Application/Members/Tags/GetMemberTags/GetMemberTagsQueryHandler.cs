using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Application.Members.Dtos;
using Domain.Gaming.Shared;
using Domain.Members;
using SharedKernel;

namespace Application.Members.Tags.GetMemberTags;

internal sealed class GetMemberTagsQueryHandler(
    IMemberRepository memberRepository,
    IMemberTagBindingRepository memberTagBindingRepository,
    IMemberTagCatalogRepository memberTagCatalogRepository,
    ITenantContext tenantContext) : IQueryHandler<GetMemberTagsQuery, IReadOnlyCollection<MemberTagDto>>
{
    public async Task<Result<IReadOnlyCollection<MemberTagDto>>> Handle(GetMemberTagsQuery request, CancellationToken cancellationToken)
    {
        if (request.TenantId != tenantContext.TenantId)
        {
            return Result.Failure<IReadOnlyCollection<MemberTagDto>>(GamingErrors.MemberTagTenantMismatch);
        }

        Member? member = await memberRepository.GetByIdAsync(request.TenantId, request.MemberId, cancellationToken);
        if (member is null)
        {
            return Result.Failure<IReadOnlyCollection<MemberTagDto>>(GamingErrors.MemberNotFound);
        }

        IReadOnlyCollection<Guid> tagIds = await memberTagBindingRepository.GetTagIdsByMemberIdAsync(
            request.TenantId,
            request.MemberId,
            cancellationToken);

        if (tagIds.Count == 0)
        {
            return Array.Empty<MemberTagDto>();
        }

        IReadOnlyCollection<MemberTag> tags = await memberTagCatalogRepository.GetByIdsAsync(
            request.TenantId,
            tagIds,
            cancellationToken);

        IReadOnlyCollection<MemberTagDto> result = tags
            .OrderBy(tag => tag.TagCode)
            .Select(tag => new MemberTagDto(tag.Id, tag.TagCode, tag.DisplayName, tag.IsActive, tag.CreatedAtUtc, tag.UpdatedAtUtc))
            .ToArray();

        return Result.Success(result);
    }
}
