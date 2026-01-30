using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Time;
using Application.Gaming.Tickets.Services;
using Domain.Gaming.Catalog;
using Domain.Gaming.Draws;
using Domain.Gaming.Repositories;
using Domain.Gaming.Shared;
using Domain.Gaming.Tickets;
using Domain.Members;
using SharedKernel;

namespace Application.Gaming.Tickets.Admin;

internal sealed class IssueMemberTicketsCommandHandler(
    IDrawRepository drawRepository,
    ITicketIdempotencyRepository ticketIdempotencyRepository,
    IMemberRepository memberRepository,
    TicketIssuanceService ticketIssuanceService,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ITenantContext tenantContext,
    IUserContext userContext) : ICommandHandler<IssueMemberTicketsCommand, IssueMemberTicketsResult>
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

        DateTime now = dateTimeProvider.UtcNow;
        GameCode gameCode = gameCodeResult.Value;

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

        Member? member = await memberRepository.GetByIdAsync(tenantContext.TenantId, request.MemberId, cancellationToken);
        if (member is null)
        {
            return Result.Failure<IssueMemberTicketsResult>(GamingErrors.MemberNotFound);
        }

        if (request.Quantity <= 0 || request.Quantity > 100)
        {
            return Result.Failure<IssueMemberTicketsResult>(GamingErrors.TicketIssueQuantityInvalid);
        }

        TicketIssuanceRequest issuanceRequest = new(
            tenantContext.TenantId,
            draw.GameCode,
            member.Id,
            null,
            null,
            draw.Id,
            new[] { draw.Id },
            IssuedByType.Backoffice,
            userContext.UserId,
            request.Reason,
            request.Note,
            now);

        Result<IReadOnlyCollection<Ticket>> issuanceResult = await ticketIssuanceService.IssueBulkSameDrawAsync(
            issuanceRequest,
            request.Quantity,
            cancellationToken);
        if (issuanceResult.IsFailure)
        {
            return Result.Failure<IssueMemberTicketsResult>(issuanceResult.Error);
        }

        IReadOnlyCollection<Ticket> tickets = issuanceResult.Value;

        IssueMemberTicketsResult result = new IssueMemberTicketsResult(
            tickets.Select(ticket => new IssuedTicketDto(
                ticket.Id,
                TicketStatusMapper.ToAdminStatus(ticket.SubmissionStatus),
                ticket.IssuedAtUtc,
                ticket.DrawId ?? draw.Id,
                ticket.GameCode.Value,
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
        string raw = $"{request.MemberId:N}|{request.GameCode}|{request.DrawId:N}|{request.Quantity}|{request.Reason}|{request.Note}";
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
