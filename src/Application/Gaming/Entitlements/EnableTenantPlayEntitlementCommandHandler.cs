using Application.Abstractions.Data;
using Application.Abstractions.Gaming;
using Application.Abstractions.Messaging;
using Domain.Gaming;
using Domain.Gaming.Services;
using Domain.Security;
using SharedKernel;

namespace Application.Gaming.Entitlements;

internal sealed class EnableTenantPlayEntitlementCommandHandler(
    ITenantPlayEntitlementRepository playEntitlementRepository,
    IResourceNodeRepository resourceNodeRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    IEntitlementCacheInvalidator cacheInvalidator)
    : ICommandHandler<EnableTenantPlayEntitlementCommand>
{
    public async Task<Result> Handle(EnableTenantPlayEntitlementCommand request, CancellationToken cancellationToken)
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

        DateTime now = dateTimeProvider.UtcNow;
        if (existing is null)
        {
            TenantPlayEntitlement entitlement = TenantPlayEntitlement.Create(request.TenantId, gameCode, playTypeCode, now);
            playEntitlementRepository.Insert(entitlement);
        }
        else
        {
            existing.Enable(now);
            playEntitlementRepository.Update(existing);
        }

        await EnsurePlayNodeAsync(request.TenantId, gameCode, playTypeCode, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await cacheInvalidator.InvalidateTenantAsync(request.TenantId, cancellationToken);

        return Result.Success();
    }

    private static bool IsCatalogPlay(GameCode gameCode, PlayTypeCode playTypeCode)
    {
        PlayRuleRegistry registry = PlayRuleRegistry.CreateDefault();
        return registry.GetAllowedPlayTypes(gameCode).Contains(playTypeCode);
    }

    private async Task<ResourceNode> EnsurePlayNodeAsync(
        Guid tenantId,
        GameCode gameCode,
        PlayTypeCode playTypeCode,
        CancellationToken cancellationToken)
    {
        ResourceNode gameNode = await EnsureGameNodeAsync(tenantId, gameCode, cancellationToken);
        string externalKey = $"play:{gameCode.Value}:{playTypeCode.Value}";

        ResourceNode? existing = await resourceNodeRepository.GetByExternalKeyAsync(tenantId, externalKey, cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        ResourceNode node = ResourceNode.Create(playTypeCode.Value, externalKey, tenantId, gameNode.Id);
        resourceNodeRepository.Insert(node);
        return node;
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
