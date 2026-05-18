using Domain.Gaming.Catalog;
using SharedKernel;

namespace Application.Abstractions.Gaming;

public interface IEntitlementChecker
{
    Task<Result> EnsureGameEnabledAsync(Guid tenantId, GameCode gameCode, CancellationToken cancellationToken = default);

    Task<Result> EnsurePlayEnabledAsync(
        Guid tenantId,
        GameCode gameCode,
        PlayTypeCode playTypeCode,
        CancellationToken cancellationToken = default);

    Task<TenantEntitlementsDto> GetTenantEntitlementsAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
