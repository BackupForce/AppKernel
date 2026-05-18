using Domain.Gaming.Catalog;

namespace Application.Abstractions.Gaming;

public interface IDrawSequenceService
{
    Task<long> IssueNextAsync(
        Guid tenantId,
        GameCode gameCode,
        DateTime nowUtc,
        CancellationToken cancellationToken);
}
