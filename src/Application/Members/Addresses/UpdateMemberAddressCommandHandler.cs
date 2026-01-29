using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Members;
using SharedKernel;

namespace Application.Members.Addresses;

internal sealed class UpdateMemberAddressCommandHandler(
    IMemberRepository memberRepository,
    IMemberAddressRepository memberAddressRepository,
    IUnitOfWork unitOfWork,
    ITenantContext tenantContext) : ICommandHandler<UpdateMemberAddressCommand>
{
    public async Task<Result> Handle(UpdateMemberAddressCommand request, CancellationToken cancellationToken)
    {
        Member? member = await memberRepository.GetByIdAsync(tenantContext.TenantId, request.MemberId, cancellationToken);
        if (member is null)
        {
            return Result.Failure(MemberErrors.MemberNotFound);
        }

        if (IsAddressFieldInvalid(request))
        {
            return Result.Failure(MemberErrors.MemberAddressFieldRequired);
        }

        MemberAddress? address = await memberAddressRepository.GetByIdAsync(
            request.MemberId,
            request.Id,
            cancellationToken);
        if (address is null)
        {
            return Result.Failure(MemberErrors.MemberAddressNotFound);
        }

        if (request.IsDefault)
        {
            List<MemberAddress> existingAddresses = await memberAddressRepository.GetByMemberIdAsync(
                request.MemberId,
                cancellationToken);
            foreach (MemberAddress other in existingAddresses)
            {
                if (other.Id != address.Id && other.IsDefault)
                {
                    other.SetDefault(false);
                    memberAddressRepository.Update(other);
                }
            }
        }

        address.Update(
            request.ReceiverName,
            request.PhoneNumber,
            request.Country,
            request.City,
            request.District,
            request.AddressLine,
            request.IsDefault);
        memberAddressRepository.Update(address);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static bool IsAddressFieldInvalid(UpdateMemberAddressCommand request)
    {
        return string.IsNullOrWhiteSpace(request.ReceiverName)
            || string.IsNullOrWhiteSpace(request.PhoneNumber)
            || string.IsNullOrWhiteSpace(request.Country)
            || string.IsNullOrWhiteSpace(request.City)
            || string.IsNullOrWhiteSpace(request.District)
            || string.IsNullOrWhiteSpace(request.AddressLine);
    }
}
