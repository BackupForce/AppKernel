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

internal sealed class DisableTenantGameEntitlementCommandHandler(
    ITenantGameEntitlementRepository gameEntitlementRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    IEntitlementCacheInvalidator cacheInvalidator)
    : ICommandHandler<DisableTenantGameEntitlementCommand>
{
    public async Task<Result> Handle(DisableTenantGameEntitlementCommand request, CancellationToken cancellationToken)
    {
        Result<GameCode> gameCodeResult = GameCode.Create(request.GameCode);
        if (gameCodeResult.IsFailure)
        {
            return Result.Failure(gameCodeResult.Error);
        }

        GameCode gameCode = gameCodeResult.Value;
        if (!IsCatalogGame(gameCode))
        {
            return Result.Failure(GamingErrors.GameNotFound);
        }

        TenantGameEntitlement? existing = await gameEntitlementRepository.GetAsync(request.TenantId, gameCode, cancellationToken);
        if (existing is null)
        {
            return Result.Success();
        }

        existing.Disable(dateTimeProvider.UtcNow);
        gameEntitlementRepository.Update(existing);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await cacheInvalidator.InvalidateTenantAsync(request.TenantId, cancellationToken);

        return Result.Success();
    }

    private static bool IsCatalogGame(GameCode gameCode)
    {
        PlayRuleRegistry registry = PlayRuleRegistry.CreateDefault();
        return registry.GetAllowedPlayTypes(gameCode).Count > 0;
    }
}
