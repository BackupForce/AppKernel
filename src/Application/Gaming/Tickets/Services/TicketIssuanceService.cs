using Domain.Gaming.Repositories;
using Domain.Gaming.Shared;
using Domain.Gaming.Tickets;
using SharedKernel;

namespace Application.Gaming.Tickets.Services;

internal sealed class TicketIssuanceService(
    ITicketRepository ticketRepository)
{
    public Task<Result<TicketIssuanceResult>> IssueSingleAsync(
        TicketIssuanceRequest request)
    {
        if (request.PrimaryDrawId == Guid.Empty)
        {
            return Task.FromResult(Result.Failure<TicketIssuanceResult>(GamingErrors.TicketDrawNotAvailable));
        }

        Ticket ticket = Ticket.Create(
            request.TenantId,
            request.GameCode,
            request.MemberId,
            request.DrawGroupId,
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

        TicketIssuanceResult result = new(ticket, request.PrimaryDrawId);
        return Task.FromResult<Result<TicketIssuanceResult>>(result);
    }

    public Task<Result<IReadOnlyCollection<Ticket>>> IssueBulkSameDrawAsync(
        TicketIssuanceRequest request,
        int quantity)
    {
        if (request.PrimaryDrawId == Guid.Empty)
        {
            return Task.FromResult(Result.Failure<IReadOnlyCollection<Ticket>>(GamingErrors.TicketDrawNotAvailable));
        }

        List<Ticket> tickets = new();

        for (int index = 0; index < quantity; index++)
        {
            Ticket ticket = Ticket.Create(
                request.TenantId,
                request.GameCode,
                request.MemberId,
                request.DrawGroupId,
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
        }

        foreach (Ticket ticket in tickets)
        {
            ticketRepository.Insert(ticket);
        }

        return Task.FromResult<Result<IReadOnlyCollection<Ticket>>>(tickets);
    }
}

internal sealed record TicketIssuanceResult(Ticket Ticket, Guid PrimaryDrawId);
