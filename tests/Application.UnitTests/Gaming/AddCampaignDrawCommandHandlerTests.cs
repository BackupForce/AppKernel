using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Time;
using Application.Gaming.Campaigns.Draws.Add;
using Domain.Gaming.Campaigns;
using Domain.Gaming.Catalog;
using Domain.Gaming.Repositories;
using Domain.Gaming.Shared;
using FluentAssertions;
using NSubstitute;
using SharedKernel;

namespace Application.UnitTests.Gaming;

public sealed class AddCampaignDrawCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Reject_When_Campaign_Not_Draft()
    {
        Guid tenantId = Guid.NewGuid();
        DateTime now = DateTime.UtcNow;
        Campaign campaign = Campaign.Create(
            tenantId,
            GameCodes.Lottery539,
            PlayTypeCodes.Basic,
            "非草稿活動",
            now.AddHours(-2),
            now.AddHours(2),
            CampaignStatus.Active,
            now).Value;

        ICampaignRepository campaignRepository = Substitute.For<ICampaignRepository>();
        IDrawRepository drawRepository = Substitute.For<IDrawRepository>();
        IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
        IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
        ITenantContext tenantContext = Substitute.For<ITenantContext>();

        campaignRepository.GetByIdAsync(tenantId, campaign.Id, Arg.Any<CancellationToken>())
            .Returns(campaign);
        tenantContext.TenantId.Returns(tenantId);

        AddCampaignDrawCommandHandler handler = new(
            campaignRepository,
            drawRepository,
            unitOfWork,
            dateTimeProvider,
            tenantContext);

        Result result = await handler.Handle(
            new AddCampaignDrawCommand(tenantId, campaign.Id, Guid.NewGuid()),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(GamingErrors.CampaignNotDraft);
        await drawRepository.DidNotReceiveWithAnyArgs()
            .GetByIdAsync(default, default, default);
    }
}
