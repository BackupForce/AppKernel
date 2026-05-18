using SharedKernel;

namespace Application.Abstractions.Gaming;

/// <summary>
/// 點數/帳本服務介面，提供扣點與餘額查詢。
/// </summary>
public interface IWalletLedgerService
{
    /// <summary>
    /// 扣點並回傳帳本流水號，reference 用於防止重複扣點。
    /// </summary>
    Task<Result<long>> DebitAsync(
        Guid tenantId,
        Guid memberId,
        long amount,
        string referenceType,
        string? referenceId,
        string remark,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 取得會員餘額。
    /// </summary>
    Task<long?> GetBalanceAsync(Guid memberId, CancellationToken cancellationToken = default);
}
