using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Gaming;
using Application.Gaming.Tickets.Place;
using Domain.Gaming.Catalog;
using Domain.Gaming.Draws;
using Domain.Gaming.Repositories;
using Domain.Gaming.Rules;
using Domain.Gaming.Shared;
using Domain.Gaming.TicketTemplates;
using Domain.Members;
using FluentAssertions;
using NSubstitute;

namespace Application.UnitTests.Gaming;

public sealed class PlaceTicketCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Reject_When_PlayType_Not_Enabled()
    {
        Guid tenantId = Guid.NewGuid();
        DateTime now = DateTime.UtcNow;

        Draw draw = Draw.Create(
            tenantId,
            GameCodes.Lottery539,
            "539-2602001",
            now.AddHours(-1),
            now.AddHours(1),
            now.AddHours(2),
            null,
            now,
            PlayRuleRegistry.CreateDefault()).Value;
        Guid drawId = draw.Id;

        IDrawRepository drawRepository = Substitute.For<IDrawRepository>();
        ITicketRepository ticketRepository = Substitute.For<ITicketRepository>();
        ITicketDrawRepository ticketDrawRepository = Substitute.For<ITicketDrawRepository>();
        ITicketTemplateRepository ticketTemplateRepository = Substitute.For<ITicketTemplateRepository>();
        IDrawAllowedTicketTemplateRepository drawAllowedTicketTemplateRepository = Substitute.For<IDrawAllowedTicketTemplateRepository>();
        IMemberRepository memberRepository = Substitute.For<IMemberRepository>();
        IServerSeedStore serverSeedStore = Substitute.For<IServerSeedStore>();
        IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
        IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
        ITenantContext tenantContext = Substitute.For<ITenantContext>();
        IUserContext userContext = Substitute.For<IUserContext>();
        ILottery539RngService rngService = Substitute.For<ILottery539RngService>();
        IEntitlementChecker entitlementChecker = Substitute.For<IEntitlementChecker>();

        drawRepository.GetByIdAsync(tenantId, drawId, Arg.Any<CancellationToken>())
            .Returns(draw);
        dateTimeProvider.UtcNow.Returns(now);
        tenantContext.TenantId.Returns(tenantId);
        entitlementChecker.EnsurePlayEnabledAsync(tenantId, draw.GameCode, PlayTypeCodes.Basic, Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        PlaceTicketCommandHandler handler = new(
            drawRepository,
            ticketRepository,
            ticketDrawRepository,
            ticketTemplateRepository,
            drawAllowedTicketTemplateRepository,
            memberRepository,
            serverSeedStore,
            unitOfWork,
            dateTimeProvider,
            tenantContext,
            userContext,
            rngService,
            entitlementChecker);

        Result<Guid> result = await handler.Handle(
            new PlaceTicketCommand(drawId, PlayTypeCodes.Basic.Value, Guid.NewGuid(), new[] { new[] { 1, 2, 3, 4, 5 } }),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(GamingErrors.TicketPlayTypeNotEnabled);
    }

    [Fact]
    public async Task Handle_Should_Reject_When_TicketTemplate_Not_Allowed()
    {
        Guid tenantId = Guid.NewGuid();
        DateTime now = DateTime.UtcNow;
        Guid ticketTemplateId = Guid.NewGuid();

        Draw draw = Draw.Create(
            tenantId,
            GameCodes.Lottery539,
            "539-2602002",
            now.AddHours(-1),
            now.AddHours(1),
            now.AddHours(2),
            null,
            now,
            PlayRuleRegistry.CreateDefault()).Value;
        draw.EnablePlayTypes(new[] { PlayTypeCodes.Basic }, PlayRuleRegistry.CreateDefault());
        Guid drawId = draw.Id;

        TicketTemplate template = TicketTemplate.Create(
            tenantId,
            "TT01",
            "Template",
            TicketTemplateType.Standard,
            10m,
            now.AddDays(-1),
            now.AddDays(1),
            1,
            now).Value;

        IDrawRepository drawRepository = Substitute.For<IDrawRepository>();
        ITicketRepository ticketRepository = Substitute.For<ITicketRepository>();
        ITicketDrawRepository ticketDrawRepository = Substitute.For<ITicketDrawRepository>();
        ITicketTemplateRepository ticketTemplateRepository = Substitute.For<ITicketTemplateRepository>();
        IDrawAllowedTicketTemplateRepository drawAllowedTicketTemplateRepository = Substitute.For<IDrawAllowedTicketTemplateRepository>();
        IMemberRepository memberRepository = Substitute.For<IMemberRepository>();
        IServerSeedStore serverSeedStore = Substitute.For<IServerSeedStore>();
        IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
        IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
        ITenantContext tenantContext = Substitute.For<ITenantContext>();
        IUserContext userContext = Substitute.For<IUserContext>();
        ILottery539RngService rngService = Substitute.For<ILottery539RngService>();
        IEntitlementChecker entitlementChecker = Substitute.For<IEntitlementChecker>();

        drawRepository.GetByIdAsync(tenantId, drawId, Arg.Any<CancellationToken>())
            .Returns(draw);
        ticketTemplateRepository.GetByIdAsync(tenantId, ticketTemplateId, Arg.Any<CancellationToken>())
            .Returns(template);
        memberRepository.GetByUserIdAsync(tenantId, Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Member.Create(tenantId, null, "M002", "Tester", now).Value);
        drawAllowedTicketTemplateRepository.GetByDrawIdAsync(tenantId, drawId, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<DrawAllowedTicketTemplate>());
        dateTimeProvider.UtcNow.Returns(now);
        tenantContext.TenantId.Returns(tenantId);
        userContext.UserId.Returns(Guid.NewGuid());
        entitlementChecker.EnsurePlayEnabledAsync(tenantId, draw.GameCode, PlayTypeCodes.Basic, Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        rngService.CreateServerSeed().Returns("seed");
        rngService.ComputeServerSeedHash("seed").Returns("hash");
        serverSeedStore.StoreAsync(draw.Id, Arg.Any<string>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        PlaceTicketCommandHandler handler = new(
            drawRepository,
            ticketRepository,
            ticketDrawRepository,
            ticketTemplateRepository,
            drawAllowedTicketTemplateRepository,
            memberRepository,
            serverSeedStore,
            unitOfWork,
            dateTimeProvider,
            tenantContext,
            userContext,
            rngService,
            entitlementChecker);

        Result<Guid> result = await handler.Handle(
            new PlaceTicketCommand(drawId, PlayTypeCodes.Basic.Value, ticketTemplateId, new[] { new[] { 1, 2, 3, 4, 5 } }),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(GamingErrors.TicketTemplateNotAllowed);
    }
}
