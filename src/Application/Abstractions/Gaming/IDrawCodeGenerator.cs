using Domain.Gaming.Catalog;

namespace Application.Abstractions.Gaming;

public interface IDrawCodeGenerator
{
    Task<string> IssueDrawCodeAsync(
        Guid tenantId,
        GameCode gameCode,
        DateTime drawAtUtc,
        DateTime nowUtc,
        CancellationToken cancellationToken);
}
