using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Gaming;
using Application.Abstractions.Messaging;
using Application.Abstractions.Time;
using Application.Gaming.Tickets.Issue;
using Application.Gaming.Tickets.Services;
using Domain.Gaming.DrawGroups;
using Domain.Gaming.Draws;
using Domain.Gaming.Repositories;
using Domain.Gaming.Shared;
using Domain.Gaming.Tickets;
using Domain.Members;
using SharedKernel;

namespace Application.Gaming.Tickets.Claim;

internal sealed class ClaimDrawGroupTicketCommandHandler(
    IDrawGroupRepository drawGroupRepository,
    IDrawGroupDrawRepository drawGroupDrawRepository,
    IDrawRepository drawRepository,
    ITicketRepository ticketRepository,
    IMemberRepository memberRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ITenantContext tenantContext,
    IUserContext userContext,
    IEntitlementChecker entitlementChecker) : ICommandHandler<ClaimDrawGroupTicketCommand, IssueTicketResult>
{
    public async Task<Result<IssueTicketResult>> Handle(ClaimDrawGroupTicketCommand request, CancellationToken cancellationToken)
    {
        DrawGroup? drawGroup = await drawGroupRepository.GetByIdAsync(
            tenantContext.TenantId,
            request.DrawGroupId,
            cancellationToken);
        if (drawGroup is null)
        {
            return Result.Failure<IssueTicketResult>(GamingErrors.DrawGroupNotFound);
        }

        DateTime now = dateTimeProvider.UtcNow;
        if (drawGroup.Status != DrawGroupStatus.Enabled
            || drawGroup.GrantOpenAtUtc is null
            || drawGroup.GrantCloseAtUtc is null
            || now < drawGroup.GrantOpenAtUtc.Value
            || now >= drawGroup.GrantCloseAtUtc.Value)
        {
            return Result.Failure<IssueTicketResult>(GamingErrors.DrawGroupInactive);
        }

        Result entitlementResult = await entitlementChecker.EnsurePlayEnabledAsync(
            tenantContext.TenantId,
            drawGroup.GameCode,
            drawGroup.PlayTypeCode,
            cancellationToken);
        if (entitlementResult.IsFailure)
        {
            return Result.Failure<IssueTicketResult>(entitlementResult.Error);
        }

        Member? member = await memberRepository.GetByUserIdAsync(tenantContext.TenantId, userContext.UserId, cancellationToken);
        if (member is null)
        {
            return Result.Failure<IssueTicketResult>(GamingErrors.MemberNotFound);
        }

        bool alreadyClaimed = await ticketRepository.ExistsForDrawGroupAsync(
            tenantContext.TenantId,
            member.Id,
            drawGroup.Id,
            cancellationToken);
        if (alreadyClaimed)
        {
            return Result.Failure<IssueTicketResult>(GamingErrors.DrawGroupAlreadyClaimed);
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
                return Result.Failure<IssueTicketResult>(GamingErrors.DrawGroupDrawPlayTypeNotEnabled);
            }

            if (!DrawEligibility.IsEligiblePrimary(draw, now))
            {
                continue;
            }

            eligibleDraws.Add(draw);
        }

        if (eligibleDraws.Count == 0)
        {
            return Result.Failure<IssueTicketResult>(GamingErrors.TicketDrawNotAvailable);
        }

        Guid primaryDrawId = eligibleDraws
            .OrderBy(draw => draw.SalesOpenAt)
            .ThenBy(draw => draw.DrawAt)
            .Select(draw => draw.Id)
            .First();

        Ticket ticket = Ticket.Create(
            tenantContext.TenantId,
            drawGroup.GameCode,
            member.Id,
            drawGroup.Id,
            null,
            primaryDrawId,
            null,
            null,
            now,
            IssuedByType.DrawGroup,
            userContext.UserId,
            null,
            null,
            now);

        ticketRepository.Insert(ticket);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        IssueTicketResult result = new IssueTicketResult(ticket.Id, primaryDrawId);
        return result;
    }
}
