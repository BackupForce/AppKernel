using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
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

namespace Application.Gaming.Tickets.Admin;

internal sealed class PlaceTicketBetCommandHandler(
    IDrawRepository drawRepository,
    ITicketRepository ticketRepository,
    ITicketIdempotencyRepository ticketIdempotencyRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ITenantContext tenantContext,
    IUserContext userContext,
    IEntitlementChecker entitlementChecker) : ICommandHandler<PlaceTicketBetCommand, PlaceTicketBetResult>
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

        Ticket? ticket = await ticketRepository.GetByIdAsync(
            tenantContext.TenantId,
            request.TicketId,
            cancellationToken);
        if (ticket is null)
        {
            return Result.Failure<PlaceTicketBetResult>(GamingErrors.TicketNotFound);
        }

        Guid? drawId = ticket.DrawId;
        if (!drawId.HasValue)
        {
            return Result.Failure<PlaceTicketBetResult>(GamingErrors.DrawNotFound);
        }

        Draw? draw = await drawRepository.GetByIdAsync(tenantContext.TenantId, drawId.Value, cancellationToken);
        if (draw is null)
        {
            return Result.Failure<PlaceTicketBetResult>(GamingErrors.DrawNotFound);
        }

        Result entitlementResult = await entitlementChecker.EnsurePlayEnabledAsync(
            tenantContext.TenantId,
            ticket.GameCode,
            ticket.PlayTypeCode,
            cancellationToken);
        if (entitlementResult.IsFailure)
        {
            return Result.Failure<PlaceTicketBetResult>(entitlementResult.Error);
        }

        DateTime now = dateTimeProvider.UtcNow;
        Result submitPolicyResult = TicketSubmissionPolicy.EnsureCanSubmit(ticket, draw, now);
        if (submitPolicyResult.IsFailure)
        {
            return Result.Failure<PlaceTicketBetResult>(submitPolicyResult.Error);
        }

        Result<LotteryNumbers> numbersResult = LotteryNumbers.Create(request.Numbers);
        if (numbersResult.IsFailure)
        {
            return Result.Failure<PlaceTicketBetResult>(numbersResult.Error);
        }

        PlayRuleRegistry registry = PlayRuleRegistry.CreateDefault();
        IPlayRule rule = registry.GetRule(ticket.GameCode, ticket.PlayTypeCode);
        Result validationResult = rule.ValidateBet(numbersResult.Value);
        if (validationResult.IsFailure)
        {
            return Result.Failure<PlaceTicketBetResult>(validationResult.Error);
        }

        Result submitResult = ticket.SubmitNumbers(
            numbersResult.Value,
            now,
            userContext.UserId,
            request.ClientReference,
            request.Note);
        if (submitResult.IsFailure)
        {
            return Result.Failure<PlaceTicketBetResult>(submitResult.Error);
        }

        TicketLine line = ticket.Lines.Single();

        using IDbTransaction transaction = await unitOfWork.BeginTransactionAsync();

        bool updated = await ticketRepository.TryMarkSubmittedAsync(
            tenantContext.TenantId,
            ticket.Id,
            ticket.SubmittedAtUtc ?? now,
            ticket.SubmittedByUserId,
            ticket.SubmittedClientReference,
            ticket.SubmittedNote,
            cancellationToken);

        if (!updated)
        {
            return Result.Failure<PlaceTicketBetResult>(GamingErrors.TicketAlreadySubmittedConflict);
        }

        ticketRepository.InsertLine(line);

        PlaceTicketBetResult result = new PlaceTicketBetResult(
            ticket.Id,
            TicketStatusMapper.ToAdminStatus(ticket.SubmissionStatus),
            ticket.SubmittedAtUtc ?? now,
            ticket.SubmittedByUserId ?? Guid.Empty,
            new BetPayloadDto(numbersResult.Value.Numbers, request.ClientReference, request.Note));

        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            string payload = JsonSerializer.Serialize(result, JsonOptions);
            TicketIdempotencyRecord record = TicketIdempotencyRecord.Create(
                tenantContext.TenantId,
                idempotencyKey,
                Operation,
                requestHash,
                payload,
                now);
            ticketIdempotencyRepository.Insert(record);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        transaction.Commit();

        return result;
    }

    private static string? NormalizeKey(string? key)
    {
        return string.IsNullOrWhiteSpace(key) ? null : key.Trim();
    }

    private static string ComputeBetHash(PlaceTicketBetCommand request)
    {
        string numbers = request.Numbers is null ? string.Empty : string.Join(',', request.Numbers);
        string raw = $"{request.TicketId:N}|{numbers}|{request.ClientReference}|{request.Note}";
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(hash);
    }
}
