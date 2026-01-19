using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Gaming;
using Application.Abstractions.Messaging;
using Application.Abstractions.Time;
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

        Result entitlementResult = await entitlementChecker.EnsurePlayEnabledAsync(
            tenantContext.TenantId,
            ticket.GameCode,
            ticket.PlayTypeCode,
            cancellationToken);
        if (entitlementResult.IsFailure)
        {
            return Result.Failure(entitlementResult.Error);
        }

        Result<LotteryNumbers> numbersResult = LotteryNumbers.Create(request.Numbers);
        if (numbersResult.IsFailure)
        {
            return Result.Failure(numbersResult.Error);
        }

        PlayRuleRegistry registry = PlayRuleRegistry.CreateDefault();
        IPlayRule rule = registry.GetRule(ticket.GameCode, ticket.PlayTypeCode);
        Result validationResult = rule.ValidateBet(numbersResult.Value);
        if (validationResult.IsFailure)
        {
            return Result.Failure(validationResult.Error);
        }

        DateTime now = dateTimeProvider.UtcNow;
        Result submitResult = ticket.SubmitNumbers(numbersResult.Value, now);
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
