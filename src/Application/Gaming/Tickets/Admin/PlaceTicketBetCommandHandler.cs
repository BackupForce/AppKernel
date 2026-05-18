using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Time;
using Application.Gaming.Tickets.Submission;
using Domain.Gaming.Catalog;
using Domain.Gaming.Repositories;
using Domain.Gaming.Shared;
using Domain.Gaming.Tickets;
using SharedKernel;

namespace Application.Gaming.Tickets.Admin;

internal sealed class PlaceTicketBetCommandHandler(
    ITicketIdempotencyRepository ticketIdempotencyRepository,
    ITicketBetSubmissionService ticketBetSubmissionService,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ITenantContext tenantContext,
    IUserContext userContext) : ICommandHandler<PlaceTicketBetCommand, PlaceTicketBetResult>
{
    private const string Operation = "place_ticket_bet";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<Result<PlaceTicketBetResult>> Handle(
        PlaceTicketBetCommand request,
        CancellationToken cancellationToken)
    {
        string? idempotencyKey = NormalizeKey(request.IdempotencyKey);
        string requestHash = ComputeBetHash(request);

        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            TicketIdempotencyRecord? existing = await ticketIdempotencyRepository.GetByKeyAsync(
                tenantContext.TenantId,
                idempotencyKey,
                Operation,
                cancellationToken);

            if (existing is not null)
            {
                if (!string.Equals(existing.RequestHash, requestHash, StringComparison.Ordinal))
                {
                    return Result.Failure<PlaceTicketBetResult>(GamingErrors.TicketIdempotencyKeyConflict);
                }

                PlaceTicketBetResult? cached =
                    JsonSerializer.Deserialize<PlaceTicketBetResult>(existing.ResponsePayload, JsonOptions);
                if (cached is null)
                {
                    return Result.Failure<PlaceTicketBetResult>(GamingErrors.TicketIdempotencyPayloadInvalid);
                }

                return cached;
            }
        }

        Result<TicketBetSubmissionResult> submissionResult = await ticketBetSubmissionService.SubmitAsync(
            tenantContext.TenantId,
            request.TicketId,
            request.PlayTypeCode,
            request.Numbers.ToArray(),
            userContext.UserId,
            dateTimeProvider.UtcNow,
            request.ClientReference,
            request.Note,
            cancellationToken);
        if (submissionResult.IsFailure)
        {
            return Result.Failure<PlaceTicketBetResult>(submissionResult.Error);
        }

        PlaceTicketBetResult result = new PlaceTicketBetResult(
            submissionResult.Value.TicketId,
            TicketStatusMapper.ToAdminStatus(submissionResult.Value.SubmissionStatus),
            submissionResult.Value.SubmittedAtUtc,
            submissionResult.Value.SubmittedByUserId,
            new BetPayloadDto(
                submissionResult.Value.PlayTypeCode,
                submissionResult.Value.Numbers,
                submissionResult.Value.ClientReference,
                submissionResult.Value.Note));

        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            string payload = JsonSerializer.Serialize(result, JsonOptions);
            TicketIdempotencyRecord record = TicketIdempotencyRecord.Create(
                tenantContext.TenantId,
                idempotencyKey,
                Operation,
                requestHash,
                payload,
                dateTimeProvider.UtcNow);
            ticketIdempotencyRepository.Insert(record);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return result;
    }

    private static string? NormalizeKey(string? key)
    {
        return string.IsNullOrWhiteSpace(key) ? null : key.Trim();
    }

    private static string ComputeBetHash(PlaceTicketBetCommand request)
    {
        string numbers = request.Numbers is null ? string.Empty : string.Join(',', request.Numbers);
        string playTypeCode = string.IsNullOrWhiteSpace(request.PlayTypeCode)
            ? string.Empty
            : PlayTypeCode.Normalize(request.PlayTypeCode);
        string raw = $"{request.TicketId:N}|{playTypeCode}|{numbers}|{request.ClientReference}|{request.Note}";
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(hash);
    }
}
