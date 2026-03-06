using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Time;
using Domain.Gaming.Repositories;
using Domain.Gaming.Shared;
using Domain.Members;
using SharedKernel;

namespace Application.Gaming.TicketClaimEvents.Update;

internal sealed class UpdateTicketClaimEventCommandHandler(
    ITicketClaimEventRepository ticketClaimEventRepository,
    ITicketClaimEventTagRuleRepository ticketClaimEventTagRuleRepository,
    IMemberTagCatalogRepository memberTagCatalogRepository,
    IUserContext userContext,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ITenantContext tenantContext) : ICommandHandler<UpdateTicketClaimEventCommand>
{
    public async Task<Result> Handle(UpdateTicketClaimEventCommand request, CancellationToken cancellationToken)
    {
        if (request.TenantId != tenantContext.TenantId)
        {
            return Result.Failure(GamingErrors.TicketClaimEventTenantMismatch);
        }

        Domain.Gaming.TicketClaimEvents.TicketClaimEvent? ticketClaimEvent = await ticketClaimEventRepository.GetByIdAsync(
            request.TenantId,
            request.EventId,
            cancellationToken);

        if (ticketClaimEvent is null)
        {
            return Result.Failure(GamingErrors.TicketClaimEventNotFound);
        }

        Result updateResult = ticketClaimEvent.UpdateInfo(
            request.Name,
            request.StartsAtUtc,
            request.EndsAtUtc,
            request.TotalQuota,
            request.PerMemberQuota,
            request.ScopeType,
            request.ScopeId,
            request.TicketTemplateId,
            dateTimeProvider.UtcNow);

        if (updateResult.IsFailure)
        {
            return updateResult;
        }

        IReadOnlyCollection<Guid> allowedTagIds = request.AllowedTagIds?
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToArray() ?? Array.Empty<Guid>();

        if (allowedTagIds.Count > 0)
        {
            IReadOnlyCollection<MemberTag> tags = await memberTagCatalogRepository.GetByIdsAsync(
                request.TenantId,
                allowedTagIds,
                cancellationToken);

            if (tags.Count != allowedTagIds.Count)
            {
                return Result.Failure(GamingErrors.MemberTagNotFound);
            }

            if (tags.Any(tag => !tag.IsActive))
            {
                return Result.Failure(GamingErrors.MemberTagInactive);
            }
        }

        ticketClaimEventRepository.Update(ticketClaimEvent);

        await ticketClaimEventTagRuleRepository.ReplaceTagRulesAsync(
            request.TenantId,
            request.EventId,
            allowedTagIds,
            userContext.UserId,
            dateTimeProvider.UtcNow,
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
