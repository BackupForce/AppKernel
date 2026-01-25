using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Gaming;
using Application.Abstractions.Messaging;
using Application.Abstractions.Time;
using Domain.Gaming.Catalog;
using Domain.Gaming.Draws;
using Domain.Gaming.Repositories;
using Domain.Gaming.Rules;
using Domain.Gaming.Shared;
using Domain.Gaming.Tickets;
using SharedKernel;

namespace Application.Gaming.Tickets.Submit;

internal sealed class SubmitTicketNumbersCommandHandler(
    IDrawRepository drawRepository,
    ITicketRepository ticketRepository,
    ITicketDrawRepository ticketDrawRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ITenantContext tenantContext,
    IUserContext userContext,
    IEntitlementChecker entitlementChecker) : ICommandHandler<SubmitTicketNumbersCommand>
{
    public async Task<Result> Handle(SubmitTicketNumbersCommand request, CancellationToken cancellationToken)
    {
        Ticket? ticket = await ticketRepository.GetByIdAsync(tenantContext.TenantId, request.TicketId, cancellationToken);
        if (ticket is null)
        {
            return Result.Failure(GamingErrors.TicketNotFound);
        }

        if (ticket.SubmissionStatus == TicketSubmissionStatus.Cancelled)
        {
            return Result.Failure(GamingErrors.TicketCancelled);
        }

        if (ticket.SubmissionStatus != TicketSubmissionStatus.NotSubmitted)
        {
            return Result.Failure(GamingErrors.TicketAlreadySubmitted);
        }

        Result<PlayTypeCode> playTypeResult = PlayTypeCode.Create(request.PlayTypeCode);
        if (playTypeResult.IsFailure)
        {
            return Result.Failure(playTypeResult.Error);
        }

        PlayTypeCode playTypeCode = playTypeResult.Value;

        Result<LotteryNumbers> numbersResult = LotteryNumbers.Create(request.Numbers);
        if (numbersResult.IsFailure)
        {
            return Result.Failure(numbersResult.Error);
        }

        Guid? drawId = ticket.DrawId;
        if (!drawId.HasValue)
        {
            return Result.Failure(GamingErrors.DrawNotFound);
        }

        Draw? primaryDraw = await drawRepository.GetByIdAsync(tenantContext.TenantId, drawId.Value, cancellationToken);
        if (primaryDraw is null)
        {
            return Result.Failure(GamingErrors.DrawNotFound);
        }

        PlayRuleRegistry registry = PlayRuleRegistry.CreateDefault();
        if (!registry.GetAllowedPlayTypes(primaryDraw.GameCode).Contains(playTypeCode))
        {
            return Result.Failure(GamingErrors.PlayTypeNotAllowed);
        }

        if (!primaryDraw.EnabledPlayTypes.Contains(playTypeCode))
        {
            return Result.Failure(GamingErrors.TicketPlayTypeNotEnabled);
        }

        Result entitlementResult = await entitlementChecker.EnsurePlayEnabledAsync(
            tenantContext.TenantId,
            ticket.GameCode,
            playTypeCode,
            cancellationToken);
        if (entitlementResult.IsFailure)
        {
            return Result.Failure(entitlementResult.Error);
        }

        IPlayRule rule = registry.GetRule(ticket.GameCode, playTypeCode);
        Result validationResult = rule.ValidateBet(numbersResult.Value);
        if (validationResult.IsFailure)
        {
            return Result.Failure(validationResult.Error);
        }

        DateTime now = dateTimeProvider.UtcNow;
        Result policyResult = TicketSubmissionPolicy.EnsureCanSubmit(ticket, primaryDraw, now);
        if (policyResult.IsFailure)
        {
            return Result.Failure(policyResult.Error);
        }

        Result submitResult = ticket.SubmitNumbers(playTypeCode, numbersResult.Value, now, userContext.UserId, null, null);
        if (submitResult.IsFailure)
        {
            return Result.Failure(submitResult.Error);
        }

        TicketLine line = ticket.Lines.Single();

        IReadOnlyCollection<TicketDraw> ticketDraws = await ticketDrawRepository.GetByTicketIdAsync(
            tenantContext.TenantId,
            ticket.Id,
            cancellationToken);

        foreach (TicketDraw ticketDraw in ticketDraws)
        {
            Draw? draw = await drawRepository.GetByIdAsync(tenantContext.TenantId, ticketDraw.DrawId, cancellationToken);
            if (draw is null)
            {
                ticketDraw.MarkInvalid(now);
                continue;
            }

            if (draw.IsWithinSalesWindow(now))
            {
                ticketDraw.MarkActive(now);
            }
            else
            {
                ticketDraw.MarkInvalid(now);
            }
        }

        ticketRepository.Update(ticket);
        ticketRepository.InsertLine(line);
        ticketDrawRepository.UpdateRange(ticketDraws);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
