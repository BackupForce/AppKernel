using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Gaming.Campaigns;
using Domain.Gaming.Repositories;
using Domain.Gaming.Shared;
using SharedKernel;

namespace Application.Gaming.Campaigns.Update;

internal sealed class UpdateCampaignCommandHandler(
    ICampaignRepository campaignRepository,
    IUnitOfWork unitOfWork,
    ITenantContext tenantContext) : ICommandHandler<UpdateCampaignCommand>
{
    public async Task<Result> Handle(UpdateCampaignCommand request, CancellationToken cancellationToken)
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

        if (campaign.Status != CampaignStatus.Draft)
        {
            return Result.Failure(GamingErrors.CampaignNotDraft);
        }

        Result updateResult = campaign.Update(request.Name, request.GrantOpenAtUtc, request.GrantCloseAtUtc);
        if (updateResult.IsFailure)
        {
            return updateResult;
        }

        campaignRepository.Update(campaign);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
