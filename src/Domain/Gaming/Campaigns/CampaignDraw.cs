using SharedKernel;

namespace Domain.Gaming.Campaigns;

public sealed class CampaignDraw : Entity
{
    private CampaignDraw(Guid id, Guid tenantId, Guid campaignId, Guid drawId, DateTime createdAtUtc) : base(id)
    {
        TenantId = tenantId;
        CampaignId = campaignId;
        DrawId = drawId;
        CreatedAtUtc = createdAtUtc;
    }

    private CampaignDraw()
    {
    }

    public Guid TenantId { get; private set; }

    public Guid CampaignId { get; private set; }

    public Guid DrawId { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public static CampaignDraw Create(Guid tenantId, Guid campaignId, Guid drawId, DateTime utcNow)
    {
        return new CampaignDraw(Guid.NewGuid(), tenantId, campaignId, drawId, utcNow);
    }
}
