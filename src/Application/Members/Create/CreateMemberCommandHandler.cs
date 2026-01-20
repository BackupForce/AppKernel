using System.Data;
using Application.Abstractions.Authentication;
using Application.Abstractions.Caching;
using Application.Abstractions.Data;
using Application.Abstractions.Identity;
using Application.Abstractions.Messaging;
using Domain.Members;
using Domain.Security;
using SharedKernel;

namespace Application.Members.Create;

internal sealed class CreateMemberCommandHandler(
    IMemberRepository memberRepository,
    IResourceNodeRepository resourceNodeRepository,
    ITenantContext tenantContext,
    IDateTimeProvider dateTimeProvider,
    IMemberNoGenerator memberNoGenerator,
    IUnitOfWork unitOfWork,
    ICacheService cacheService) : ICommandHandler<CreateMemberCommand, Guid>
{
    private const string MemberCacheKeyPrefix = "members:id:";
    private const string PointBalanceCacheKeyPrefix = "members:points:balance:";

    public async Task<Result<Guid>> Handle(CreateMemberCommand request, CancellationToken cancellationToken)
    {
        Guid tenantId = tenantContext.TenantId;

        if (request.UserId.HasValue
            && !await memberRepository.IsUserIdUniqueAsync(tenantId, request.UserId.Value, cancellationToken))
        {
            return Result.Failure<Guid>(MemberErrors.MemberUserNotUnique);
        }

        string memberNo = string.IsNullOrWhiteSpace(request.MemberNo)
            ? await memberNoGenerator.GenerateAsync(
                tenantId,
                MemberNoGenerationMode.Timestamp,
                cancellationToken)
            : request.MemberNo!;

        if (!await memberRepository.IsMemberNoUniqueAsync(tenantId, memberNo, cancellationToken))
        {
            return Result.Failure<Guid>(MemberErrors.MemberNoNotUnique);
        }

        DateTime utcNow = dateTimeProvider.UtcNow;

        Result<Member> memberResult = Member.Create(tenantId, request.UserId, memberNo, request.DisplayName, utcNow);
        if (memberResult.IsFailure)
        {
            return Result.Failure<Guid>(memberResult.Error);
        }

        Member member = memberResult.Value;
        var pointBalance = MemberPointBalance.Create(member.Id, utcNow);
        Guid? parentNodeId = await resourceNodeRepository.GetRootNodeIdAsync(tenantId, cancellationToken);
        var memberNode = ResourceNode.Create(
            member.DisplayName,
            ResourceNodeKeys.Member(member.Id),
            tenantId,
            parentNodeId);

        using IDbTransaction transaction = await unitOfWork.BeginTransactionAsync();

        memberRepository.Insert(member);
        memberRepository.InsertPointBalance(pointBalance);
        resourceNodeRepository.Insert(memberNode);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        transaction.Commit();

        await cacheService.RemoveAsync($"{MemberCacheKeyPrefix}{member.Id}", cancellationToken);
        await cacheService.RemoveAsync($"{PointBalanceCacheKeyPrefix}{member.Id}", cancellationToken);

        return member.Id;
    }

}
