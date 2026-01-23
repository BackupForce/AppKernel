using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Gaming;
using Application.Abstractions.Messaging;
using Application.Abstractions.Time;
using Application.Gaming.Tickets.Issue;
using Domain.Gaming.Campaigns;
using Domain.Gaming.Draws;
using Domain.Gaming.Repositories;
using Domain.Gaming.Shared;
using Domain.Gaming.Tickets;
using Domain.Members;
using SharedKernel;

namespace Application.Gaming.Tickets.Claim;

internal sealed class ClaimCampaignTicketCommandHandler(
    ICampaignRepository campaignRepository,
    ICampaignDrawRepository campaignDrawRepository,
    IDrawRepository drawRepository,
    ITicketRepository ticketRepository,
    ITicketDrawRepository ticketDrawRepository,
    IMemberRepository memberRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ITenantContext tenantContext,
    IUserContext userContext,
    IEntitlementChecker entitlementChecker) : ICommandHandler<ClaimCampaignTicketCommand, IssueTicketResult>
{
    public async Task<Result<IssueTicketResult>> Handle(ClaimCampaignTicketCommand request, CancellationToken cancellationToken)
    {
        Campaign? campaign = await campaignRepository.GetByIdAsync(
            tenantContext.TenantId,
            request.CampaignId,
            cancellationToken);
        if (campaign is null)
        {
            return Result.Failure<IssueTicketResult>(GamingErrors.CampaignNotFound);
        }

        DateTime now = dateTimeProvider.UtcNow;
        if (campaign.Status != CampaignStatus.Active || now < campaign.GrantOpenAtUtc || now >= campaign.GrantCloseAtUtc)
        {
            return Result.Failure<IssueTicketResult>(GamingErrors.CampaignInactive);
        }

        Result entitlementResult = await entitlementChecker.EnsurePlayEnabledAsync(
            tenantContext.TenantId,
            campaign.GameCode,
            campaign.PlayTypeCode,
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

        bool alreadyClaimed = await ticketRepository.ExistsForCampaignAsync(
            tenantContext.TenantId,
            member.Id,
            campaign.Id,
            cancellationToken);
        if (alreadyClaimed)
        {
            return Result.Failure<IssueTicketResult>(GamingErrors.CampaignAlreadyClaimed);
        }

        IReadOnlyCollection<CampaignDraw> campaignDraws = await campaignDrawRepository.GetByCampaignIdAsync(
            tenantContext.TenantId,
            campaign.Id,
            cancellationToken);

        List<Draw> eligibleDraws = new();

        foreach (CampaignDraw campaignDraw in campaignDraws)
        {
            Draw? draw = await drawRepository.GetByIdAsync(tenantContext.TenantId, campaignDraw.DrawId, cancellationToken);
            if (draw is null)
            {
                continue;
            }

            if (draw.GameCode != campaign.GameCode)
            {
                continue;
            }

            if (!draw.EnabledPlayTypes.Contains(campaign.PlayTypeCode))
            {
                return Result.Failure<IssueTicketResult>(GamingErrors.CampaignDrawPlayTypeNotEnabled);
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
            campaign.GameCode,
            campaign.PlayTypeCode,
            member.Id,
            campaign.Id,
            null,
            primaryDrawId,
            null,
            null,
            now,
            IssuedByType.Campaign,
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
