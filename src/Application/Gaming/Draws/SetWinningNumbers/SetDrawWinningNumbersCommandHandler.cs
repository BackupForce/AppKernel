using System.Text.Json;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Gaming;
using Application.Abstractions.Messaging;
using Application.Gaming.Draws.Settle;
using Domain.Admin.OperationLogs;
using Domain.Gaming.Draws;
using Domain.Gaming.Repositories;
using Domain.Gaming.Shared;
using Domain.Gaming.Tickets;
using MediatR;
using SharedKernel;

namespace Application.Gaming.Draws.SetWinningNumbers;

internal sealed class SetDrawWinningNumbersCommandHandler(
    IDrawRepository drawRepository,
    ITicketDrawRepository ticketDrawRepository,
    ITicketLineResultRepository ticketLineResultRepository,
    IAdminOperationLogRepository adminOperationLogRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ITenantContext tenantContext,
    IEntitlementChecker entitlementChecker,
    IUserContext userContext,
    ISender sender) : ICommandHandler<SetDrawWinningNumbersCommand>
{
    public async Task<Result> Handle(SetDrawWinningNumbersCommand request, CancellationToken cancellationToken)
    {
        Draw? draw = await drawRepository.GetByIdAsync(tenantContext.TenantId, request.DrawId, cancellationToken);
        if (draw is null)
        {
            return Result.Failure(GamingErrors.DrawNotFound);
        }

        Result entitlementResult = await entitlementChecker.EnsureGameEnabledAsync(
            tenantContext.TenantId,
            draw.GameCode,
            cancellationToken);
        if (entitlementResult.IsFailure)
        {
            return Result.Failure(entitlementResult.Error);
        }

        DateTime now = dateTimeProvider.UtcNow;
        if (!draw.IsEffectivelyClosed(now))
        {
            return Result.Failure(GamingErrors.DrawNotReadyToSetWinningNumbers);
        }

        bool hasWinningNumbers = !string.IsNullOrWhiteSpace(draw.WinningNumbersRaw) || draw.DrawnAt.HasValue;
        bool isSettled = draw.SettledAtUtc.HasValue;

        if ((hasWinningNumbers || isSettled) && !request.ForceRecalculate)
        {
            return Result.Failure(GamingErrors.DrawAlreadyExecuted);
        }

        if (isSettled && request.ForceRecalculate)
        {
            IReadOnlyCollection<TicketDraw> redeemed = await ticketDrawRepository.GetByDrawIdAsync(
                tenantContext.TenantId,
                draw.Id,
                TicketDrawParticipationStatus.Redeemed,
                cancellationToken);

            if (redeemed.Count > 0)
            {
                return Result.Failure(GamingErrors.TicketDrawAlreadyRedeemed);
            }

            IReadOnlyCollection<TicketDraw> settledTicketDraws = await ticketDrawRepository.GetByDrawIdAsync(
                tenantContext.TenantId,
                draw.Id,
                TicketDrawParticipationStatus.Settled,
                cancellationToken);

            foreach (TicketDraw ticketDraw in settledTicketDraws)
            {
                ticketDraw.ResetForRecalculation(now);
            }

            ticketDrawRepository.UpdateRange(settledTicketDraws);
            await ticketLineResultRepository.DeleteByDrawIdAsync(tenantContext.TenantId, draw.Id, cancellationToken);
        }

        Result<LotteryNumbers> numbersResult = ResolveWinningNumbers(request);
        if (numbersResult.IsFailure)
        {
            return Result.Failure(numbersResult.Error);
        }

        draw.SetWinningNumbers(numbersResult.Value, userContext.UserId, now, request.SourceNote, request.ForceRecalculate);
        drawRepository.Update(draw);

        if (draw.IsEffectivelyClosed(now))
        {
            IReadOnlyCollection<TicketDraw> pendingTicketDraws = await ticketDrawRepository.GetPendingForUnsubmittedTicketsAsync(
                tenantContext.TenantId,
                draw.Id,
                cancellationToken);

            foreach (TicketDraw ticketDraw in pendingTicketDraws)
            {
                ticketDraw.MarkInvalid(now);
            }

            ticketDrawRepository.UpdateRange(pendingTicketDraws);
        }


        string action = request.ForceRecalculate ? "RecalculateWinningNumbers" : "SetWinningNumbers";

        var metadata = new
        {
            winningNumbers = numbersResult.Value.ToStorageString(),
            forceRecalculate = request.ForceRecalculate
        };

        string metadataJson = JsonSerializer.Serialize(metadata);
        AdminOperationLog log = AdminOperationLog.Create(
            tenantContext.TenantId,
            "Draw",
            draw.Id,
            action,
            userContext.UserId,
            operatorType: null,
            request.SourceNote,
            metadataJson,
            now);

        await adminOperationLogRepository.AddAsync(log, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        Result settleResult = await sender.Send(new SettleDrawCommand(draw.Id), cancellationToken);
        if (settleResult.IsFailure)
        {
            return Result.Failure(settleResult.Error);
        }

        return Result.Success();
    }

    private static Result<LotteryNumbers> ResolveWinningNumbers(SetDrawWinningNumbersCommand request)
    {
        if (request.WinningNumbers is { Count: > 0 })
        {
            return LotteryNumbers.Create(request.WinningNumbers);
        }

        if (!string.IsNullOrWhiteSpace(request.WinningNumbersRaw))
        {
            return LotteryNumbers.Parse(request.WinningNumbersRaw);
        }

        return Result.Failure<LotteryNumbers>(GamingErrors.LotteryNumbersRequired);
    }
}
