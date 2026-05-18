using Application.Gaming.Dtos;
using SharedKernel;

namespace Application.Abstractions.Gaming;

public enum AdminWinningRedemptionResult
{
    Success = 0,
    NotFound = 1,
    Conflict = 2
}

public interface IAdminWinningRedemptionRepository
{
    Task<AdminWinningRedemptionResult> RedeemAsync(
        Guid tenantId,
        Guid winningId,
        Guid redeemedByUserId,
        DateTime redeemedAtUtc,
        CancellationToken cancellationToken = default);

    Task<Result<AdminWinningDetailDto>> GetWinningDetailAsync(
        Guid tenantId,
        Guid winningId,
        CancellationToken cancellationToken = default);
}
