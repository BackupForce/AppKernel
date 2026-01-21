using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Gaming.Campaigns;
using Domain.Gaming.Draws;
using Domain.Gaming.Repositories;
using Domain.Gaming.Shared;
using SharedKernel;

namespace Application.Gaming.Campaigns.Draws.Add;

internal sealed class AddCampaignDrawCommandHandler(
    ICampaignRepository campaignRepository,
    IDrawRepository drawRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ITenantContext tenantContext) : ICommandHandler<AddCampaignDrawCommand>
{
    public async Task<Result> Handle(AddCampaignDrawCommand request, CancellationToken cancellationToken)
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

        Draw? draw = await drawRepository.GetByIdAsync(request.TenantId, request.DrawId, cancellationToken);
        if (draw is null)
        {
            return Result.Failure(GamingErrors.DrawNotFound);
        }

        if (draw.GameCode != campaign.GameCode)
        {
            return Result.Failure(GamingErrors.CampaignDrawGameCodeMismatch);
        }

        Result addResult = campaign.AddDraw(request.DrawId, dateTimeProvider.UtcNow);
        if (addResult.IsFailure)
        {
            return addResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
