using Domain.Gaming;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class PrizeAwardRepository(ApplicationDbContext context) : IPrizeAwardRepository
{
    public async Task<PrizeAward?> GetByIdAsync(Guid tenantId, Guid awardId, CancellationToken cancellationToken = default)
    {
        return await context.PrizeAwards
            .FirstOrDefaultAsync(award => award.TenantId == tenantId && award.Id == awardId, cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        Guid tenantId,
        Guid drawId,
        Guid ticketId,
        int lineIndex,
        CancellationToken cancellationToken = default)
    {
        return await context.PrizeAwards.AnyAsync(
            award => award.TenantId == tenantId
                     && award.DrawId == drawId
                     && award.TicketId == ticketId
                     && award.LineIndex == lineIndex,
            cancellationToken);
    }

    public void Insert(PrizeAward award)
    {
        context.PrizeAwards.Add(award);
    }

    public void Update(PrizeAward award)
    {
        context.PrizeAwards.Update(award);
    }
}
