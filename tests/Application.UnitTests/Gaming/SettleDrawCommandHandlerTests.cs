using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Gaming;
using Application.Gaming.Draws.Settle;
using Domain.Gaming.Catalog;
using Domain.Gaming.Draws;
using Domain.Gaming.Repositories;
using Domain.Gaming.Rules;
using Domain.Gaming.Shared;
using Domain.Gaming.Tickets;
using FluentAssertions;
using NSubstitute;
using SharedKernel;

namespace Application.UnitTests.Gaming;

public sealed class SettleDrawCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Mark_Draw_Settled_When_No_Tickets()
    {
        Guid tenantId = Guid.NewGuid();
        DateTime now = DateTime.UtcNow;

        Draw draw = Draw.Create(
            tenantId,
            GameCodes.Lottery539,
            "539-2401001",
            now.AddHours(-2),
            now.AddHours(-1),
            now.AddHours(-1),
            null,
            now.AddHours(-3),
            PlayRuleRegistry.CreateDefault()).Value;

        LotteryNumbers winningNumbers = LotteryNumbers.Create(new[] { 1, 2, 3, 4, 5 }).Value;
        draw.Execute(winningNumbers, "seed", "algo", "input", now.AddMinutes(-10));

        IDrawRepository drawRepository = Substitute.For<IDrawRepository>();
        ITicketRepository ticketRepository = Substitute.For<ITicketRepository>();
        ITicketDrawRepository ticketDrawRepository = Substitute.For<ITicketDrawRepository>();
        ITicketLineResultRepository ticketLineResultRepository = Substitute.For<ITicketLineResultRepository>();
        IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
        IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
        ITenantContext tenantContext = Substitute.For<ITenantContext>();
        IEntitlementChecker entitlementChecker = Substitute.For<IEntitlementChecker>();

        drawRepository.GetByIdAsync(tenantId, draw.Id, Arg.Any<CancellationToken>()).Returns(draw);
        ticketDrawRepository.GetByDrawIdAsync(tenantId, draw.Id, TicketDrawParticipationStatus.Active, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<TicketDraw>());
        entitlementChecker.EnsureGameEnabledAsync(tenantId, draw.GameCode, Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        dateTimeProvider.UtcNow.Returns(now);
        tenantContext.TenantId.Returns(tenantId);

        SettleDrawCommandHandler handler = new(
            drawRepository,
            ticketRepository,
            ticketDrawRepository,
            ticketLineResultRepository,
            unitOfWork,
            dateTimeProvider,
            tenantContext,
            entitlementChecker);

        Result result = await handler.Handle(new SettleDrawCommand(draw.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        draw.SettledAtUtc.Should().Be(now);
        drawRepository.Received(1).Update(draw);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
