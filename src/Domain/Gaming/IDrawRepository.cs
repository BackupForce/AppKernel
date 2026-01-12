namespace Domain.Gaming;

public interface IDrawRepository
{
    Task<Draw?> GetByIdAsync(Guid tenantId, Guid drawId, CancellationToken cancellationToken = default);

    void Insert(Draw draw);

    void Update(Draw draw);
}
