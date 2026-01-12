using SharedKernel;

namespace Application.Abstractions.Gaming;

public interface IWalletLedgerService
{
    Task<Result<long>> DebitAsync(
        Guid tenantId,
        Guid memberId,
        long amount,
        string referenceType,
        string? referenceId,
        string remark,
        CancellationToken cancellationToken = default);

    Task<long?> GetBalanceAsync(Guid memberId, CancellationToken cancellationToken = default);
}
