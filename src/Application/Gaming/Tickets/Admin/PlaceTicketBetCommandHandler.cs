using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
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

namespace Application.Gaming.Tickets.Admin;

internal sealed class PlaceTicketBetCommandHandler(
    IDrawRepository drawRepository,
    ITicketRepository ticketRepository,
    ITicketIdempotencyRepository ticketIdempotencyRepository,
    IDrawAllowedTicketTemplateRepository drawAllowedTicketTemplateRepository,
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

        Result<PlayTypeCode> playTypeResult = PlayTypeCode.Create(request.PlayTypeCode);
        if (playTypeResult.IsFailure)
        {
            return Result.Failure<PlaceTicketBetResult>(playTypeResult.Error);
        }

        PlayTypeCode playTypeCode = playTypeResult.Value;

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

        if (ticket.TicketTemplateId.HasValue)
        {
            IReadOnlyCollection<DrawAllowedTicketTemplate> allowedTemplates =
                await drawAllowedTicketTemplateRepository.GetByDrawIdAsync(
                    tenantContext.TenantId,
                    draw.Id,
                    cancellationToken);

            if (allowedTemplates.Count == 0)
            {
                return Result.Failure<PlaceTicketBetResult>(GamingErrors.TicketTemplateNotAllowed);
            }

            bool isAllowed = allowedTemplates.Any(item => item.TicketTemplateId == ticket.TicketTemplateId.Value);
            if (!isAllowed)
            {
                return Result.Failure<PlaceTicketBetResult>(GamingErrors.TicketTemplateNotAllowed);
            }
        }

        PlayRuleRegistry registry = PlayRuleRegistry.CreateDefault();
        if (!registry.GetAllowedPlayTypes(draw.GameCode).Contains(playTypeCode))
        {
            return Result.Failure<PlaceTicketBetResult>(GamingErrors.PlayTypeNotAllowed);
        }

        if (!draw.EnabledPlayTypes.Contains(playTypeCode))
        {
            return Result.Failure<PlaceTicketBetResult>(GamingErrors.TicketPlayTypeNotEnabled);
        }

        Result entitlementResult = await entitlementChecker.EnsurePlayEnabledAsync(
            tenantContext.TenantId,
            ticket.GameCode,
            playTypeCode,
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

        IPlayRule rule = registry.GetRule(ticket.GameCode, playTypeCode);
        Result validationResult = rule.ValidateBet(numbersResult.Value);
        if (validationResult.IsFailure)
        {
            return Result.Failure<PlaceTicketBetResult>(validationResult.Error);
        }

        Result submitResult = ticket.SubmitNumbers(
            playTypeCode,
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
            new BetPayloadDto(playTypeCode.Value, numbersResult.Value.Numbers, request.ClientReference, request.Note));

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
        string playTypeCode = string.IsNullOrWhiteSpace(request.PlayTypeCode)
            ? string.Empty
            : PlayTypeCode.Normalize(request.PlayTypeCode);
        string raw = $"{request.TicketId:N}|{playTypeCode}|{numbers}|{request.ClientReference}|{request.Note}";
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(hash);
    }
}
