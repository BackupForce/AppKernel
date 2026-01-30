using Domain.Gaming.Repositories;
using Domain.Gaming.Shared;
using Domain.Gaming.Tickets;
using SharedKernel;

namespace Application.Gaming.Tickets.Services;

internal sealed class TicketIssuanceService(
    ITicketRepository ticketRepository,
    ITicketDrawRepository ticketDrawRepository)
{
    public Task<Result<TicketIssuanceResult>> IssueSingleAsync(
        TicketIssuanceRequest request,
        CancellationToken cancellationToken)
    {
        if (request.DrawIds.Count == 0)
        {
            return Task.FromResult(Result.Failure<TicketIssuanceResult>(GamingErrors.TicketDrawNotAvailable));
        }

        Ticket ticket = Ticket.Create(
            request.TenantId,
            request.GameCode,
            request.MemberId,
            request.CampaignId,
            request.TicketTemplateId,
            request.PrimaryDrawId,
            null,
            null,
            request.NowUtc,
            request.IssuedByType,
            request.IssuedByUserId,
            request.IssuedReason,
            request.IssuedNote,
            request.NowUtc);

        ticketRepository.Insert(ticket);

        List<Guid> drawIds = request.DrawIds.ToList();
        foreach (Guid drawId in drawIds)
        {
            TicketDraw ticketDraw = TicketDraw.Create(request.TenantId, ticket.Id, drawId, request.NowUtc);
            ticketDrawRepository.Insert(ticketDraw);
        }

        TicketIssuanceResult result = new(ticket, drawIds);
        return Task.FromResult<Result<TicketIssuanceResult>>(result);
    }

    public Task<Result<IReadOnlyCollection<Ticket>>> IssueBulkSameDrawAsync(
        TicketIssuanceRequest request,
        int quantity,
        CancellationToken cancellationToken)
    {
        if (request.DrawIds.Count == 0)
        {
            return Task.FromResult(Result.Failure<IReadOnlyCollection<Ticket>>(GamingErrors.TicketDrawNotAvailable));
        }

        Guid drawId = request.DrawIds.First();
        List<Ticket> tickets = new();
        List<TicketDraw> ticketDraws = new();

        for (int index = 0; index < quantity; index++)
        {
            Ticket ticket = Ticket.Create(
                request.TenantId,
                request.GameCode,
                request.MemberId,
                request.CampaignId,
                request.TicketTemplateId,
                request.PrimaryDrawId,
                null,
                null,
                request.NowUtc,
                request.IssuedByType,
                request.IssuedByUserId,
                request.IssuedReason,
                request.IssuedNote,
                request.NowUtc);

            tickets.Add(ticket);
            ticketDraws.Add(TicketDraw.Create(request.TenantId, ticket.Id, drawId, request.NowUtc));
        }

        foreach (Ticket ticket in tickets)
        {
            ticketRepository.Insert(ticket);
        }

        foreach (TicketDraw ticketDraw in ticketDraws)
        {
            ticketDrawRepository.Insert(ticketDraw);
        }

        return Task.FromResult<Result<IReadOnlyCollection<Ticket>>>(tickets);
    }
}

internal sealed record TicketIssuanceResult(Ticket Ticket, IReadOnlyCollection<Guid> DrawIds);
