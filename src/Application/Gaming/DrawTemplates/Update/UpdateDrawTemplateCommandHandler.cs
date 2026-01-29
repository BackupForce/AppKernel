using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Gaming;
using Application.Abstractions.Messaging;
using Domain.Gaming.Catalog;
using Domain.Gaming.DrawTemplates;
using Domain.Gaming.Repositories;
using Domain.Gaming.Rules;
using Domain.Gaming.Shared;
using Domain.Gaming.TicketTemplates;
using SharedKernel;

namespace Application.Gaming.DrawTemplates.Update;

internal sealed class UpdateDrawTemplateCommandHandler(
    IDrawTemplateRepository drawTemplateRepository,
    ITicketTemplateRepository ticketTemplateRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ITenantContext tenantContext,
    IEntitlementChecker entitlementChecker) : ICommandHandler<UpdateDrawTemplateCommand>
{
    public async Task<Result> Handle(UpdateDrawTemplateCommand request, CancellationToken cancellationToken)
    {
        DrawTemplate? template = await drawTemplateRepository.GetByIdAsync(
            tenantContext.TenantId,
            request.TemplateId,
            cancellationToken);
        if (template is null)
        {
            return Result.Failure(GamingErrors.DrawTemplateNotFound);
        }

        bool hasDraws = await drawTemplateRepository.HasDrawsAsync(
            tenantContext.TenantId,
            template.Id,
            cancellationToken);
        DateTime now = dateTimeProvider.UtcNow;
        if (hasDraws)
        {
            template.Lock();
        }

        if (!string.Equals(template.Name, request.Name, StringComparison.Ordinal))
        {
            DrawTemplate? existing = await drawTemplateRepository.GetByNameAsync(
                tenantContext.TenantId,
                template.GameCode,
                request.Name.Trim(),
                cancellationToken);
            if (existing is not null && existing.Id != template.Id)
            {
                return Result.Failure(GamingErrors.DrawTemplateNameDuplicated);
            }

            Result updateNameResult = template.UpdateName(request.Name);
            if (updateNameResult.IsFailure)
            {
                return Result.Failure(updateNameResult.Error);
            }
        }

        PlayRuleRegistry registry = PlayRuleRegistry.CreateDefault();

        Dictionary<string, DrawTemplatePlayTypeInput> requestedPlayTypes = request.PlayTypes
            .ToDictionary(
                item => PlayTypeCode.Normalize(item.PlayTypeCode),
                item => item,
                StringComparer.OrdinalIgnoreCase);

        foreach (DrawTemplatePlayType existingPlayType in template.PlayTypes.ToList())
        {
            if (!requestedPlayTypes.ContainsKey(existingPlayType.PlayTypeCode.Value))
            {
                Result removeResult = template.RemovePlayType(existingPlayType.PlayTypeCode);
                if (removeResult.IsFailure)
                {
                    return Result.Failure(removeResult.Error);
                }
            }
        }

        foreach (DrawTemplatePlayTypeInput playTypeInput in requestedPlayTypes.Values)
        {
            Result<PlayTypeCode> playTypeResult = PlayTypeCode.Create(playTypeInput.PlayTypeCode);
            if (playTypeResult.IsFailure)
            {
                return Result.Failure(playTypeResult.Error);
            }

            Result playEntitlement = await entitlementChecker.EnsurePlayEnabledAsync(
                tenantContext.TenantId,
                template.GameCode,
                playTypeResult.Value,
                cancellationToken);
            if (playEntitlement.IsFailure)
            {
                return Result.Failure(playEntitlement.Error);
            }

            IReadOnlyCollection<PlayTypeCode> allowed = registry.GetAllowedPlayTypes(template.GameCode);
            if (!allowed.Contains(playTypeResult.Value))
            {
                return Result.Failure(GamingErrors.PlayTypeNotAllowed);
            }

            if (template.PlayTypes.All(item => item.PlayTypeCode != playTypeResult.Value))
            {
                Result addPlayTypeResult = template.AddPlayType(playTypeResult.Value);
                if (addPlayTypeResult.IsFailure)
                {
                    return Result.Failure(addPlayTypeResult.Error);
                }
            }

            HashSet<string> requestedTiers = playTypeInput.PrizeTiers
                .Select(item => PrizeTier.Normalize(item.Tier))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (DrawTemplatePrizeTier existingTier in template.PrizeTiers
                .Where(item => item.PlayTypeCode == playTypeResult.Value)
                .ToList())
            {
                if (!requestedTiers.Contains(existingTier.Tier.Value))
                {
                    Result removeTierResult = template.RemovePrizeTier(
                        playTypeResult.Value,
                        existingTier.Tier);
                    if (removeTierResult.IsFailure)
                    {
                        return Result.Failure(removeTierResult.Error);
                    }
                }
            }

            IPlayRule rule = registry.GetRule(template.GameCode, playTypeResult.Value);
            foreach (DrawTemplatePrizeTierInput tierInput in playTypeInput.PrizeTiers)
            {
                Result<PrizeTier> tierResult = PrizeTier.Create(tierInput.Tier);
                if (tierResult.IsFailure)
                {
                    return Result.Failure(tierResult.Error);
                }

                if (!rule.GetTiers().Contains(tierResult.Value))
                {
                    return Result.Failure(GamingErrors.PrizeTierNotAllowed);
                }

                Result<PrizeOption> optionResult = PrizeOption.Create(
                    tierInput.Option.Name,
                    tierInput.Option.Cost,
                    tierInput.Option.PayoutAmount,
                    tierInput.Option.RedeemValidDays,
                    tierInput.Option.Description,
                    tierInput.Option.PrizeId);
                if (optionResult.IsFailure)
                {
                    return Result.Failure(optionResult.Error);
                }

                Result upsertResult = template.UpsertPrizeTier(
                    playTypeResult.Value,
                    tierResult.Value,
                    optionResult.Value);
                if (upsertResult.IsFailure)
                {
                    return Result.Failure(upsertResult.Error);
                }
            }
        }

        HashSet<Guid> requestedAllowed = request.AllowedTicketTemplateIds.ToHashSet();
        foreach (DrawTemplateAllowedTicketTemplate existing in template.AllowedTicketTemplates.ToList())
        {
            if (!requestedAllowed.Contains(existing.TicketTemplateId))
            {
                Result removeResult = template.RemoveAllowedTicketTemplate(existing.TicketTemplateId);
                if (removeResult.IsFailure)
                {
                    return Result.Failure(removeResult.Error);
                }
            }
        }

        foreach (Guid templateId in requestedAllowed)
        {
            if (template.AllowedTicketTemplates.All(item => item.TicketTemplateId != templateId))
            {
                TicketTemplate? ticketTemplate = await ticketTemplateRepository.GetByIdAsync(
                    tenantContext.TenantId,
                    templateId,
                    cancellationToken);
                if (ticketTemplate is null)
                {
                    return Result.Failure(GamingErrors.TicketTemplateNotFound);
                }

                Result addResult = template.AddAllowedTicketTemplate(ticketTemplate.Id, now);
                if (addResult.IsFailure)
                {
                    return Result.Failure(addResult.Error);
                }
            }
        }

        template.Touch(now);
        drawTemplateRepository.Update(template);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
