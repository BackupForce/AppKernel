namespace Domain.Gaming;

public interface IPrizeRepository
{
    Task<Prize?> GetByIdAsync(Guid tenantId, Guid prizeId, CancellationToken cancellationToken = default);

    Task<bool> IsNameUniqueAsync(Guid tenantId, string name, CancellationToken cancellationToken = default);

    void Insert(Prize prize);

    void Update(Prize prize);
}
