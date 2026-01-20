using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Gaming;
using Application.Abstractions.Messaging;
using Domain.Gaming.Campaigns;
using Domain.Gaming.Catalog;
using Domain.Gaming.Repositories;
using Domain.Gaming.Shared;
using SharedKernel;

namespace Application.Gaming.Campaigns.Create;

internal sealed class CreateCampaignCommandHandler(
    ICampaignRepository campaignRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ITenantContext tenantContext,
    IEntitlementChecker entitlementChecker) : ICommandHandler<CreateCampaignCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateCampaignCommand request, CancellationToken cancellationToken)
    {
        if (request.TenantId != tenantContext.TenantId)
        {
            return Result.Failure<Guid>(GamingErrors.CampaignTenantMismatch);
        }

        Result<GameCode> gameCodeResult = GameCode.Create(request.GameCode);
        if (gameCodeResult.IsFailure)
        {
            return Result.Failure<Guid>(gameCodeResult.Error);
        }

        Result<PlayTypeCode> playTypeResult = PlayTypeCode.Create(request.PlayTypeCode);
        if (playTypeResult.IsFailure)
        {
            return Result.Failure<Guid>(playTypeResult.Error);
        }

        Result entitlementResult = await entitlementChecker.EnsureGameEnabledAsync(
            request.TenantId,
            gameCodeResult.Value,
            cancellationToken);
        if (entitlementResult.IsFailure)
        {
            return Result.Failure<Guid>(entitlementResult.Error);
        }

        Result playEntitlementResult = await entitlementChecker.EnsurePlayEnabledAsync(
            request.TenantId,
            gameCodeResult.Value,
            playTypeResult.Value,
            cancellationToken);
        if (playEntitlementResult.IsFailure)
        {
            return Result.Failure<Guid>(playEntitlementResult.Error);
        }

        DateTime now = dateTimeProvider.UtcNow;
        Result<Campaign> campaignResult = Campaign.Create(
            request.TenantId,
            gameCodeResult.Value,
            playTypeResult.Value,
            request.Name,
            request.GrantOpenAtUtc,
            request.GrantCloseAtUtc,
            CampaignStatus.Draft,
            now);

        if (campaignResult.IsFailure)
        {
            return Result.Failure<Guid>(campaignResult.Error);
        }

        campaignRepository.Insert(campaignResult.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return campaignResult.Value.Id;
    }
}
