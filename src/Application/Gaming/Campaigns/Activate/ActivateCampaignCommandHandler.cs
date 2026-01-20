using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Gaming.Campaigns;
using Domain.Gaming.Repositories;
using Domain.Gaming.Shared;
using SharedKernel;

namespace Application.Gaming.Campaigns.Activate;

internal sealed class ActivateCampaignCommandHandler(
    ICampaignRepository campaignRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ITenantContext tenantContext) : ICommandHandler<ActivateCampaignCommand>
{
    public async Task<Result> Handle(ActivateCampaignCommand request, CancellationToken cancellationToken)
    {
        if (request.TenantId != tenantContext.TenantId)
        {
            return Result.Failure(GamingErrors.CampaignTenantMismatch);
        }

        Campaign? campaign = await campaignRepository.GetByIdAsync(request.TenantId, request.CampaignId, cancellationToken);
        if (campaign is null)
        {
            return Result.Failure(GamingErrors.CampaignNotFound);
        }

        Result activateResult = campaign.Activate(dateTimeProvider.UtcNow);
        if (activateResult.IsFailure)
        {
            return activateResult;
        }

        campaignRepository.Update(campaign);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
