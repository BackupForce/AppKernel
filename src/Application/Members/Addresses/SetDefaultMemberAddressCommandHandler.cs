using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Members;
using SharedKernel;

namespace Application.Members.Addresses;

internal sealed class SetDefaultMemberAddressCommandHandler(
    IMemberRepository memberRepository,
    IMemberAddressRepository memberAddressRepository,
    IUnitOfWork unitOfWork,
    ITenantContext tenantContext) : ICommandHandler<SetDefaultMemberAddressCommand>
{
    public async Task<Result> Handle(SetDefaultMemberAddressCommand request, CancellationToken cancellationToken)
    {
        Member? member = await memberRepository.GetByIdAsync(tenantContext.TenantId, request.MemberId, cancellationToken);
        if (member is null)
        {
            return Result.Failure(MemberErrors.MemberNotFound);
        }

        MemberAddress? address = await memberAddressRepository.GetByIdAsync(
            request.MemberId,
            request.Id,
            cancellationToken);
        if (address is null)
        {
            return Result.Failure(MemberErrors.MemberAddressNotFound);
        }

        List<MemberAddress> existingAddresses = await memberAddressRepository.GetByMemberIdAsync(
            request.MemberId,
            cancellationToken);
        foreach (MemberAddress other in existingAddresses)
        {
            bool shouldBeDefault = other.Id == address.Id;
            if (other.IsDefault != shouldBeDefault)
            {
                other.SetDefault(shouldBeDefault);
                memberAddressRepository.Update(other);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
