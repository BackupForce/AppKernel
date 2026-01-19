using Domain.Gaming.Campaigns;

namespace Domain.Gaming.Repositories;

public interface ICampaignDrawRepository
{
    Task<IReadOnlyCollection<CampaignDraw>> GetByCampaignIdAsync(
        Guid tenantId,
        Guid campaignId,
        CancellationToken cancellationToken = default);

    void Insert(CampaignDraw campaignDraw);
}
