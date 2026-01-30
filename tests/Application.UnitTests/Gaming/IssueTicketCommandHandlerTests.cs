using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Gaming;
using Application.Abstractions.Time;
using Application.Gaming.Tickets.Issue;
using Application.Gaming.Tickets.Services;
using Domain.Gaming.DrawGroups;
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

        DrawGroup drawGroup = DrawGroup.Create(
            tenantId,
            GameCodes.Lottery539,
            PlayTypeCodes.Basic,
            "秋季活動",
            now.AddHours(-1),
            now.AddHours(2),
            DrawGroupStatus.Active,
            now).Value;

        Draw draw1 = CreateDraw(tenantId, now.AddMinutes(-10), now.AddMinutes(10), now.AddHours(1));
        Draw draw2 = CreateDraw(tenantId, now.AddHours(-3), now.AddHours(-2), now.AddHours(-1));
        draw1.EnablePlayTypes(new[] { PlayTypeCodes.Basic }, PlayRuleRegistry.CreateDefault());
        draw2.EnablePlayTypes(new[] { PlayTypeCodes.Basic }, PlayRuleRegistry.CreateDefault());

        DrawGroupDraw drawGroupDraw1 = DrawGroupDraw.Create(tenantId, drawGroup.Id, draw1.Id, now);
        DrawGroupDraw drawGroupDraw2 = DrawGroupDraw.Create(tenantId, drawGroup.Id, draw2.Id, now);

        IDrawGroupRepository drawGroupRepository = Substitute.For<IDrawGroupRepository>();
        IDrawGroupDrawRepository drawGroupDrawRepository = Substitute.For<IDrawGroupDrawRepository>();
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

        drawGroupRepository.GetByIdAsync(tenantId, drawGroup.Id, Arg.Any<CancellationToken>()).Returns(drawGroup);
        drawGroupDrawRepository.GetByDrawGroupIdAsync(tenantId, drawGroup.Id, Arg.Any<CancellationToken>())
            .Returns(new[] { drawGroupDraw1, drawGroupDraw2 });
        drawRepository.GetByIdAsync(tenantId, draw1.Id, Arg.Any<CancellationToken>()).Returns(draw1);
        drawRepository.GetByIdAsync(tenantId, draw2.Id, Arg.Any<CancellationToken>()).Returns(draw2);
        memberRepository.GetByIdAsync(tenantId, memberId, Arg.Any<CancellationToken>())
            .Returns(Member.Create(tenantId, null, "M001", "Tester", now).Value);
        entitlementChecker.EnsurePlayEnabledAsync(tenantId, drawGroup.GameCode, drawGroup.PlayTypeCode, Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        dateTimeProvider.UtcNow.Returns(now);
        tenantContext.TenantId.Returns(tenantId);
        userContext.UserId.Returns(Guid.NewGuid());

        TicketIssuanceService ticketIssuanceService = new(ticketRepository, ticketDrawRepository);

        IssueTicketCommandHandler handler = new(
            drawGroupRepository,
            drawGroupDrawRepository,
            drawRepository,
            ticketTemplateRepository,
            memberRepository,
            ticketIssuanceService,
            unitOfWork,
            dateTimeProvider,
            tenantContext,
            userContext,
            entitlementChecker);

        Result<IssueTicketResult> result = await handler.Handle(
            new IssueTicketCommand(memberId, drawGroup.Id, null, "manual"),
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
