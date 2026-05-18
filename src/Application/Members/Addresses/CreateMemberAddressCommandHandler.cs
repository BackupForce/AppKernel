using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Members;
using SharedKernel;

namespace Application.Members.Addresses;

internal sealed class CreateMemberAddressCommandHandler(
    IMemberRepository memberRepository,
    IMemberAddressRepository memberAddressRepository,
    IUnitOfWork unitOfWork,
    ITenantContext tenantContext) : ICommandHandler<CreateMemberAddressCommand, Guid>
{
    public async Task<Result<Guid>> Handle(
        CreateMemberAddressCommand request,
        CancellationToken cancellationToken)
    {
        Member? member = await memberRepository.GetByIdAsync(tenantContext.TenantId, request.MemberId, cancellationToken);
        if (member is null)
        {
            return Result.Failure<Guid>(MemberErrors.MemberNotFound);
        }

        if (IsAddressFieldInvalid(request))
        {
            return Result.Failure<Guid>(MemberErrors.MemberAddressFieldRequired);
        }

        if (request.IsDefault)
        {
            List<MemberAddress> existingAddresses = await memberAddressRepository.GetByMemberIdAsync(
                request.MemberId,
                cancellationToken);
            foreach (MemberAddress address in existingAddresses)
            {
                if (address.IsDefault)
                {
                    address.SetDefault(false);
                    memberAddressRepository.Update(address);
                }
            }
        }

        MemberAddress newAddress = MemberAddress.Create(
            request.MemberId,
            request.ReceiverName,
            request.PhoneNumber,
            request.Country,
            request.City,
            request.District,
            request.AddressLine,
            request.IsDefault);

        memberAddressRepository.Insert(newAddress);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return newAddress.Id;
    }

    private static bool IsAddressFieldInvalid(CreateMemberAddressCommand request)
    {
        return string.IsNullOrWhiteSpace(request.ReceiverName)
            || string.IsNullOrWhiteSpace(request.PhoneNumber)
            || string.IsNullOrWhiteSpace(request.Country)
            || string.IsNullOrWhiteSpace(request.City)
            || string.IsNullOrWhiteSpace(request.District)
            || string.IsNullOrWhiteSpace(request.AddressLine);
    }
}
