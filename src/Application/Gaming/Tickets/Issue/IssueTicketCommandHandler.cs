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
using Domain.Gaming.Tickets;
using Domain.Gaming.TicketTemplates;
using Domain.Members;
using SharedKernel;

namespace Application.Gaming.Tickets.Issue;

internal sealed class IssueTicketCommandHandler(
    IDrawGroupRepository drawGroupRepository,
    IDrawGroupDrawRepository drawGroupDrawRepository,
    IDrawRepository drawRepository,
    ITicketTemplateRepository ticketTemplateRepository,
    IMemberRepository memberRepository,
    TicketIssuanceService ticketIssuanceService,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ITenantContext tenantContext,
    IUserContext userContext,
    IEntitlementChecker entitlementChecker) : ICommandHandler<IssueTicketCommand, IssueTicketResult>
{
    public async Task<Result<IssueTicketResult>> Handle(IssueTicketCommand request, CancellationToken cancellationToken)
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

        Member? member = await memberRepository.GetByIdAsync(tenantContext.TenantId, request.MemberId, cancellationToken);
        if (member is null)
        {
            return Result.Failure<IssueTicketResult>(GamingErrors.MemberNotFound);
        }

        TicketTemplate? template = null;
        if (request.TicketTemplateId.HasValue)
        {
            template = await ticketTemplateRepository.GetByIdAsync(
                tenantContext.TenantId,
                request.TicketTemplateId.Value,
                cancellationToken);
            if (template is null)
            {
                return Result.Failure<IssueTicketResult>(GamingErrors.TicketTemplateNotFound);
            }

            if (!template.IsActive || !template.IsAvailable(now))
            {
                return Result.Failure<IssueTicketResult>(GamingErrors.TicketTemplateNotAvailable);
            }
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

        TicketIssuanceRequest issuanceRequest = new(
            tenantContext.TenantId,
            drawGroup.GameCode,
            member.Id,
            drawGroup.Id,
            template?.Id,
            primaryDrawId,
            eligibleDraws.Select(draw => draw.Id).ToList(),
            IssuedByType.CustomerService,
            userContext.UserId,
            request.IssuedReason,
            null,
            now);

        Result<TicketIssuanceResult> issuanceResult = await ticketIssuanceService.IssueSingleAsync(
            issuanceRequest,
            cancellationToken);
        if (issuanceResult.IsFailure)
        {
            return Result.Failure<IssueTicketResult>(issuanceResult.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        IssueTicketResult result = new IssueTicketResult(
            issuanceResult.Value.Ticket.Id,
            issuanceResult.Value.DrawIds.ToList());
        return result;
    }
}
