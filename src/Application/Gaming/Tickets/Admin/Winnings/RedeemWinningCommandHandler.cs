using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Application.Gaming.Dtos;
using Application.Abstractions.Gaming;
using Domain.Gaming.Shared;
using SharedKernel;

namespace Application.Gaming.Tickets.Admin.Winnings;

internal sealed class RedeemWinningCommandHandler(
    IAdminWinningRedemptionRepository redemptionRepository,
    ITenantContext tenantContext,
    IUserContext userContext,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<RedeemWinningCommand, AdminWinningDetailDto>
{
    public async Task<Result<AdminWinningDetailDto>> Handle(RedeemWinningCommand request, CancellationToken cancellationToken)
    {
        AdminWinningRedemptionResult redemptionResult = await redemptionRepository.RedeemAsync(
            tenantContext.TenantId,
            request.WinningId,
            userContext.UserId,
            dateTimeProvider.UtcNow,
            cancellationToken);

        if (redemptionResult == AdminWinningRedemptionResult.NotFound)
        {
            return Result.Failure<AdminWinningDetailDto>(GamingErrors.TicketLineResultNotFound);
        }

        if (redemptionResult == AdminWinningRedemptionResult.Conflict)
        {
            return Result.Failure<AdminWinningDetailDto>(GamingErrors.TicketLineResultAlreadyRedeemed);
        }

        Result<AdminWinningDetailDto> detailResult = await redemptionRepository.GetWinningDetailAsync(
            tenantContext.TenantId,
            request.WinningId,
            cancellationToken);

        return detailResult;
    }
}
