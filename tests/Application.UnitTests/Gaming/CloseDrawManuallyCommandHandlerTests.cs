using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Gaming;
using Application.Abstractions.Time;
using Application.Gaming.Draws.ManualClose;
using Domain.Gaming.Catalog;
using Domain.Gaming.Draws;
using Domain.Gaming.Repositories;
using Domain.Gaming.Rules;
using Domain.Gaming.Tickets;
using FluentAssertions;
using NSubstitute;
using SharedKernel;

namespace Application.UnitTests.Gaming;

public sealed class CloseDrawManuallyCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Mark_Pending_TicketDraws_Invalid()
    {
        Guid tenantId = Guid.NewGuid();
        DateTime now = DateTime.UtcNow;

        Draw draw = Draw.Create(
            tenantId,
            GameCodes.Lottery539,
            "539-2401001",
            now.AddHours(-1),
            now.AddHours(1),
            now.AddHours(2),
            null,
            now,
            PlayRuleRegistry.CreateDefault()).Value;

        TicketDraw pending = TicketDraw.Create(tenantId, Guid.NewGuid(), draw.Id, now);

        IDrawRepository drawRepository = Substitute.For<IDrawRepository>();
        ITicketDrawRepository ticketDrawRepository = Substitute.For<ITicketDrawRepository>();
        IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
        IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
        ITenantContext tenantContext = Substitute.For<ITenantContext>();
        IEntitlementChecker entitlementChecker = Substitute.For<IEntitlementChecker>();

        drawRepository.GetByIdAsync(tenantId, draw.Id, Arg.Any<CancellationToken>()).Returns(draw);
        ticketDrawRepository.GetPendingForUnsubmittedTicketsAsync(tenantId, draw.Id, Arg.Any<CancellationToken>())
            .Returns(new[] { pending });
        entitlementChecker.EnsureGameEnabledAsync(tenantId, draw.GameCode, Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        dateTimeProvider.UtcNow.Returns(now);
        tenantContext.TenantId.Returns(tenantId);

        CloseDrawManuallyCommandHandler handler = new(
            drawRepository,
            ticketDrawRepository,
            unitOfWork,
            dateTimeProvider,
            tenantContext,
            entitlementChecker);

        Result result = await handler.Handle(new CloseDrawManuallyCommand(draw.Id, "maintenance"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        pending.ParticipationStatus.Should().Be(TicketDrawParticipationStatus.Invalid);
    }
}
