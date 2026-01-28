using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Gaming;
using Application.Gaming.Draws.Create;
using Domain.Gaming.Catalog;
using Domain.Gaming.DrawTemplates;
using Domain.Gaming.Draws;
using Domain.Gaming.Repositories;
using Domain.Gaming.Rules;
using FluentAssertions;
using NSubstitute;
using SharedKernel;

namespace Application.UnitTests.Gaming;

public sealed class CreateDrawCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Copy_Template_Content_Into_Draw()
    {
        Guid tenantId = Guid.NewGuid();
        DateTime now = DateTime.UtcNow;
        Guid ticketTemplateId = Guid.NewGuid();

        DrawTemplate template = BuildTemplate(tenantId, ticketTemplateId, now);
        int templateVersion = template.Version;

        IDrawRepository drawRepository = Substitute.For<IDrawRepository>();
        IDrawTemplateRepository drawTemplateRepository = Substitute.For<IDrawTemplateRepository>();
        IDrawCodeGenerator drawCodeGenerator = Substitute.For<IDrawCodeGenerator>();
        IDrawAllowedTicketTemplateRepository drawAllowedTicketTemplateRepository = Substitute.For<IDrawAllowedTicketTemplateRepository>();
        IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
        IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
        ITenantContext tenantContext = Substitute.For<ITenantContext>();
        ILottery539RngService rngService = Substitute.For<ILottery539RngService>();
        IServerSeedStore serverSeedStore = Substitute.For<IServerSeedStore>();
        IEntitlementChecker entitlementChecker = Substitute.For<IEntitlementChecker>();

        drawTemplateRepository.GetByIdAsync(tenantId, template.Id, Arg.Any<CancellationToken>())
            .Returns(template);
        drawCodeGenerator.IssueDrawCodeAsync(tenantId, GameCodes.Lottery539, Arg.Any<DateTime>(), now, Arg.Any<CancellationToken>())
            .Returns("539-2601001");
        entitlementChecker.EnsureGameEnabledAsync(tenantId, GameCodes.Lottery539, Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        entitlementChecker.EnsurePlayEnabledAsync(tenantId, GameCodes.Lottery539, PlayTypeCodes.Basic, Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        dateTimeProvider.UtcNow.Returns(now);
        tenantContext.TenantId.Returns(tenantId);

        Draw? inserted = null;
        drawRepository.When(repo => repo.Insert(Arg.Any<Draw>()))
            .Do(callInfo => inserted = callInfo.Arg<Draw>());

        CreateDrawCommandHandler handler = new(
            drawRepository,
            drawTemplateRepository,
            drawCodeGenerator,
            drawAllowedTicketTemplateRepository,
            unitOfWork,
            dateTimeProvider,
            tenantContext,
            rngService,
            serverSeedStore,
            entitlementChecker);

        Result<Guid> result = await handler.Handle(
            new CreateDrawCommand(
                GameCodes.Lottery539.Value,
                template.Id,
                now.AddHours(1),
                now.AddHours(2),
                now.AddHours(3),
                null),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        inserted.Should().NotBeNull();
        inserted!.EnabledPlayTypes.Should().ContainSingle().Which.Should().Be(PlayTypeCodes.Basic);
        DrawPrizePoolItem? prizeItem = inserted.PrizePool
            .FirstOrDefault(item => item.PlayTypeCode == PlayTypeCodes.Basic && item.Tier == new PrizeTier("T1"));
        prizeItem.Should().NotBeNull();
        prizeItem!.Option!.Name.Should().Be("Template Prize");
        inserted.SourceTemplateId.Should().Be(template.Id);
        inserted.SourceTemplateVersion.Should().Be(templateVersion);

        drawAllowedTicketTemplateRepository.Received(1)
            .Insert(Arg.Is<DrawAllowedTicketTemplate>(item => item.TicketTemplateId == ticketTemplateId));
        drawTemplateRepository.Received(1).Update(template);
    }

    [Fact]
    public async Task Handle_Should_Not_Change_Draw_When_Template_Changes()
    {
        Guid tenantId = Guid.NewGuid();
        DateTime now = DateTime.UtcNow;
        Guid ticketTemplateId = Guid.NewGuid();

        DrawTemplate template = BuildTemplate(tenantId, ticketTemplateId, now);

        IDrawRepository drawRepository = Substitute.For<IDrawRepository>();
        IDrawTemplateRepository drawTemplateRepository = Substitute.For<IDrawTemplateRepository>();
        IDrawCodeGenerator drawCodeGenerator = Substitute.For<IDrawCodeGenerator>();
        IDrawAllowedTicketTemplateRepository drawAllowedTicketTemplateRepository = Substitute.For<IDrawAllowedTicketTemplateRepository>();
        IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
        IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
        ITenantContext tenantContext = Substitute.For<ITenantContext>();
        ILottery539RngService rngService = Substitute.For<ILottery539RngService>();
        IServerSeedStore serverSeedStore = Substitute.For<IServerSeedStore>();
        IEntitlementChecker entitlementChecker = Substitute.For<IEntitlementChecker>();

        drawTemplateRepository.GetByIdAsync(tenantId, template.Id, Arg.Any<CancellationToken>())
            .Returns(template);
        drawCodeGenerator.IssueDrawCodeAsync(tenantId, GameCodes.Lottery539, Arg.Any<DateTime>(), now, Arg.Any<CancellationToken>())
            .Returns("539-2601002");
        entitlementChecker.EnsureGameEnabledAsync(tenantId, GameCodes.Lottery539, Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        entitlementChecker.EnsurePlayEnabledAsync(tenantId, GameCodes.Lottery539, PlayTypeCodes.Basic, Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        dateTimeProvider.UtcNow.Returns(now);
        tenantContext.TenantId.Returns(tenantId);

        Draw? inserted = null;
        drawRepository.When(repo => repo.Insert(Arg.Any<Draw>()))
            .Do(callInfo => inserted = callInfo.Arg<Draw>());

        CreateDrawCommandHandler handler = new(
            drawRepository,
            drawTemplateRepository,
            drawCodeGenerator,
            drawAllowedTicketTemplateRepository,
            unitOfWork,
            dateTimeProvider,
            tenantContext,
            rngService,
            serverSeedStore,
            entitlementChecker);

        await handler.Handle(
            new CreateDrawCommand(
                GameCodes.Lottery539.Value,
                template.Id,
                now.AddHours(1),
                now.AddHours(2),
                now.AddHours(3),
                null),
            CancellationToken.None);

        template.AddPlayType(new PlayTypeCode("EXTRA"));

        inserted.Should().NotBeNull();
        inserted!.EnabledPlayTypes.Should().ContainSingle();
    }

    private static DrawTemplate BuildTemplate(Guid tenantId, Guid ticketTemplateId, DateTime now)
    {
        DrawTemplate template = DrawTemplate.Create(
            tenantId,
            GameCodes.Lottery539,
            "Default Template",
            true,
            now).Value;

        template.AddPlayType(PlayTypeCodes.Basic);
        PrizeOption option = PrizeOption.Create("Template Prize", 10m, null, "snapshot").Value;
        template.UpsertPrizeTier(PlayTypeCodes.Basic, new PrizeTier("T1"), option);
        template.AddAllowedTicketTemplate(ticketTemplateId, now);
        return template;
    }
}
