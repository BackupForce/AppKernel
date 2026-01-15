using Application.Abstractions.Data;
using Application.Abstractions.Gaming;
using Application.Abstractions.Messaging;
using Domain.Gaming.Catalog;
using Domain.Gaming.Entitlements;
using Domain.Gaming.Repositories;
using Domain.Gaming.Rules;
using Domain.Gaming.Shared;
using Domain.Security;
using SharedKernel;

namespace Application.Gaming.Entitlements;

internal sealed class EnableTenantGameEntitlementCommandHandler(
    ITenantGameEntitlementRepository gameEntitlementRepository,
    IResourceNodeRepository resourceNodeRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    IEntitlementCacheInvalidator cacheInvalidator)
    : ICommandHandler<EnableTenantGameEntitlementCommand>
{
    public async Task<Result> Handle(EnableTenantGameEntitlementCommand request, CancellationToken cancellationToken)
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
        DateTime now = dateTimeProvider.UtcNow;
        if (existing is null)
        {
            TenantGameEntitlement entitlement = TenantGameEntitlement.Create(request.TenantId, gameCode, now);
            gameEntitlementRepository.Insert(entitlement);
        }
        else
        {
            existing.Enable(now);
            gameEntitlementRepository.Update(existing);
        }

        await EnsureGameNodeAsync(request.TenantId, gameCode, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await cacheInvalidator.InvalidateTenantAsync(request.TenantId, cancellationToken);

        return Result.Success();
    }

    private static bool IsCatalogGame(GameCode gameCode)
    {
        PlayRuleRegistry registry = PlayRuleRegistry.CreateDefault();
        return registry.GetAllowedPlayTypes(gameCode).Count > 0;
    }

    private async Task<ResourceNode> EnsureGameNodeAsync(Guid tenantId, GameCode gameCode, CancellationToken cancellationToken)
    {
        Guid rootNodeId = await EnsureTenantRootNodeIdAsync(tenantId, cancellationToken);
        string externalKey = $"game:{gameCode.Value}";

        ResourceNode? existing = await resourceNodeRepository.GetByExternalKeyAsync(tenantId, externalKey, cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        ResourceNode node = ResourceNode.Create(gameCode.Value, externalKey, tenantId, rootNodeId);
        resourceNodeRepository.Insert(node);
        return node;
    }

    private async Task<Guid> EnsureTenantRootNodeIdAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        Guid? rootNodeId = await resourceNodeRepository.GetRootNodeIdAsync(tenantId, cancellationToken);
        if (rootNodeId.HasValue)
        {
            return rootNodeId.Value;
        }

        ResourceNode newRoot = ResourceNode.Create("Tenant Root", "root", tenantId);
        resourceNodeRepository.Insert(newRoot);
        return newRoot.Id;
    }
}
