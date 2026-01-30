using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Gaming;
using Application.Abstractions.Messaging;
using Domain.Gaming.DrawGroups;
using Domain.Gaming.Catalog;
using Domain.Gaming.Repositories;
using Domain.Gaming.Shared;
using SharedKernel;

namespace Application.Gaming.DrawGroups.Create;

internal sealed class CreateDrawGroupCommandHandler(
    IDrawGroupRepository drawGroupRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ITenantContext tenantContext,
    IEntitlementChecker entitlementChecker) : ICommandHandler<CreateDrawGroupCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateDrawGroupCommand request, CancellationToken cancellationToken)
    {
        if (request.TenantId != tenantContext.TenantId)
        {
            return Result.Failure<Guid>(GamingErrors.DrawGroupTenantMismatch);
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
        Result<DrawGroup> drawGroupResult = DrawGroup.Create(
            request.TenantId,
            gameCodeResult.Value,
            playTypeResult.Value,
            request.Name,
            request.GrantOpenAtUtc,
            request.GrantCloseAtUtc,
            DrawGroupStatus.Draft,
            now);

        if (drawGroupResult.IsFailure)
        {
            return Result.Failure<Guid>(drawGroupResult.Error);
        }

        drawGroupRepository.Insert(drawGroupResult.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return drawGroupResult.Value.Id;
    }
}
