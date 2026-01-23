using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Gaming;
using Application.Abstractions.Time;
using Application.Gaming.Tickets.Submit;
using Domain.Gaming.Catalog;
using Domain.Gaming.Draws;
using Domain.Gaming.Repositories;
using Domain.Gaming.Rules;
using Domain.Gaming.Tickets;
using FluentAssertions;
using NSubstitute;
using SharedKernel;

namespace Application.UnitTests.Gaming;

public sealed class SubmitTicketNumbersCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Evaluate_TicketDraws_By_Sales_Window()
    {
        Guid tenantId = Guid.NewGuid();
        DateTime now = DateTime.UtcNow;

        Draw openDraw = CreateDraw(tenantId, now.AddMinutes(-5), now.AddMinutes(5));
        Draw closedDrawEntity = CreateDraw(tenantId, now.AddMinutes(-10), now.AddMinutes(-1));

        Ticket ticket = Ticket.Create(
            tenantId,
            GameCodes.Lottery539,
            PlayTypeCodes.Basic,
            Guid.NewGuid(),
            null,
            null,
            openDraw.Id,
            null,
            null,
            now,
            IssuedByType.Campaign,
            null,
            null,
            null,
            now);

        TicketDraw activeDraw = TicketDraw.Create(tenantId, ticket.Id, openDraw.Id, now);
        TicketDraw closedDraw = TicketDraw.Create(tenantId, ticket.Id, closedDrawEntity.Id, now);

        IDrawRepository drawRepository = Substitute.For<IDrawRepository>();
        ITicketRepository ticketRepository = Substitute.For<ITicketRepository>();
        ITicketDrawRepository ticketDrawRepository = Substitute.For<ITicketDrawRepository>();
        IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
        IDateTimeProvider dateTimeProvider = Substitute.For<IDateTimeProvider>();
        ITenantContext tenantContext = Substitute.For<ITenantContext>();
        IUserContext userContext = Substitute.For<IUserContext>();
        IEntitlementChecker entitlementChecker = Substitute.For<IEntitlementChecker>();

        ticketRepository.GetByIdAsync(tenantId, ticket.Id, Arg.Any<CancellationToken>()).Returns(ticket);
        ticketDrawRepository.GetByTicketIdAsync(tenantId, ticket.Id, Arg.Any<CancellationToken>())
            .Returns(new[] { activeDraw, closedDraw });
        drawRepository.GetByIdAsync(tenantId, activeDraw.DrawId, Arg.Any<CancellationToken>()).Returns(openDraw);
        drawRepository.GetByIdAsync(tenantId, closedDraw.DrawId, Arg.Any<CancellationToken>()).Returns(closedDrawEntity);
        entitlementChecker.EnsurePlayEnabledAsync(tenantId, ticket.GameCode, ticket.PlayTypeCode, Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        dateTimeProvider.UtcNow.Returns(now);
        tenantContext.TenantId.Returns(tenantId);
        userContext.UserId.Returns(Guid.NewGuid());

        SubmitTicketNumbersCommandHandler handler = new(
            drawRepository,
            ticketRepository,
            ticketDrawRepository,
            unitOfWork,
            dateTimeProvider,
            tenantContext,
            userContext,
            entitlementChecker);

        Result result = await handler.Handle(
            new SubmitTicketNumbersCommand(ticket.Id, new[] { 1, 2, 3, 4, 5 }),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        activeDraw.ParticipationStatus.Should().Be(TicketDrawParticipationStatus.Active);
        closedDraw.ParticipationStatus.Should().Be(TicketDrawParticipationStatus.Invalid);
        ticket.SubmissionStatus.Should().Be(TicketSubmissionStatus.Submitted);
    }

    private static Draw CreateDraw(Guid tenantId, DateTime openAt, DateTime closeAt)
    {
        Draw draw = Draw.Create(
            tenantId,
            GameCodes.Lottery539,
            openAt,
            closeAt,
            closeAt.AddHours(1),
            DrawStatus.SalesOpen,
            null,
            DateTime.UtcNow,
            PlayRuleRegistry.CreateDefault()).Value;

        draw.EnablePlayTypes(new[] { PlayTypeCodes.Basic }, PlayRuleRegistry.CreateDefault());
        return draw;
    }
}
