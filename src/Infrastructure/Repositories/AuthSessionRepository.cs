using Domain.Auth;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class AuthSessionRepository(ApplicationDbContext context) : IAuthSessionRepository
{
    public Task<AuthSession?> GetByIdAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        return context.AuthSessions
            .Include(s => s.RefreshTokens)
            .FirstOrDefaultAsync(s => s.Id == sessionId, cancellationToken);
    }

    public async Task<IReadOnlyList<AuthSession>> GetByUserAsync(
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await context.AuthSessions
            .Where(s => s.TenantId == tenantId && s.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public void Insert(AuthSession session)
    {
        context.AuthSessions.Add(session);
    }
}
