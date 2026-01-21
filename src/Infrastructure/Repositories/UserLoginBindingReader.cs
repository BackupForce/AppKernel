using Application.Abstractions.Identity;
using Domain.Users;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class UserLoginBindingReader(ApplicationDbContext context) : IUserLoginBindingReader
{
    public Task<User?> FindUserByLoginAsync(
        Guid? tenantId,
        LoginProvider provider,
        string normalizedKey,
        CancellationToken cancellationToken = default)
    {
        return context.LoginBindings
            .AsNoTracking()
            .Where(binding => binding.TenantId == tenantId
                && binding.Provider == provider
                && binding.NormalizedProviderKey == normalizedKey)
            .Select(binding => binding.User!)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
