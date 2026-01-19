using Domain.Gaming.Campaigns;

namespace Domain.Gaming.Repositories;

public interface ICampaignRepository
{
    Task<Campaign?> GetByIdAsync(Guid tenantId, Guid campaignId, CancellationToken cancellationToken = default);

    void Insert(Campaign campaign);

    void Update(Campaign campaign);
}
