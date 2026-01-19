using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Time;
using Domain.Gaming.Draws;
using Domain.Gaming.Repositories;
using Domain.Gaming.Shared;
using Domain.Gaming.Tickets;
using SharedKernel;

namespace Application.Gaming.Tickets.Cancel;

internal sealed class CancelTicketCommandHandler(
    IDrawRepository drawRepository,
    ITicketRepository ticketRepository,
    ITicketDrawRepository ticketDrawRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ITenantContext tenantContext,
    IUserContext userContext) : ICommandHandler<CancelTicketCommand>
{
    public async Task<Result> Handle(CancelTicketCommand request, CancellationToken cancellationToken)
    {
        Ticket? ticket = await ticketRepository.GetByIdAsync(tenantContext.TenantId, request.TicketId, cancellationToken);
        if (ticket is null)
        {
            return Result.Failure(GamingErrors.TicketNotFound);
        }

        IReadOnlyCollection<TicketDraw> ticketDraws = await ticketDrawRepository.GetByTicketIdAsync(
            tenantContext.TenantId,
            ticket.Id,
            cancellationToken);

        DateTime now = dateTimeProvider.UtcNow;

        foreach (TicketDraw ticketDraw in ticketDraws)
        {
            if (ticketDraw.ParticipationStatus == TicketDrawParticipationStatus.Settled
                || ticketDraw.ParticipationStatus == TicketDrawParticipationStatus.Redeemed)
            {
                return Result.Failure(GamingErrors.TicketCannotCancelAfterDraw);
            }

            Draw? draw = await drawRepository.GetByIdAsync(tenantContext.TenantId, ticketDraw.DrawId, cancellationToken);
            if (draw is not null && draw.DrawAt <= now)
            {
                return Result.Failure(GamingErrors.TicketCannotCancelAfterDraw);
            }
        }

        ticket.Cancel(userContext.UserId, request.Reason, now);
        ticketRepository.Update(ticket);

        foreach (TicketDraw ticketDraw in ticketDraws)
        {
            ticketDraw.Cancel(now);
        }

        ticketDrawRepository.UpdateRange(ticketDraws);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
