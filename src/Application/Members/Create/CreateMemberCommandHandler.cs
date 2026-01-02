using System.Data;
using Application.Abstractions.Caching;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Members;
using SharedKernel;

namespace Application.Members.Create;

internal sealed class CreateMemberCommandHandler(
    IMemberRepository memberRepository,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork,
    ICacheService cacheService) : ICommandHandler<CreateMemberCommand, Guid>
{
    private const string MemberCacheKeyPrefix = "members:id:";
    private const string PointBalanceCacheKeyPrefix = "members:points:balance:";

    public async Task<Result<Guid>> Handle(CreateMemberCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId.HasValue && !await memberRepository.IsUserIdUniqueAsync(request.UserId.Value, cancellationToken))
        {
            return Result.Failure<Guid>(MemberErrors.MemberUserNotUnique);
        }

        string memberNo = string.IsNullOrWhiteSpace(request.MemberNo)
            ? await GenerateMemberNoAsync(cancellationToken)
            : request.MemberNo!;

        if (!await memberRepository.IsMemberNoUniqueAsync(memberNo, cancellationToken))
        {
            return Result.Failure<Guid>(MemberErrors.MemberNoNotUnique);
        }

        DateTime utcNow = dateTimeProvider.UtcNow;

        Result<Member> memberResult = Member.Create(request.UserId, memberNo, request.DisplayName, utcNow);
        if (memberResult.IsFailure)
        {
            return Result.Failure<Guid>(memberResult.Error);
        }

        Member member = memberResult.Value;
        var pointBalance = MemberPointBalance.Create(member.Id, utcNow);

        using IDbTransaction transaction = await unitOfWork.BeginTransactionAsync();

        memberRepository.Insert(member);
        memberRepository.UpsertPointBalance(pointBalance);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        transaction.Commit();

        await cacheService.RemoveAsync($"{MemberCacheKeyPrefix}{member.Id}", cancellationToken);
        await cacheService.RemoveAsync($"{PointBalanceCacheKeyPrefix}{member.Id}", cancellationToken);

        return member.Id;
    }

    private async Task<string> GenerateMemberNoAsync(CancellationToken cancellationToken)
    {
        // 中文註解：採用時間戳 + 簡短亂數，降低碰撞機率，仍以唯一性檢查保護。
        string memberNo;
        do
        {
            memberNo = $"MBR-{dateTimeProvider.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..6]}";
        }
        while (!await memberRepository.IsMemberNoUniqueAsync(memberNo, cancellationToken));

        return memberNo;
    }
}
