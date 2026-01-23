using Domain.Gaming.Draws;
using Domain.Gaming.Shared;
using SharedKernel;

namespace Domain.Gaming.Tickets;

/// <summary>
/// 票券提交規則，集中封盤與狀態檢查。
/// </summary>
public static class TicketSubmissionPolicy
{
    public static Result EnsureCanSubmit(Ticket ticket, Draw draw, DateTime utcNow)
    {
        if (ticket.SubmissionStatus == TicketSubmissionStatus.Cancelled)
        {
            return Result.Failure(GamingErrors.TicketCancelled);
        }

        if (ticket.SubmissionStatus != TicketSubmissionStatus.NotSubmitted)
        {
            return Result.Failure(GamingErrors.TicketAlreadySubmittedConflict);
        }

        if (draw.IsEffectivelyClosed(utcNow))
        {
            return Result.Failure(GamingErrors.TicketSubmissionClosed);
        }

        return Result.Success();
    }
}
