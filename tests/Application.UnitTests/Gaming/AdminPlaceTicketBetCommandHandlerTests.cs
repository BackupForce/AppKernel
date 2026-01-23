using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Gaming;
using Application.Abstractions.Time;
using Application.Gaming.Tickets.Admin;
using Domain.Gaming.Catalog;
using Domain.Gaming.Draws;
using Domain.Gaming.Repositories;
using Domain.Gaming.Rules;
using Domain.Gaming.Shared;
using Domain.Gaming.Tickets;
using FluentAssertions;
using NSubstitute;
using SharedKernel;
using System.Data;

namespace Application.UnitTests.Gaming;

public sealed class AdminPlaceTicketBetCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Submit_Bet()
    {
        Guid tenantId = Guid.NewGuid();
        DateTime now = DateTime.UtcNow;

        Draw draw = CreateDraw(tenantId, now.AddMinutes(-5), now.AddMinutes(5));
        draw.EnablePlayTypes(new[] { PlayTypeCodes.Basic }, PlayRuleRegistry.CreateDefault());

        Ticket ticket = Ticket.Create(
            tenantId,
            draw.GameCode,
            PlayTypeCodes.Basic,
            Guid.NewGuid(),
            null,
            null,
            draw.Id,
            null,
            null,
            now,
            IssuedByType.Backoffice,
            Guid.NewGuid(),
            "support",
            null,
            now);

        IDrawRepository drawRepository = Substitute.For<IDrawRepository>();
        ITicketRepository ticketRepository = Substitute.For<ITicketRepository>();
        ITicketIdempotencyRepository ticketIdempotencyRepository = Substitute.For<ITicketIdempotencyRepository>();
        IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
        IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
        ITenantContext tenantContext = Substitute.For<ITenantContext>();
        IUserContext userContext = Substitute.For<IUserContext>();
        IEntitlementChecker entitlementChecker = Substitute.For<IEntitlementChecker>();
        IDbTransaction transaction = Substitute.For<IDbTransaction>();

        drawRepository.GetByIdAsync(tenantId, draw.Id, Arg.Any<CancellationToken>()).Returns(draw);
        ticketRepository.GetByIdAsync(tenantId, ticket.Id, Arg.Any<CancellationToken>()).Returns(ticket);
        ticketRepository.TryMarkSubmittedAsync(
            tenantId,
            ticket.Id,
            Arg.Any<DateTime>(),
            Arg.Any<Guid?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(true);
        entitlementChecker.EnsurePlayEnabledAsync(tenantId, ticket.GameCode, ticket.PlayTypeCode, Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        dateTimeProvider.UtcNow.Returns(now);
        tenantContext.TenantId.Returns(tenantId);
        userContext.UserId.Returns(Guid.NewGuid());
        unitOfWork.BeginTransactionAsync().Returns(transaction);

        PlaceTicketBetCommandHandler handler = new(
            drawRepository,
            ticketRepository,
            ticketIdempotencyRepository,
            unitOfWork,
            dateTimeProvider,
            tenantContext,
            userContext,
            entitlementChecker);

        Result<PlaceTicketBetResult> result = await handler.Handle(
            new PlaceTicketBetCommand(ticket.Id, new[] { 1, 2, 3, 4, 5 }, "ref", "note", null),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Submitted");
        ticketRepository.Received(1).InsertLine(Arg.Any<TicketLine>());
        transaction.Received(1).Commit();
    }

    [Fact]
    public async Task Handle_Should_ReturnConflict_WhenAlreadySubmitted()
    {
        Guid tenantId = Guid.NewGuid();
        DateTime now = DateTime.UtcNow;

        Draw draw = CreateDraw(tenantId, now.AddMinutes(-5), now.AddMinutes(5));
        draw.EnablePlayTypes(new[] { PlayTypeCodes.Basic }, PlayRuleRegistry.CreateDefault());

        Ticket ticket = Ticket.Create(
            tenantId,
            draw.GameCode,
            PlayTypeCodes.Basic,
            Guid.NewGuid(),
            null,
            null,
            draw.Id,
            null,
            null,
            now,
            IssuedByType.Backoffice,
            Guid.NewGuid(),
            "support",
            null,
            now);

        ticket.SubmitNumbers(LotteryNumbers.Create(new[] { 1, 2, 3, 4, 5 }).Value, now, Guid.NewGuid(), null, null);

        IDrawRepository drawRepository = Substitute.For<IDrawRepository>();
        ITicketRepository ticketRepository = Substitute.For<ITicketRepository>();
        ITicketIdempotencyRepository ticketIdempotencyRepository = Substitute.For<ITicketIdempotencyRepository>();
        IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
        IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
        ITenantContext tenantContext = Substitute.For<ITenantContext>();
        IUserContext userContext = Substitute.For<IUserContext>();
        IEntitlementChecker entitlementChecker = Substitute.For<IEntitlementChecker>();

        drawRepository.GetByIdAsync(tenantId, draw.Id, Arg.Any<CancellationToken>()).Returns(draw);
        ticketRepository.GetByIdAsync(tenantId, ticket.Id, Arg.Any<CancellationToken>()).Returns(ticket);
        entitlementChecker.EnsurePlayEnabledAsync(tenantId, ticket.GameCode, ticket.PlayTypeCode, Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        dateTimeProvider.UtcNow.Returns(now);
        tenantContext.TenantId.Returns(tenantId);
        userContext.UserId.Returns(Guid.NewGuid());

        PlaceTicketBetCommandHandler handler = new(
            drawRepository,
            ticketRepository,
            ticketIdempotencyRepository,
            unitOfWork,
            dateTimeProvider,
            tenantContext,
            userContext,
            entitlementChecker);

        Result<PlaceTicketBetResult> result = await handler.Handle(
            new PlaceTicketBetCommand(ticket.Id, new[] { 1, 2, 3, 4, 5 }, null, null, null),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public async Task Handle_Should_ReturnConflict_WhenSalesClosed()
    {
        Guid tenantId = Guid.NewGuid();
        DateTime now = DateTime.UtcNow;

        Draw draw = CreateDraw(tenantId, now.AddMinutes(-10), now.AddMinutes(-1));
        draw.EnablePlayTypes(new[] { PlayTypeCodes.Basic }, PlayRuleRegistry.CreateDefault());

        Ticket ticket = Ticket.Create(
            tenantId,
            draw.GameCode,
            PlayTypeCodes.Basic,
            Guid.NewGuid(),
            null,
            null,
            draw.Id,
            null,
            null,
            now,
            IssuedByType.Backoffice,
            Guid.NewGuid(),
            "support",
            null,
            now);

        IDrawRepository drawRepository = Substitute.For<IDrawRepository>();
        ITicketRepository ticketRepository = Substitute.For<ITicketRepository>();
        ITicketIdempotencyRepository ticketIdempotencyRepository = Substitute.For<ITicketIdempotencyRepository>();
        IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
        IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
        ITenantContext tenantContext = Substitute.For<ITenantContext>();
        IUserContext userContext = Substitute.For<IUserContext>();
        IEntitlementChecker entitlementChecker = Substitute.For<IEntitlementChecker>();

        drawRepository.GetByIdAsync(tenantId, draw.Id, Arg.Any<CancellationToken>()).Returns(draw);
        ticketRepository.GetByIdAsync(tenantId, ticket.Id, Arg.Any<CancellationToken>()).Returns(ticket);
        entitlementChecker.EnsurePlayEnabledAsync(tenantId, ticket.GameCode, ticket.PlayTypeCode, Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        dateTimeProvider.UtcNow.Returns(now);
        tenantContext.TenantId.Returns(tenantId);
        userContext.UserId.Returns(Guid.NewGuid());

        PlaceTicketBetCommandHandler handler = new(
            drawRepository,
            ticketRepository,
            ticketIdempotencyRepository,
            unitOfWork,
            dateTimeProvider,
            tenantContext,
            userContext,
            entitlementChecker);

        Result<PlaceTicketBetResult> result = await handler.Handle(
            new PlaceTicketBetCommand(ticket.Id, new[] { 1, 2, 3, 4, 5 }, null, null, null),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(GamingErrors.TicketSubmissionClosed);
    }

    [Fact]
    public async Task Handle_Should_ReturnValidation_WhenNumbersInvalid()
    {
        Guid tenantId = Guid.NewGuid();
        DateTime now = DateTime.UtcNow;

        Draw draw = CreateDraw(tenantId, now.AddMinutes(-5), now.AddMinutes(5));
        draw.EnablePlayTypes(new[] { PlayTypeCodes.Basic }, PlayRuleRegistry.CreateDefault());

        Ticket ticket = Ticket.Create(
            tenantId,
            draw.GameCode,
            PlayTypeCodes.Basic,
            Guid.NewGuid(),
            null,
            null,
            draw.Id,
            null,
            null,
            now,
            IssuedByType.Backoffice,
            Guid.NewGuid(),
            "support",
            null,
            now);

        IDrawRepository drawRepository = Substitute.For<IDrawRepository>();
        ITicketRepository ticketRepository = Substitute.For<ITicketRepository>();
        ITicketIdempotencyRepository ticketIdempotencyRepository = Substitute.For<ITicketIdempotencyRepository>();
        IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
        IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
        ITenantContext tenantContext = Substitute.For<ITenantContext>();
        IUserContext userContext = Substitute.For<IUserContext>();
        IEntitlementChecker entitlementChecker = Substitute.For<IEntitlementChecker>();

        drawRepository.GetByIdAsync(tenantId, draw.Id, Arg.Any<CancellationToken>()).Returns(draw);
        ticketRepository.GetByIdAsync(tenantId, ticket.Id, Arg.Any<CancellationToken>()).Returns(ticket);
        entitlementChecker.EnsurePlayEnabledAsync(tenantId, ticket.GameCode, ticket.PlayTypeCode, Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        dateTimeProvider.UtcNow.Returns(now);
        tenantContext.TenantId.Returns(tenantId);
        userContext.UserId.Returns(Guid.NewGuid());

        PlaceTicketBetCommandHandler handler = new(
            drawRepository,
            ticketRepository,
            ticketIdempotencyRepository,
            unitOfWork,
            dateTimeProvider,
            tenantContext,
            userContext,
            entitlementChecker);

        Result<PlaceTicketBetResult> result = await handler.Handle(
            new PlaceTicketBetCommand(ticket.Id, new[] { 1, 2 }, null, null, null),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(GamingErrors.LotteryNumbersCountInvalid);
    }

    private static Draw CreateDraw(Guid tenantId, DateTime openAt, DateTime closeAt)
    {
        return Draw.Create(
            tenantId,
            GameCodes.Lottery539,
            openAt,
            closeAt,
            closeAt.AddHours(1),
            DrawStatus.SalesOpen,
            null,
            DateTime.UtcNow,
            PlayRuleRegistry.CreateDefault()).Value;
    }
}
