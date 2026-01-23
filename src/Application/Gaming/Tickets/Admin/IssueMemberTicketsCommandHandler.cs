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
using Domain.Members;
using SharedKernel;

namespace Application.Gaming.Tickets.Admin;

internal sealed class IssueMemberTicketsCommandHandler(
    IDrawRepository drawRepository,
    ITicketRepository ticketRepository,
    ITicketDrawRepository ticketDrawRepository,
    ITicketIdempotencyRepository ticketIdempotencyRepository,
    IMemberRepository memberRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ITenantContext tenantContext,
    IUserContext userContext,
    IEntitlementChecker entitlementChecker) : ICommandHandler<IssueMemberTicketsCommand, IssueMemberTicketsResult>
{
    private const string Operation = "issue_ticket";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<Result<IssueMemberTicketsResult>> Handle(
        IssueMemberTicketsCommand request,
        CancellationToken cancellationToken)
    {
        string? idempotencyKey = NormalizeKey(request.IdempotencyKey);
        string requestHash = ComputeIssueHash(request);

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
                    return Result.Failure<IssueMemberTicketsResult>(GamingErrors.TicketIdempotencyKeyConflict);
                }

                IssueMemberTicketsResult? cached =
                    JsonSerializer.Deserialize<IssueMemberTicketsResult>(existing.ResponsePayload, JsonOptions);
                if (cached is null)
                {
                    return Result.Failure<IssueMemberTicketsResult>(GamingErrors.TicketIdempotencyPayloadInvalid);
                }

                return cached;
            }
        }

        Result<GameCode> gameCodeResult = GameCode.Create(request.GameCode);
        if (gameCodeResult.IsFailure)
        {
            return Result.Failure<IssueMemberTicketsResult>(gameCodeResult.Error);
        }

        Result<PlayTypeCode> playTypeResult = PlayTypeCode.Create(request.PlayTypeCode);
        if (playTypeResult.IsFailure)
        {
            return Result.Failure<IssueMemberTicketsResult>(playTypeResult.Error);
        }

        DateTime now = dateTimeProvider.UtcNow;
        GameCode gameCode = gameCodeResult.Value;
        PlayTypeCode playTypeCode = playTypeResult.Value;

        Draw? draw = await drawRepository.GetByIdAsync(tenantContext.TenantId, request.DrawId, cancellationToken);
        if (draw is null)
        {
            return Result.Failure<IssueMemberTicketsResult>(GamingErrors.DrawNotFound);
        }

        if (draw.GameCode != gameCode)
        {
            return Result.Failure<IssueMemberTicketsResult>(GamingErrors.DrawNotFound);
        }

        if (draw.IsEffectivelyClosed(now))
        {
            return Result.Failure<IssueMemberTicketsResult>(GamingErrors.DrawNotOpen);
        }

        PlayRuleRegistry registry = PlayRuleRegistry.CreateDefault();
        if (!registry.GetAllowedPlayTypes(draw.GameCode).Contains(playTypeCode))
        {
            return Result.Failure<IssueMemberTicketsResult>(GamingErrors.PlayTypeNotAllowed);
        }

        if (!draw.EnabledPlayTypes.Contains(playTypeCode))
        {
            return Result.Failure<IssueMemberTicketsResult>(GamingErrors.TicketPlayTypeNotEnabled);
        }

        Result entitlementResult = await entitlementChecker.EnsurePlayEnabledAsync(
            tenantContext.TenantId,
            draw.GameCode,
            playTypeCode,
            cancellationToken);
        if (entitlementResult.IsFailure)
        {
            return Result.Failure<IssueMemberTicketsResult>(entitlementResult.Error);
        }

        Member? member = await memberRepository.GetByIdAsync(tenantContext.TenantId, request.MemberId, cancellationToken);
        if (member is null)
        {
            return Result.Failure<IssueMemberTicketsResult>(GamingErrors.MemberNotFound);
        }

        if (request.Quantity <= 0 || request.Quantity > 100)
        {
            return Result.Failure<IssueMemberTicketsResult>(GamingErrors.TicketIssueQuantityInvalid);
        }

        List<Ticket> tickets = new();
        List<TicketDraw> ticketDraws = new();
        for (int index = 0; index < request.Quantity; index++)
        {
            Ticket ticket = Ticket.Create(
                tenantContext.TenantId,
                draw.GameCode,
                playTypeCode,
                member.Id,
                null,
                null,
                draw.Id,
                null,
                null,
                now,
                IssuedByType.Backoffice,
                userContext.UserId,
                request.Reason,
                request.Note,
                now);

            tickets.Add(ticket);
            ticketDraws.Add(TicketDraw.Create(tenantContext.TenantId, ticket.Id, draw.Id, now));
        }

        foreach (Ticket ticket in tickets)
        {
            ticketRepository.Insert(ticket);
        }

        foreach (TicketDraw ticketDraw in ticketDraws)
        {
            ticketDrawRepository.Insert(ticketDraw);
        }

        IssueMemberTicketsResult result = new IssueMemberTicketsResult(
            tickets.Select(ticket => new IssuedTicketDto(
                ticket.Id,
                TicketStatusMapper.ToAdminStatus(ticket.SubmissionStatus),
                ticket.IssuedAtUtc,
                ticket.DrawId ?? draw.Id,
                ticket.GameCode.Value,
                ticket.PlayTypeCode.Value,
                ticket.IssuedByUserId ?? Guid.Empty,
                ticket.IssuedReason,
                ticket.IssuedNote))
            .ToList());

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

        return result;
    }

    private static string? NormalizeKey(string? key)
    {
        return string.IsNullOrWhiteSpace(key) ? null : key.Trim();
    }

    private static string ComputeIssueHash(IssueMemberTicketsCommand request)
    {
        string raw = $"{request.MemberId:N}|{request.GameCode}|{request.PlayTypeCode}|{request.DrawId:N}|{request.Quantity}|{request.Reason}|{request.Note}";
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(hash);
    }
}

internal static class TicketStatusMapper
{
    public static string ToAdminStatus(TicketSubmissionStatus status)
    {
        return status switch
        {
            TicketSubmissionStatus.NotSubmitted => "Issued",
            TicketSubmissionStatus.Submitted => "Submitted",
            TicketSubmissionStatus.Cancelled => "Voided",
            TicketSubmissionStatus.Expired => "Expired",
            _ => status.ToString()
        };
    }
}
