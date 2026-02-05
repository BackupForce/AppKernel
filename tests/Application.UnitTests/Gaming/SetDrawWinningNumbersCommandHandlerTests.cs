using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Gaming;
using Application.Gaming.Draws.SetWinningNumbers;
using Application.Gaming.Draws.Settle;
using Domain.Admin.OperationLogs;
using Domain.Gaming.Catalog;
using Domain.Gaming.Draws;
using Domain.Gaming.Repositories;
using Domain.Gaming.Rules;
using Domain.Gaming.Shared;
using Domain.Gaming.Tickets;
using FluentAssertions;
using MediatR;
using NSubstitute;
using SharedKernel;

namespace Application.UnitTests.Gaming;

public sealed class SetDrawWinningNumbersCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Set_WinningNumbers_And_Settle()
    {
        Guid tenantId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        DateTime now = DateTime.UtcNow;

        Draw draw = CreateClosedDraw(tenantId, now);

        IDrawRepository drawRepository = Substitute.For<IDrawRepository>();
        ITicketDrawRepository ticketDrawRepository = Substitute.For<ITicketDrawRepository>();
        ITicketLineResultRepository ticketLineResultRepository = Substitute.For<ITicketLineResultRepository>();
        IAdminOperationLogRepository adminOperationLogRepository = Substitute.For<IAdminOperationLogRepository>();
        IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
        IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
        ITenantContext tenantContext = Substitute.For<ITenantContext>();
        IEntitlementChecker entitlementChecker = Substitute.For<IEntitlementChecker>();
        IUserContext userContext = Substitute.For<IUserContext>();
        ISender sender = Substitute.For<ISender>();

        drawRepository.GetByIdAsync(tenantId, draw.Id, Arg.Any<CancellationToken>()).Returns(draw);
        entitlementChecker.EnsureGameEnabledAsync(tenantId, draw.GameCode, Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        dateTimeProvider.UtcNow.Returns(now);
        tenantContext.TenantId.Returns(tenantId);
        userContext.UserId.Returns(userId);
        sender.Send(Arg.Any<SettleDrawCommand>(), Arg.Any<CancellationToken>()).Returns(Result.Success());

        SetDrawWinningNumbersCommandHandler handler = new(
            drawRepository,
            ticketDrawRepository,
            ticketLineResultRepository,
            adminOperationLogRepository,
            unitOfWork,
            dateTimeProvider,
            tenantContext,
            entitlementChecker,
            userContext,
            sender);

        SetDrawWinningNumbersCommand command = new(
            draw.Id,
            null,
            new[] { 1, 2, 3, 4, 5 },
            false,
            "manual input");

        Result result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        draw.WinningNumbersRaw.Should().Be("1,2,3,4,5");
        await sender.Received(1).Send(Arg.Any<SettleDrawCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Reject_When_Draw_Not_Closed()
    {
        Guid tenantId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        DateTime now = DateTime.UtcNow;

        Draw draw = CreateOpenDraw(tenantId, now);

        IDrawRepository drawRepository = Substitute.For<IDrawRepository>();
        ITicketDrawRepository ticketDrawRepository = Substitute.For<ITicketDrawRepository>();
        ITicketLineResultRepository ticketLineResultRepository = Substitute.For<ITicketLineResultRepository>();
        IAdminOperationLogRepository adminOperationLogRepository = Substitute.For<IAdminOperationLogRepository>();
        IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
        IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
        ITenantContext tenantContext = Substitute.For<ITenantContext>();
        IEntitlementChecker entitlementChecker = Substitute.For<IEntitlementChecker>();
        IUserContext userContext = Substitute.For<IUserContext>();
        ISender sender = Substitute.For<ISender>();

        drawRepository.GetByIdAsync(tenantId, draw.Id, Arg.Any<CancellationToken>()).Returns(draw);
        entitlementChecker.EnsureGameEnabledAsync(tenantId, draw.GameCode, Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        dateTimeProvider.UtcNow.Returns(now);
        tenantContext.TenantId.Returns(tenantId);
        userContext.UserId.Returns(userId);

        SetDrawWinningNumbersCommandHandler handler = new(
            drawRepository,
            ticketDrawRepository,
            ticketLineResultRepository,
            adminOperationLogRepository,
            unitOfWork,
            dateTimeProvider,
            tenantContext,
            entitlementChecker,
            userContext,
            sender);

        SetDrawWinningNumbersCommand command = new(
            draw.Id,
            null,
            new[] { 1, 2, 3, 4, 5 },
            false,
            null);

        Result result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(GamingErrors.DrawNotReadyToSetWinningNumbers);
    }

    [Fact]
    public async Task Handle_Should_Reset_Settlement_When_Force_Recalculate()
    {
        Guid tenantId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        DateTime now = DateTime.UtcNow;

        Draw draw = CreateClosedDraw(tenantId, now);
        LotteryNumbers existingNumbers = LotteryNumbers.Create(new[] { 1, 2, 3, 4, 5 }).Value;
        draw.Execute(existingNumbers, "seed", "algo", "input", now.AddMinutes(-5));
        draw.MarkSettled(now.AddMinutes(-1));

        TicketDraw settled = TicketDraw.Create(tenantId, Guid.NewGuid(), draw.Id, now);
        settled.MarkSettled(now.AddMinutes(-1));

        IDrawRepository drawRepository = Substitute.For<IDrawRepository>();
        ITicketDrawRepository ticketDrawRepository = Substitute.For<ITicketDrawRepository>();
        ITicketLineResultRepository ticketLineResultRepository = Substitute.For<ITicketLineResultRepository>();
        IAdminOperationLogRepository adminOperationLogRepository = Substitute.For<IAdminOperationLogRepository>();
        IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
        IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
        ITenantContext tenantContext = Substitute.For<ITenantContext>();
        IEntitlementChecker entitlementChecker = Substitute.For<IEntitlementChecker>();
        IUserContext userContext = Substitute.For<IUserContext>();
        ISender sender = Substitute.For<ISender>();

        drawRepository.GetByIdAsync(tenantId, draw.Id, Arg.Any<CancellationToken>()).Returns(draw);
        entitlementChecker.EnsureGameEnabledAsync(tenantId, draw.GameCode, Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        dateTimeProvider.UtcNow.Returns(now);
        tenantContext.TenantId.Returns(tenantId);
        userContext.UserId.Returns(userId);
        ticketDrawRepository.GetByDrawIdAsync(tenantId, draw.Id, TicketDrawParticipationStatus.Redeemed, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<TicketDraw>());
        ticketDrawRepository.GetByDrawIdAsync(tenantId, draw.Id, TicketDrawParticipationStatus.Settled, Arg.Any<CancellationToken>())
            .Returns(new[] { settled });
        sender.Send(Arg.Any<SettleDrawCommand>(), Arg.Any<CancellationToken>()).Returns(Result.Success());

        SetDrawWinningNumbersCommandHandler handler = new(
            drawRepository,
            ticketDrawRepository,
            ticketLineResultRepository,
            adminOperationLogRepository,
            unitOfWork,
            dateTimeProvider,
            tenantContext,
            entitlementChecker,
            userContext,
            sender);

        SetDrawWinningNumbersCommand command = new(
            draw.Id,
            "6,7,8,9,10",
            null,
            true,
            "fix" );

        Result result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        settled.ParticipationStatus.Should().Be(TicketDrawParticipationStatus.Active);
        draw.SettledAtUtc.Should().BeNull();
        await ticketLineResultRepository.Received(1).DeleteByDrawIdAsync(tenantId, draw.Id, Arg.Any<CancellationToken>());
    }

    private static Draw CreateClosedDraw(Guid tenantId, DateTime now)
    {
        return Draw.Create(
            tenantId,
            GameCodes.Lottery539,
            "539-2401001",
            now.AddHours(-3),
            now.AddHours(-1),
            now.AddHours(1),
            null,
            now.AddHours(-4),
            PlayRuleRegistry.CreateDefault()).Value;
    }

    private static Draw CreateOpenDraw(Guid tenantId, DateTime now)
    {
        return Draw.Create(
            tenantId,
            GameCodes.Lottery539,
            "539-2401002",
            now.AddHours(-1),
            now.AddHours(1),
            now.AddHours(2),
            null,
            now.AddHours(-2),
            PlayRuleRegistry.CreateDefault()).Value;
    }
}
