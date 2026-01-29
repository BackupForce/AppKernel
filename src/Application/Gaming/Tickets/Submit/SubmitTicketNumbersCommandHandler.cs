using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Application.Abstractions.Time;
using Application.Gaming.Tickets.Submission;
using SharedKernel;

namespace Application.Gaming.Tickets.Submit;

internal sealed class SubmitTicketNumbersCommandHandler(
    ITicketBetSubmissionService ticketBetSubmissionService,
    IDateTimeProvider dateTimeProvider,
    ITenantContext tenantContext,
    IUserContext userContext) : ICommandHandler<SubmitTicketNumbersCommand>
{
    public async Task<Result> Handle(SubmitTicketNumbersCommand request, CancellationToken cancellationToken)
    {
        Result<TicketBetSubmissionResult> submissionResult = await ticketBetSubmissionService.SubmitAsync(
            tenantContext.TenantId,
            request.TicketId,
            request.PlayTypeCode,
            request.Numbers.ToArray(),
            userContext.UserId,
            dateTimeProvider.UtcNow,
            null,
            null,
            cancellationToken);

        return submissionResult.IsFailure
            ? Result.Failure(submissionResult.Error)
            : Result.Success();
    }
}
