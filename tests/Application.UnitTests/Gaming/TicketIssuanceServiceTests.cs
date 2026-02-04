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
    public async Task IssueSingleAsync_Should_Create_Ticket()
    {
        Guid tenantId = Guid.NewGuid();
        Guid memberId = Guid.NewGuid();
        Guid drawId = Guid.NewGuid();
        DateTime now = DateTime.UtcNow;

        ITicketRepository ticketRepository = Substitute.For<ITicketRepository>();

        TicketIssuanceService service = new(ticketRepository);

        TicketIssuanceRequest request = new(
            tenantId,
            GameCodes.Lottery539,
            memberId,
            null,
            null,
            drawId,
            IssuedByType.Backoffice,
            Guid.NewGuid(),
            "reason",
            "note",
            now);

        Result<TicketIssuanceResult> result = await service.IssueSingleAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.PrimaryDrawId.Should().Be(drawId);
        ticketRepository.Received(1).Insert(Arg.Any<Ticket>());
    }

    [Fact]
    public async Task IssueBulkSameDrawAsync_Should_Create_Tickets_For_Quantity()
    {
        Guid tenantId = Guid.NewGuid();
        Guid memberId = Guid.NewGuid();
        Guid drawId = Guid.NewGuid();
        DateTime now = DateTime.UtcNow;

        ITicketRepository ticketRepository = Substitute.For<ITicketRepository>();

        TicketIssuanceService service = new(ticketRepository);

        TicketIssuanceRequest request = new(
            tenantId,
            GameCodes.Lottery539,
            memberId,
            null,
            null,
            drawId,
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
    }
}
