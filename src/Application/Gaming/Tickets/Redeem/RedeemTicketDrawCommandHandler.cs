using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Time;
using Domain.Gaming.Repositories;
using Domain.Gaming.Shared;
using Domain.Gaming.Tickets;
using SharedKernel;

namespace Application.Gaming.Tickets.Redeem;

internal sealed class RedeemTicketDrawCommandHandler(
    ITicketRepository ticketRepository,
    ITicketDrawRepository ticketDrawRepository,
    TicketRedeemableDrawService redeemableDrawService,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ITenantContext tenantContext) : ICommandHandler<RedeemTicketDrawCommand>
{
    public async Task<Result> Handle(RedeemTicketDrawCommand request, CancellationToken cancellationToken)
    {
        Ticket? ticket = await ticketRepository.GetByIdAsync(tenantContext.TenantId, request.TicketId, cancellationToken);
        if (ticket is null)
        {
            return Result.Failure(GamingErrors.TicketNotFound);
        }

        Result<IReadOnlyCollection<Guid>> redeemableResult = await redeemableDrawService.GetRedeemableDrawIdsAsync(
            tenantContext.TenantId,
            ticket,
            dateTimeProvider.UtcNow,
            cancellationToken);
        if (redeemableResult.IsFailure)
        {
            return Result.Failure(redeemableResult.Error);
        }

        if (!redeemableResult.Value.Contains(request.DrawId))
        {
            return Result.Failure(GamingErrors.TicketDrawNotAvailable);
        }

        IReadOnlyCollection<TicketDraw> ticketDraws = await ticketDrawRepository.GetByTicketIdAsync(
            tenantContext.TenantId,
            ticket.Id,
            cancellationToken);

        TicketDraw? ticketDraw = ticketDraws.FirstOrDefault(item => item.DrawId == request.DrawId);
        if (ticketDraw is null)
        {
            return Result.Failure(GamingErrors.TicketDrawNotFound);
        }

        if (ticketDraw.ParticipationStatus == TicketDrawParticipationStatus.Redeemed)
        {
            return Result.Failure(GamingErrors.TicketDrawAlreadyRedeemed);
        }

        if (ticketDraw.ParticipationStatus != TicketDrawParticipationStatus.Settled)
        {
            return Result.Failure(GamingErrors.TicketDrawNotSettled);
        }

        ticketDraw.MarkRedeemed(dateTimeProvider.UtcNow);
        ticketDrawRepository.Update(ticketDraw);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
