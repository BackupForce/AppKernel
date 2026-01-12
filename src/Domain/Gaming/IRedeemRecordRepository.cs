namespace Domain.Gaming;

public interface IRedeemRecordRepository
{
    Task<RedeemRecord?> GetByAwardIdAsync(Guid prizeAwardId, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Guid prizeAwardId, CancellationToken cancellationToken = default);

    void Insert(RedeemRecord record);
}
