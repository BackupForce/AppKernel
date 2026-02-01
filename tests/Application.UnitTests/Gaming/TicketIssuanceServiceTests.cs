using Application.Gaming.Tickets.Services;
using Domain.Gaming.Catalog;
using Domain.Gaming.Repositories;
using Domain.Gaming.Shared;
using Domain.Gaming.Tickets;
using FluentAssertions;
using NSubstitute;
using SharedKernel;

namespace Application.UnitTests.Gaming;

public sealed class TicketIssuanceServiceTests
{
    [Fact]
    public async Task IssueSingleAsync_Should_Create_Ticket_And_TicketDraws()
    {
        Guid tenantId = Guid.NewGuid();
        Guid memberId = Guid.NewGuid();
        Guid drawId1 = Guid.NewGuid();
        Guid drawId2 = Guid.NewGuid();
        DateTime now = DateTime.UtcNow;

        ITicketRepository ticketRepository = Substitute.For<ITicketRepository>();
        ITicketDrawRepository ticketDrawRepository = Substitute.For<ITicketDrawRepository>();

        TicketIssuanceService service = new(ticketRepository, ticketDrawRepository);

        TicketIssuanceRequest request = new(
            tenantId,
            GameCodes.Lottery539,
            memberId,
            null,
            null,
            drawId1,
            new[] { drawId1, drawId2 },
            IssuedByType.Backoffice,
            Guid.NewGuid(),
            "reason",
            "note",
            now);

        Result<TicketIssuanceResult> result = await service.IssueSingleAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.DrawIds.Should().BeEquivalentTo(new[] { drawId1, drawId2 });
        ticketRepository.Received(1).Insert(Arg.Any<Ticket>());
        ticketDrawRepository.Received(2).Insert(Arg.Any<TicketDraw>());
    }

    [Fact]
    public async Task IssueBulkSameDrawAsync_Should_Create_Tickets_For_Quantity()
    {
        Guid tenantId = Guid.NewGuid();
        Guid memberId = Guid.NewGuid();
        Guid drawId = Guid.NewGuid();
        DateTime now = DateTime.UtcNow;

        ITicketRepository ticketRepository = Substitute.For<ITicketRepository>();
        ITicketDrawRepository ticketDrawRepository = Substitute.For<ITicketDrawRepository>();

        TicketIssuanceService service = new(ticketRepository, ticketDrawRepository);

        TicketIssuanceRequest request = new(
            tenantId,
            GameCodes.Lottery539,
            memberId,
            null,
            null,
            drawId,
            new[] { drawId },
            IssuedByType.Backoffice,
            Guid.NewGuid(),
            "reason",
            "note",
            now);

        Result<IReadOnlyCollection<Ticket>> result = await service.IssueBulkSameDrawAsync(
            request,
            3);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);
        ticketRepository.Received(3).Insert(Arg.Any<Ticket>());
        ticketDrawRepository.Received(3).Insert(Arg.Any<TicketDraw>());
    }
}
