using Domain.Gaming.Tickets;
using SharedKernel;

namespace Application.Gaming.Tickets.Submission;

internal interface ITicketBetSubmissionService
{
    Task<Result<TicketBetSubmissionResult>> SubmitAsync(
        Guid tenantId,
        Guid ticketId,
        string playTypeCode,
        int[] numbers,
        Guid submittedByUserId,
        DateTime nowUtc,
        string? clientReference,
        string? note,
        CancellationToken cancellationToken);
}
