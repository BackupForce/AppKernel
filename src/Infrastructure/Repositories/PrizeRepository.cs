using Domain.Gaming;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class PrizeRepository(ApplicationDbContext context) : IPrizeRepository
{
    public async Task<Prize?> GetByIdAsync(Guid tenantId, Guid prizeId, CancellationToken cancellationToken = default)
    {
        return await context.Prizes
            .FirstOrDefaultAsync(prize => prize.TenantId == tenantId && prize.Id == prizeId, cancellationToken);
    }

    public async Task<bool> IsNameUniqueAsync(Guid tenantId, string name, CancellationToken cancellationToken = default)
    {
        return !await context.Prizes
            .AnyAsync(prize => prize.TenantId == tenantId && prize.Name == name, cancellationToken);
    }

    public void Insert(Prize prize)
    {
        context.Prizes.Add(prize);
    }

    public void Update(Prize prize)
    {
        context.Prizes.Update(prize);
    }
}
