using System.Data;
using Application.Abstractions.Data;
using Application.Abstractions.Gaming;
using Domain.Gaming.Catalog;
using Domain.Gaming.Draws;
using Domain.Gaming.Repositories;
using Domain.Gaming.Rules;
using Domain.Gaming.Shared;
using Domain.Gaming.Tickets;
using SharedKernel;

namespace Application.Gaming.Tickets.Submission;

internal sealed class TicketBetSubmissionService(
    IDrawRepository drawRepository,
    ITicketRepository ticketRepository,
    ITicketDrawRepository ticketDrawRepository,
    IDrawAllowedTicketTemplateRepository drawAllowedTicketTemplateRepository,
    IUnitOfWork unitOfWork,
    IEntitlementChecker entitlementChecker) : ITicketBetSubmissionService
{
    public async Task<Result<TicketBetSubmissionResult>> SubmitAsync(
        Guid tenantId,
        Guid ticketId,
        string playTypeCode,
        int[] numbers,
        Guid submittedByUserId,
        DateTime nowUtc,
        string? clientReference,
        string? note,
        CancellationToken cancellationToken)
    {
        Ticket? ticket = await ticketRepository.GetByIdAsync(tenantId, ticketId, cancellationToken);
        if (ticket is null)
        {
            return Result.Failure<TicketBetSubmissionResult>(GamingErrors.TicketNotFound);
        }

        if (ticket.SubmissionStatus == TicketSubmissionStatus.Cancelled)
        {
            return Result.Failure<TicketBetSubmissionResult>(GamingErrors.TicketCancelled);
        }

        Result<PlayTypeCode> playTypeResult = PlayTypeCode.Create(playTypeCode);
        if (playTypeResult.IsFailure)
        {
            return Result.Failure<TicketBetSubmissionResult>(playTypeResult.Error);
        }

        Result<LotteryNumbers> numbersResult = LotteryNumbers.Create(numbers);
        if (numbersResult.IsFailure)
        {
            return Result.Failure<TicketBetSubmissionResult>(numbersResult.Error);
        }

        Guid? drawId = ticket.DrawId;
        if (!drawId.HasValue)
        {
            return Result.Failure<TicketBetSubmissionResult>(GamingErrors.DrawNotFound);
        }

        Draw? draw = await drawRepository.GetByIdAsync(tenantId, drawId.Value, cancellationToken);
        if (draw is null)
        {
            return Result.Failure<TicketBetSubmissionResult>(GamingErrors.DrawNotFound);
        }

        if (ticket.TicketTemplateId.HasValue)
        {
            IReadOnlyCollection<DrawAllowedTicketTemplate> allowedTemplates =
                await drawAllowedTicketTemplateRepository.GetByDrawIdAsync(tenantId, draw.Id, cancellationToken);

            if (allowedTemplates.Count == 0)
            {
                return Result.Failure<TicketBetSubmissionResult>(GamingErrors.TicketTemplateNotAllowed);
            }

            bool isAllowed = allowedTemplates.Any(item => item.TicketTemplateId == ticket.TicketTemplateId.Value);
            if (!isAllowed)
            {
                return Result.Failure<TicketBetSubmissionResult>(GamingErrors.TicketTemplateNotAllowed);
            }
        }

        PlayRuleRegistry registry = PlayRuleRegistry.CreateDefault();
        if (!registry.GetAllowedPlayTypes(draw.GameCode).Contains(playTypeResult.Value))
        {
            return Result.Failure<TicketBetSubmissionResult>(GamingErrors.PlayTypeNotAllowed);
        }

        if (!draw.EnabledPlayTypes.Contains(playTypeResult.Value))
        {
            return Result.Failure<TicketBetSubmissionResult>(GamingErrors.TicketPlayTypeNotEnabled);
        }

        Result entitlementResult = await entitlementChecker.EnsurePlayEnabledAsync(
            tenantId,
            ticket.GameCode,
            playTypeResult.Value,
            cancellationToken);
        if (entitlementResult.IsFailure)
        {
            return Result.Failure<TicketBetSubmissionResult>(entitlementResult.Error);
        }

        IPlayRule rule = registry.GetRule(ticket.GameCode, playTypeResult.Value);
        Result validationResult = rule.ValidateBet(numbersResult.Value);
        if (validationResult.IsFailure)
        {
            return Result.Failure<TicketBetSubmissionResult>(validationResult.Error);
        }

        Result policyResult = TicketSubmissionPolicy.EnsureCanSubmit(ticket, draw, nowUtc);
        if (policyResult.IsFailure)
        {
            return Result.Failure<TicketBetSubmissionResult>(policyResult.Error);
        }

        Result submitResult = ticket.SubmitNumbers(
            playTypeResult.Value,
            numbersResult.Value,
            nowUtc,
            submittedByUserId,
            clientReference,
            note);
        if (submitResult.IsFailure)
        {
            return Result.Failure<TicketBetSubmissionResult>(submitResult.Error);
        }

        TicketLine line = ticket.Lines.Single();

        using IDbTransaction transaction = await unitOfWork.BeginTransactionAsync();

        bool updated = await ticketRepository.TryMarkSubmittedAsync(
            tenantId,
            ticket.Id,
            ticket.SubmittedAtUtc ?? nowUtc,
            ticket.SubmittedByUserId,
            ticket.SubmittedClientReference,
            ticket.SubmittedNote,
            cancellationToken);

        if (!updated)
        {
            return Result.Failure<TicketBetSubmissionResult>(GamingErrors.TicketAlreadySubmittedConflict);
        }

        ticketRepository.InsertLine(line);

        IReadOnlyCollection<TicketDraw> ticketDraws = await ticketDrawRepository.GetByTicketIdAsync(
            tenantId,
            ticket.Id,
            cancellationToken);

        foreach (TicketDraw ticketDraw in ticketDraws)
        {
            Draw? ticketDrawRef = await drawRepository.GetByIdAsync(tenantId, ticketDraw.DrawId, cancellationToken);
            if (ticketDrawRef is null)
            {
                ticketDraw.MarkInvalid(nowUtc);
                continue;
            }

            if (ticketDrawRef.IsWithinSalesWindow(nowUtc))
            {
                ticketDraw.MarkActive(nowUtc);
            }
            else
            {
                ticketDraw.MarkInvalid(nowUtc);
            }
        }

        ticketRepository.Update(ticket);
        ticketDrawRepository.UpdateRange(ticketDraws);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        transaction.Commit();

        return new TicketBetSubmissionResult(
            ticket.Id,
            ticket.SubmissionStatus,
            ticket.SubmittedAtUtc ?? nowUtc,
            ticket.SubmittedByUserId ?? Guid.Empty,
            playTypeResult.Value.Value,
            numbersResult.Value.Numbers,
            clientReference,
            note);
    }
}
