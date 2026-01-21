using Domain.Gaming.Campaigns;
using Domain.Gaming.Repositories;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class CampaignDrawRepository(ApplicationDbContext context) : ICampaignDrawRepository
{
    public async Task<IReadOnlyCollection<CampaignDraw>> GetByCampaignIdAsync(
        Guid tenantId,
        Guid campaignId,
        CancellationToken cancellationToken = default)
    {
        return await context.CampaignDraws
            .Where(item => item.TenantId == tenantId && item.CampaignId == campaignId)
            .ToListAsync(cancellationToken);
    }
}
