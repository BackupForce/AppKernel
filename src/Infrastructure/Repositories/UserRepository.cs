using Domain.Users;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class UserRepository(ApplicationDbContext context) : IUserRepository
{
    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return context.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public Task<User?> GetByIdWithRolesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return context.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public Task<User?> GetByIdWithGroupsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return context.Users
            .Include(u => u.UserGroups)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        return context.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public Task<bool> IsInTenantAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return context.UserTenants
            .AsNoTracking()
            .AnyAsync(userTenant => userTenant.UserId == userId && userTenant.TenantId == tenantId, cancellationToken);
    }

    public async Task<bool> IsEmailUniqueAsync(Email email)
    {
        return !await context.Users.AnyAsync(u => u.Email == email);
    }

    public void Insert(User user)
    {
        context.Users.Add(user);
    }
}
