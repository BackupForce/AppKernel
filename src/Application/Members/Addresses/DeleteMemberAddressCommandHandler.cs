using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Members;
using SharedKernel;

namespace Application.Members.Addresses;

internal sealed class DeleteMemberAddressCommandHandler(
    IMemberRepository memberRepository,
    IMemberAddressRepository memberAddressRepository,
    IUnitOfWork unitOfWork,
    ITenantContext tenantContext) : ICommandHandler<DeleteMemberAddressCommand>
{
    public async Task<Result> Handle(DeleteMemberAddressCommand request, CancellationToken cancellationToken)
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

        memberAddressRepository.Remove(address);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
