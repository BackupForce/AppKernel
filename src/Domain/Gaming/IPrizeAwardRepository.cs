namespace Domain.Gaming;

public interface IPrizeAwardRepository
{
    Task<PrizeAward?> GetByIdAsync(Guid tenantId, Guid awardId, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Guid tenantId, Guid drawId, Guid ticketId, int lineIndex, CancellationToken cancellationToken = default);

    void Insert(PrizeAward award);

    void Update(PrizeAward award);
}
