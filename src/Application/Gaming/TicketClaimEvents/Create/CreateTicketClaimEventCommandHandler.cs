using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Time;
using Domain.Gaming.Repositories;
using Domain.Gaming.Shared;
using Domain.Gaming.TicketClaimEvents;
using Domain.Members;
using SharedKernel;

namespace Application.Gaming.TicketClaimEvents.Create;

internal sealed class CreateTicketClaimEventCommandHandler(
    ITicketClaimEventRepository ticketClaimEventRepository,
    ITicketClaimEventTagRuleRepository ticketClaimEventTagRuleRepository,
    IMemberTagCatalogRepository memberTagCatalogRepository,
    IUserContext userContext,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ITenantContext tenantContext) : ICommandHandler<CreateTicketClaimEventCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateTicketClaimEventCommand request, CancellationToken cancellationToken)
    {
        if (request.TenantId != tenantContext.TenantId)
        {
            return Result.Failure<Guid>(GamingErrors.TicketClaimEventTenantMismatch);
        }

        DateTime now = dateTimeProvider.UtcNow;
        Result<TicketClaimEvent> createResult = TicketClaimEvent.Create(
            request.TenantId,
            request.Name,
            request.StartsAtUtc,
            request.EndsAtUtc,
            request.TotalQuota,
            request.PerMemberQuota,
            request.ScopeType,
            request.ScopeId,
            request.TicketTemplateId,
            now);

        if (createResult.IsFailure)
        {
            return Result.Failure<Guid>(createResult.Error);
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
                return Result.Failure<Guid>(GamingErrors.MemberTagNotFound);
            }

            if (tags.Any(tag => !tag.IsActive))
            {
                return Result.Failure<Guid>(GamingErrors.MemberTagInactive);
            }
        }

        ticketClaimEventRepository.Insert(createResult.Value);

        await ticketClaimEventTagRuleRepository.ReplaceTagRulesAsync(
            request.TenantId,
            createResult.Value.Id,
            allowedTagIds,
            userContext.UserId,
            now,
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return createResult.Value.Id;
    }
}
