using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Gaming;
using Application.Abstractions.Messaging;
using Application.Abstractions.Time;
using Application.Gaming.Tickets.Issue;
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
    ITicketDrawRepository ticketDrawRepository,
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
        if (drawGroup.Status != DrawGroupStatus.Active || now < drawGroup.GrantOpenAtUtc || now >= drawGroup.GrantCloseAtUtc)
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

        List<Draw> eligibleDraws = new();

        foreach (DrawGroupDraw drawGroupDraw in drawGroupDraws)
        {
            Draw? draw = await drawRepository.GetByIdAsync(tenantContext.TenantId, drawGroupDraw.DrawId, cancellationToken);
            if (draw is null)
            {
                continue;
            }

            if (draw.GameCode != drawGroup.GameCode)
            {
                continue;
            }

            if (!draw.EnabledPlayTypes.Contains(drawGroup.PlayTypeCode))
            {
                return Result.Failure<IssueTicketResult>(GamingErrors.DrawGroupDrawPlayTypeNotEnabled);
            }

            if (!draw.IsWithinSalesWindow(now))
            {
                continue;
            }

            eligibleDraws.Add(draw);
        }

        if (eligibleDraws.Count == 0)
        {
            return Result.Failure<IssueTicketResult>(GamingErrors.TicketDrawNotAvailable);
        }

        Guid? primaryDrawId = eligibleDraws
            .OrderBy(draw => draw.DrawAt)
            .Select(draw => draw.Id)
            .FirstOrDefault();

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

        foreach (Draw draw in eligibleDraws)
        {
            TicketDraw ticketDraw = TicketDraw.Create(tenantContext.TenantId, ticket.Id, draw.Id, now);
            ticketDrawRepository.Insert(ticketDraw);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        IssueTicketResult result = new IssueTicketResult(ticket.Id, eligibleDraws.Select(draw => draw.Id).ToList());
        return result;
    }
}
