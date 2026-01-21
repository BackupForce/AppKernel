using Application.IntegrationTests.Infrastructure;
using Domain.Gaming.Campaigns;
using Domain.Gaming.Catalog;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.IntegrationTests.Gaming;

public sealed class CampaignDrawPersistenceTests : BaseIntegrationTest
{
    public CampaignDrawPersistenceTests(IntegrationTestWebAppFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task AddDraw_Should_Persist_CampaignDraw()
    {
        Guid tenantId = Guid.NewGuid();
        DateTime now = DateTime.UtcNow;
        Campaign campaign = Campaign.Create(
            tenantId,
            GameCodes.Lottery539,
            PlayTypeCodes.Basic,
            "持久化測試活動",
            now.AddHours(1),
            now.AddHours(2),
            CampaignStatus.Draft,
            now).Value;

        DbContext.Campaigns.Add(campaign);
        await DbContext.SaveChangesAsync();

        Result addResult = campaign.AddDraw(Guid.NewGuid(), now);
        addResult.IsSuccess.Should().BeTrue();

        await DbContext.SaveChangesAsync();

        CampaignDraw? persisted = await DbContext.CampaignDraws
            .FirstOrDefaultAsync(item => item.TenantId == tenantId && item.CampaignId == campaign.Id);

        persisted.Should().NotBeNull();
    }

    [Fact]
    public async Task RemoveCampaign_Should_Cascade_Delete_Draws()
    {
        Guid tenantId = Guid.NewGuid();
        DateTime now = DateTime.UtcNow;
        Campaign campaign = Campaign.Create(
            tenantId,
            GameCodes.Lottery539,
            PlayTypeCodes.Basic,
            "刪除測試活動",
            now.AddHours(1),
            now.AddHours(2),
            CampaignStatus.Draft,
            now).Value;

        DbContext.Campaigns.Add(campaign);
        await DbContext.SaveChangesAsync();

        Result addResult = campaign.AddDraw(Guid.NewGuid(), now);
        addResult.IsSuccess.Should().BeTrue();
        await DbContext.SaveChangesAsync();

        DbContext.Campaigns.Remove(campaign);
        await DbContext.SaveChangesAsync();

        int remaining = await DbContext.CampaignDraws
            .CountAsync(item => item.TenantId == tenantId && item.CampaignId == campaign.Id);

        remaining.Should().Be(0);
    }
}
