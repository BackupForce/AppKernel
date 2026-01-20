using Domain.Gaming.Campaigns;
using Domain.Gaming.Repositories;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

internal sealed class CampaignRepository(ApplicationDbContext context) : ICampaignRepository
{
    public async Task<Campaign?> GetByIdAsync(Guid tenantId, Guid campaignId, CancellationToken cancellationToken = default)
    {
        return await context.Campaigns
            .Include(campaign => campaign.Draws)
            .FirstOrDefaultAsync(
                campaign => campaign.TenantId == tenantId && campaign.Id == campaignId,
                cancellationToken);
    }

    public void Insert(Campaign campaign)
    {
        context.Campaigns.Add(campaign);
    }

    public void Update(Campaign campaign)
    {
        context.Campaigns.Update(campaign);
    }

    public void Remove(Campaign campaign)
    {
        context.Campaigns.Remove(campaign);
    }
}
