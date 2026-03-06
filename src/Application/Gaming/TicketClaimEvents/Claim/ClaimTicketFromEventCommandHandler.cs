using System.Data;
using System.Text.Json;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Gaming;
using Application.Abstractions.Messaging;
using Application.Abstractions.Time;
using Application.Gaming.Tickets.Services;
using Domain.Gaming.DrawGroups;
using Domain.Gaming.Draws;
using Domain.Gaming.Repositories;
using Domain.Gaming.Shared;
using Domain.Gaming.TicketClaimEvents;
using Domain.Gaming.Tickets;
using Domain.Gaming.TicketTemplates;
using Domain.Members;
using SharedKernel;

namespace Application.Gaming.TicketClaimEvents.Claim;

internal sealed class ClaimTicketFromEventCommandHandler(
    ITicketClaimEventRepository ticketClaimEventRepository,
    ITicketClaimMemberCounterRepository ticketClaimMemberCounterRepository,
    ITicketClaimRecordRepository ticketClaimRecordRepository,
    IDrawGroupRepository drawGroupRepository,
    IDrawGroupDrawRepository drawGroupDrawRepository,
    IDrawRepository drawRepository,
    ITicketTemplateRepository ticketTemplateRepository,
    IMemberRepository memberRepository,
    ITicketClaimEventTagRuleRepository ticketClaimEventTagRuleRepository,
    IMemberTagBindingRepository memberTagBindingRepository,
    IMemberTagCatalogRepository memberTagCatalogRepository,
    TicketIssuanceService ticketIssuanceService,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ITenantContext tenantContext,
    IUserContext userContext,
    IEntitlementChecker entitlementChecker) : ICommandHandler<ClaimTicketFromEventCommand, TicketClaimResult>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<Result<TicketClaimResult>> Handle(ClaimTicketFromEventCommand request, CancellationToken cancellationToken)
    {
        string? idempotencyKey = NormalizeKey(request.IdempotencyKey);
        DateTime now = dateTimeProvider.UtcNow;

        using IDbTransaction transaction = await unitOfWork.BeginTransactionAsync();

        TicketClaimEvent? ticketClaimEvent = await ticketClaimEventRepository.GetByIdForUpdateAsync(
            tenantContext.TenantId,
            request.EventId,
            cancellationToken);

        if (ticketClaimEvent is null)
        {
            return Result.Failure<TicketClaimResult>(GamingErrors.TicketClaimEventNotFound);
        }

        Member? member = await memberRepository.GetByUserIdAsync(
            tenantContext.TenantId,
            userContext.UserId,
            cancellationToken);
        if (member is null)
        {
            return Result.Failure<TicketClaimResult>(GamingErrors.MemberNotFound);
        }

        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            TicketClaimRecord? existing = await ticketClaimRecordRepository.GetByIdempotencyKeyAsync(
                tenantContext.TenantId,
                ticketClaimEvent.Id,
                member.Id,
                idempotencyKey,
                cancellationToken);

            if (existing is not null)
            {
                IReadOnlyCollection<Guid> cachedIds = DeserializeTicketIds(existing.IssuedTicketIds);
                return new TicketClaimResult(existing.EventId, cachedIds, existing.Quantity);
            }
        }

        Result canClaim = ticketClaimEvent.EnsureCanClaim(now);
        if (canClaim.IsFailure)
        {
            return Result.Failure<TicketClaimResult>(canClaim.Error);
        }

        Result tagEligibilityResult = await EnsureMemberTagEligibilityAsync(ticketClaimEvent, member, cancellationToken);
        if (tagEligibilityResult.IsFailure)
        {
            return Result.Failure<TicketClaimResult>(tagEligibilityResult.Error);
        }

        TicketClaimMemberCounter? counter = await ticketClaimMemberCounterRepository.GetByIdForUpdateAsync(
            ticketClaimEvent.Id,
            member.Id,
            cancellationToken);

        bool isNewCounter = counter is null;
        counter ??= TicketClaimMemberCounter.Create(ticketClaimEvent.Id, member.Id, now);

        Result memberQuotaResult = counter.Increase(1, ticketClaimEvent.PerMemberQuota, now);
        if (memberQuotaResult.IsFailure)
        {
            return Result.Failure<TicketClaimResult>(memberQuotaResult.Error);
        }

        Result quotaResult = ticketClaimEvent.IncreaseClaimed(1, now);
        if (quotaResult.IsFailure)
        {
            return Result.Failure<TicketClaimResult>(quotaResult.Error);
        }

        Result<TicketIssuanceRequest> issuanceRequestResult = await BuildIssuanceRequestAsync(
            ticketClaimEvent,
            member,
            now,
            cancellationToken);

        if (issuanceRequestResult.IsFailure)
        {
            return Result.Failure<TicketClaimResult>(issuanceRequestResult.Error);
        }

        Result<TicketIssuanceResult> issuanceResult = await ticketIssuanceService.IssueSingleAsync(
            issuanceRequestResult.Value
            );

        if (issuanceResult.IsFailure)
        {
            return Result.Failure<TicketClaimResult>(issuanceResult.Error);
        }

        Guid ticketId = issuanceResult.Value.Ticket.Id;
        IReadOnlyCollection<Guid> ticketIds = new[] { ticketId };
        string issuedTicketIds = JsonSerializer.Serialize(ticketIds, JsonOptions);

        TicketClaimRecord record = TicketClaimRecord.Create(
            tenantContext.TenantId,
            ticketClaimEvent.Id,
            member.Id,
            1,
            idempotencyKey,
            issuedTicketIds,
            now);

        if (isNewCounter)
        {
            ticketClaimMemberCounterRepository.Insert(counter);
        }
        else
        {
            ticketClaimMemberCounterRepository.Update(counter);
        }

        ticketClaimEventRepository.Update(ticketClaimEvent);
        ticketClaimRecordRepository.Insert(record);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        transaction.Commit();

        return new TicketClaimResult(ticketClaimEvent.Id, ticketIds, 1);
    }

    private async Task<Result<TicketIssuanceRequest>> BuildIssuanceRequestAsync(
        TicketClaimEvent ticketClaimEvent,
        Member member,
        DateTime now,
        CancellationToken cancellationToken)
    {
        TicketTemplate? template = null;
        if (ticketClaimEvent.TicketTemplateId.HasValue)
        {
            template = await ticketTemplateRepository.GetByIdAsync(
                tenantContext.TenantId,
                ticketClaimEvent.TicketTemplateId.Value,
                cancellationToken);

            if (template is null)
            {
                return Result.Failure<TicketIssuanceRequest>(GamingErrors.TicketTemplateNotFound);
            }

            if (!template.IsActive || !template.IsAvailable(now))
            {
                return Result.Failure<TicketIssuanceRequest>(GamingErrors.TicketTemplateNotAvailable);
            }
        }

        if (ticketClaimEvent.ScopeType == TicketClaimEventScopeType.SingleDrawGroup)
        {
            DrawGroup? drawGroup = await drawGroupRepository.GetByIdAsync(
                tenantContext.TenantId,
                ticketClaimEvent.ScopeId,
                cancellationToken);
            if (drawGroup is null)
            {
                return Result.Failure<TicketIssuanceRequest>(GamingErrors.DrawGroupNotFound);
            }

            if (drawGroup.Status != DrawGroupStatus.Enabled
                || drawGroup.GrantOpenAtUtc is null
                || drawGroup.GrantCloseAtUtc is null
                || now < drawGroup.GrantOpenAtUtc.Value
                || now >= drawGroup.GrantCloseAtUtc.Value)
            {
                return Result.Failure<TicketIssuanceRequest>(GamingErrors.DrawGroupInactive);
            }

            Result entitlementResult = await entitlementChecker.EnsurePlayEnabledAsync(
                tenantContext.TenantId,
                drawGroup.GameCode,
                drawGroup.PlayTypeCode,
                cancellationToken);
            if (entitlementResult.IsFailure)
            {
                return Result.Failure<TicketIssuanceRequest>(entitlementResult.Error);
            }

            IReadOnlyCollection<DrawGroupDraw> drawGroupDraws = await drawGroupDrawRepository.GetByDrawGroupIdAsync(
                tenantContext.TenantId,
                drawGroup.Id,
                cancellationToken);

            List<Guid> drawIds = drawGroupDraws.Select(item => item.DrawId).Distinct().ToList();
            IReadOnlyCollection<Draw> draws = drawIds.Count == 0
                ? Array.Empty<Draw>()
                : await drawRepository.GetByIdsAsync(tenantContext.TenantId, drawIds, cancellationToken);

            List<Draw> eligibleDraws = new();

            foreach (Draw draw in draws)
            {
                if (draw.GameCode != drawGroup.GameCode)
                {
                    continue;
                }

                if (!draw.EnabledPlayTypes.Contains(drawGroup.PlayTypeCode))
                {
                    return Result.Failure<TicketIssuanceRequest>(GamingErrors.DrawGroupDrawPlayTypeNotEnabled);
                }

                if (!DrawEligibility.IsEligiblePrimary(draw, now))
                {
                    continue;
                }

                eligibleDraws.Add(draw);
            }

            if (eligibleDraws.Count == 0)
            {
                return Result.Failure<TicketIssuanceRequest>(GamingErrors.TicketDrawNotAvailable);
            }

            Guid primaryDrawId = eligibleDraws
                .OrderBy(draw => draw.SalesOpenAt)
                .ThenBy(draw => draw.DrawAt)
                .Select(draw => draw.Id)
                .First();

            return new TicketIssuanceRequest(
                tenantContext.TenantId,
                drawGroup.GameCode,
                member.Id,
                drawGroup.Id,
                template?.Id,
                primaryDrawId,
                IssuedByType.System,
                userContext.UserId,
                "TicketClaimEvent",
                null,
                now);
        }

        Draw? targetDraw = await drawRepository.GetByIdAsync(
            tenantContext.TenantId,
            ticketClaimEvent.ScopeId,
            cancellationToken);
        if (targetDraw is null)
        {
            return Result.Failure<TicketIssuanceRequest>(GamingErrors.DrawNotFound);
        }

        if (!DrawEligibility.IsEligiblePrimary(targetDraw, now))
        {
            return Result.Failure<TicketIssuanceRequest>(GamingErrors.TicketDrawNotAvailable);
        }

        return new TicketIssuanceRequest(
            tenantContext.TenantId,
            targetDraw.GameCode,
            member.Id,
            null,
            template?.Id,
            targetDraw.Id,
            IssuedByType.System,
            userContext.UserId,
            "TicketClaimEvent",
            null,
            now);
    }

    private async Task<Result> EnsureMemberTagEligibilityAsync(
        TicketClaimEvent ticketClaimEvent,
        Member member,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<Guid> eventTagIds = await ticketClaimEventTagRuleRepository.GetTagIdsByEventIdAsync(
            tenantContext.TenantId,
            ticketClaimEvent.Id,
            cancellationToken);

        if (eventTagIds.Count == 0)
        {
            return Result.Success();
        }

        IReadOnlyCollection<MemberTag> eventTags = await memberTagCatalogRepository.GetByIdsAsync(
            tenantContext.TenantId,
            eventTagIds,
            cancellationToken);

        HashSet<Guid> activeEventTagIds = eventTags
            .Where(tag => tag.IsActive)
            .Select(tag => tag.Id)
            .ToHashSet();

        if (activeEventTagIds.Count == 0)
        {
            return Result.Success();
        }

        IReadOnlyCollection<Guid> memberTagIds = await memberTagBindingRepository.GetTagIdsByMemberIdAsync(
            tenantContext.TenantId,
            member.Id,
            cancellationToken);

        bool matched = memberTagIds.Any(tagId => activeEventTagIds.Contains(tagId));
        if (!matched)
        {
            return Result.Failure(GamingErrors.TicketClaimEventMemberTagNotEligible);
        }

        return Result.Success();
    }

    private static string? NormalizeKey(string? key)
    {
        return string.IsNullOrWhiteSpace(key) ? null : key.Trim();
    }

    private static IReadOnlyCollection<Guid> DeserializeTicketIds(string? payload)
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            return Array.Empty<Guid>();
        }

        return JsonSerializer.Deserialize<IReadOnlyCollection<Guid>>(payload, JsonOptions)
               ?? Array.Empty<Guid>();
    }
}
