using Application.Abstractions.Data;
using Application.Abstractions.Gaming;
using Application.Abstractions.Messaging;
using Domain.Gaming.Catalog;
using Domain.Gaming.Entitlements;
using Domain.Gaming.Repositories;
using Domain.Gaming.Rules;
using Domain.Gaming.Shared;
using SharedKernel;

namespace Application.Gaming.Entitlements;

internal sealed class DisableTenantPlayEntitlementCommandHandler(
    ITenantPlayEntitlementRepository playEntitlementRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    IEntitlementCacheInvalidator cacheInvalidator)
    : ICommandHandler<DisableTenantPlayEntitlementCommand>
{
    public async Task<Result> Handle(DisableTenantPlayEntitlementCommand request, CancellationToken cancellationToken)
    {
        Result<GameCode> gameCodeResult = GameCode.Create(request.GameCode);
        if (gameCodeResult.IsFailure)
        {
            return Result.Failure(gameCodeResult.Error);
        }

        Result<PlayTypeCode> playCodeResult = PlayTypeCode.Create(request.PlayTypeCode);
        if (playCodeResult.IsFailure)
        {
            return Result.Failure(playCodeResult.Error);
        }

        GameCode gameCode = gameCodeResult.Value;
        PlayTypeCode playTypeCode = playCodeResult.Value;

        if (!IsCatalogPlay(gameCode, playTypeCode))
        {
            return Result.Failure(GamingErrors.PlayTypeNotAllowed);
        }

        TenantPlayEntitlement? existing = await playEntitlementRepository.GetAsync(
            request.TenantId,
            gameCode,
            playTypeCode,
            cancellationToken);

        if (existing is null)
        {
            return Result.Success();
        }

        existing.Disable(dateTimeProvider.UtcNow);
        playEntitlementRepository.Update(existing);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await cacheInvalidator.InvalidateTenantAsync(request.TenantId, cancellationToken);

        return Result.Success();
    }

    private static bool IsCatalogPlay(GameCode gameCode, PlayTypeCode playTypeCode)
    {
        PlayRuleRegistry registry = PlayRuleRegistry.CreateDefault();
        return registry.GetAllowedPlayTypes(gameCode).Contains(playTypeCode);
    }
}
