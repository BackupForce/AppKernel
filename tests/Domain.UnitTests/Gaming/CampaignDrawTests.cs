using Domain.Gaming.Campaigns;
using Domain.Gaming.Catalog;
using Domain.Gaming.Shared;
using FluentAssertions;
using SharedKernel;

namespace Domain.UnitTests.Gaming;

public sealed class CampaignDrawTests
{
    [Fact]
    public void AddDraw_Should_Reject_Duplicated_Draw()
    {
        Guid tenantId = Guid.NewGuid();
        DateTime now = DateTime.UtcNow;

        Campaign campaign = Campaign.Create(
            tenantId,
            GameCodes.Lottery539,
            PlayTypeCodes.Basic,
            "重複期數測試",
            now.AddHours(1),
            now.AddHours(2),
            CampaignStatus.Draft,
            now).Value;

        Guid drawId = Guid.NewGuid();
        Result firstAdd = campaign.AddDraw(drawId, now);
        Result secondAdd = campaign.AddDraw(drawId, now);

        firstAdd.IsSuccess.Should().BeTrue();
        secondAdd.IsFailure.Should().BeTrue();
        secondAdd.Error.Should().Be(GamingErrors.CampaignDrawDuplicated);
    }
}
