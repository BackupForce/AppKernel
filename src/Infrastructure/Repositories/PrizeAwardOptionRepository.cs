using Domain.Gaming;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class PrizeAwardOptionRepository(ApplicationDbContext context) : IPrizeAwardOptionRepository
{
    public async Task<IReadOnlyCollection<PrizeAwardOption>> GetByAwardIdAsync(
        Guid tenantId,
        Guid awardId,
        CancellationToken cancellationToken = default)
    {
        List<PrizeAwardOption> items = await context.PrizeAwardOptions
            .Where(option => option.TenantId == tenantId && option.PrizeAwardId == awardId)
            .ToListAsync(cancellationToken);

        return items;
    }

    public void InsertRange(IEnumerable<PrizeAwardOption> options)
    {
        context.PrizeAwardOptions.AddRange(options);
    }
}
