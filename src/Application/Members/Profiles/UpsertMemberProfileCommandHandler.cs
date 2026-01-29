using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Members;
using SharedKernel;

namespace Application.Members.Profiles;

internal sealed class UpsertMemberProfileCommandHandler(
    IMemberRepository memberRepository,
    IMemberProfileRepository memberProfileRepository,
    IUnitOfWork unitOfWork,
    ITenantContext tenantContext,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<UpsertMemberProfileCommand>
{
    public async Task<Result> Handle(UpsertMemberProfileCommand request, CancellationToken cancellationToken)
    {
        Member? member = await memberRepository.GetByIdAsync(tenantContext.TenantId, request.MemberId, cancellationToken);
        if (member is null)
        {
            return Result.Failure(MemberErrors.MemberNotFound);
        }

        if (!Enum.IsDefined(typeof(Gender), request.Gender))
        {
            return Result.Failure(MemberErrors.InvalidGender);
        }

        MemberProfile? profile = await memberProfileRepository.GetByMemberIdAsync(
            request.MemberId,
            cancellationToken);

        DateTime utcNow = dateTimeProvider.UtcNow;
        if (profile is null)
        {
            profile = MemberProfile.Create(
                request.MemberId,
                request.RealName,
                request.Gender,
                request.PhoneNumber,
                request.PhoneVerified,
                utcNow);
            memberProfileRepository.Insert(profile);
        }
        else
        {
            profile.Update(
                request.RealName,
                request.Gender,
                request.PhoneNumber,
                request.PhoneVerified,
                utcNow);
            memberProfileRepository.Update(profile);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
