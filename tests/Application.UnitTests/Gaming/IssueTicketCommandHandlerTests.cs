using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Gaming;
using Application.Abstractions.Time;
using Application.Gaming.Tickets.Issue;
using Domain.Gaming.Campaigns;
using Domain.Gaming.Catalog;
using Domain.Gaming.Draws;
using Domain.Gaming.Repositories;
using Domain.Gaming.Rules;
using Domain.Gaming.Tickets;
using Domain.Members;
using FluentAssertions;
using NSubstitute;
using SharedKernel;

namespace Application.UnitTests.Gaming;

public sealed class IssueTicketCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Issue_Ticket_For_Eligible_Draws()
    {
        Guid tenantId = Guid.NewGuid();
        Guid memberId = Guid.NewGuid();
        DateTime now = DateTime.UtcNow;

        Campaign campaign = Campaign.Create(
            tenantId,
            GameCodes.Lottery539,
            PlayTypeCodes.Basic,
            "秋季活動",
            now.AddHours(-1),
            now.AddHours(2),
            CampaignStatus.Active,
            now).Value;

        Draw draw1 = CreateDraw(tenantId, now.AddMinutes(-10), now.AddMinutes(10), now.AddHours(1));
        Draw draw2 = CreateDraw(tenantId, now.AddHours(-3), now.AddHours(-2), now.AddHours(-1));
        draw1.EnablePlayTypes(new[] { PlayTypeCodes.Basic }, PlayRuleRegistry.CreateDefault());
        draw2.EnablePlayTypes(new[] { PlayTypeCodes.Basic }, PlayRuleRegistry.CreateDefault());

        CampaignDraw campaignDraw1 = CampaignDraw.Create(tenantId, campaign.Id, draw1.Id, now);
        CampaignDraw campaignDraw2 = CampaignDraw.Create(tenantId, campaign.Id, draw2.Id, now);

        ICampaignRepository campaignRepository = Substitute.For<ICampaignRepository>();
        ICampaignDrawRepository campaignDrawRepository = Substitute.For<ICampaignDrawRepository>();
        IDrawRepository drawRepository = Substitute.For<IDrawRepository>();
        ITicketRepository ticketRepository = Substitute.For<ITicketRepository>();
        ITicketDrawRepository ticketDrawRepository = Substitute.For<ITicketDrawRepository>();
        ITicketTemplateRepository ticketTemplateRepository = Substitute.For<ITicketTemplateRepository>();
        IMemberRepository memberRepository = Substitute.For<IMemberRepository>();
        IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
        IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
        ITenantContext tenantContext = Substitute.For<ITenantContext>();
        IUserContext userContext = Substitute.For<IUserContext>();
        IEntitlementChecker entitlementChecker = Substitute.For<IEntitlementChecker>();

        campaignRepository.GetByIdAsync(tenantId, campaign.Id, Arg.Any<CancellationToken>()).Returns(campaign);
        campaignDrawRepository.GetByCampaignIdAsync(tenantId, campaign.Id, Arg.Any<CancellationToken>())
            .Returns(new[] { campaignDraw1, campaignDraw2 });
        drawRepository.GetByIdAsync(tenantId, draw1.Id, Arg.Any<CancellationToken>()).Returns(draw1);
        drawRepository.GetByIdAsync(tenantId, draw2.Id, Arg.Any<CancellationToken>()).Returns(draw2);
        memberRepository.GetByIdAsync(tenantId, memberId, Arg.Any<CancellationToken>())
            .Returns(Member.Create(tenantId, null, "M001", "Tester", now).Value);
        entitlementChecker.EnsurePlayEnabledAsync(tenantId, campaign.GameCode, campaign.PlayTypeCode, Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        dateTimeProvider.UtcNow.Returns(now);
        tenantContext.TenantId.Returns(tenantId);
        userContext.UserId.Returns(Guid.NewGuid());

        IssueTicketCommandHandler handler = new(
            campaignRepository,
            campaignDrawRepository,
            drawRepository,
            ticketRepository,
            ticketDrawRepository,
            ticketTemplateRepository,
            memberRepository,
            unitOfWork,
            dateTimeProvider,
            tenantContext,
            userContext,
            entitlementChecker);

        Result<IssueTicketResult> result = await handler.Handle(
            new IssueTicketCommand(memberId, campaign.Id, null, "manual"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.DrawIds.Should().ContainSingle().Which.Should().Be(draw1.Id);
        ticketDrawRepository.Received(1).Insert(Arg.Any<TicketDraw>());
    }

    private static Draw CreateDraw(Guid tenantId, DateTime openAt, DateTime closeAt, DateTime drawAt)
    {
        return Draw.Create(
            tenantId,
            GameCodes.Lottery539,
            "539-2401001",
            openAt,
            closeAt,
            drawAt,
            null,
            DateTime.UtcNow,
            PlayRuleRegistry.CreateDefault()).Value;
    }
}
