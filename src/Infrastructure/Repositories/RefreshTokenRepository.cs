using Domain.Auth;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class RefreshTokenRepository(ApplicationDbContext context) : IRefreshTokenRepository
{
    public Task<RefreshTokenRecord?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        return context.RefreshTokenRecords
            .Include(t => t.Session)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);
    }

    public async Task<IReadOnlyList<RefreshTokenRecord>> GetBySessionIdAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        return await context.RefreshTokenRecords
            .Where(t => t.SessionId == sessionId)
            .ToListAsync(cancellationToken);
    }

    public void Insert(RefreshTokenRecord record)
    {
        context.RefreshTokenRecords.Add(record);
    }
}
