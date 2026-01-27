using Domain.Gaming.Catalog;

namespace Application.Abstractions.Gaming;

public interface IDrawSequenceRepository
{
    Task<int> GetNextSequenceAsync(
        Guid tenantId,
        GameCode gameCode,
        string prefix,
        CancellationToken cancellationToken);
}
