using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Time;
using Domain.Gaming.Repositories;
using Domain.Gaming.Shared;
using Domain.Gaming.Tickets;
using SharedKernel;

namespace Application.Gaming.Tickets.Redeem;

internal sealed class RedeemTicketLineCommandHandler(
    ITicketLineResultRepository ticketLineResultRepository,
    ITicketDrawRepository ticketDrawRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ITenantContext tenantContext,
    IUserContext userContext) : ICommandHandler<RedeemTicketLineCommand, Guid>
{
    public async Task<Result<Guid>> Handle(RedeemTicketLineCommand request, CancellationToken cancellationToken)
    {
        TicketLineResult? ticketLineResult = await ticketLineResultRepository.GetByIdAsync(
            tenantContext.TenantId,
            request.TicketLineResultId,
            cancellationToken);

        if (ticketLineResult is null)
        {
            return Result.Failure<Guid>(GamingErrors.TicketLineResultNotFound);
        }

        IReadOnlyCollection<TicketDraw> ticketDraws = await ticketDrawRepository.GetByTicketIdAsync(
            tenantContext.TenantId,
            ticketLineResult.TicketId,
            cancellationToken);

        TicketDraw? ticketDraw = ticketDraws.FirstOrDefault(item => item.DrawId == ticketLineResult.DrawId);
        if (ticketDraw is null)
        {
            return Result.Failure<Guid>(GamingErrors.TicketDrawNotFound);
        }

        DateTime redeemedAtUtc = dateTimeProvider.UtcNow;

        try
        {
            ticketDraw.MarkRedeemed(redeemedAtUtc);
            ticketLineResult.Redeem(userContext.UserId, redeemedAtUtc);
        }
        catch (InvalidOperationException exception) when (exception.Message == "TicketDraw.MarkRedeemed.NotRedeemable")
        {
            return Result.Failure<Guid>(GamingErrors.TicketLineResultNotRedeemable);
        }
        catch (InvalidOperationException exception) when (
            exception.Message == "TicketDraw.MarkRedeemed.AlreadyRedeemed"
            || exception.Message == "TicketLineResult.Redeem.AlreadyRedeemed")
        {
            return Result.Failure<Guid>(GamingErrors.TicketLineResultAlreadyRedeemed);
        }

        ticketDrawRepository.Update(ticketDraw);
        ticketLineResultRepository.Update(ticketLineResult);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(ticketLineResult.Id);
    }
}
