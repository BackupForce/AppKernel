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

namespace Application.Gaming.DrawTemplates.Create;

internal sealed class CreateDrawTemplateCommandHandler(
    IDrawTemplateRepository drawTemplateRepository,
    ITicketTemplateRepository ticketTemplateRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ITenantContext tenantContext,
    IEntitlementChecker entitlementChecker) : ICommandHandler<CreateDrawTemplateCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateDrawTemplateCommand request, CancellationToken cancellationToken)
    {
        Result<GameCode> gameCodeResult = GameCode.Create(request.GameCode);
        if (gameCodeResult.IsFailure)
        {
            return Result.Failure<Guid>(gameCodeResult.Error);
        }

        Result entitlementResult = await entitlementChecker.EnsureGameEnabledAsync(
            tenantContext.TenantId,
            gameCodeResult.Value,
            cancellationToken);
        if (entitlementResult.IsFailure)
        {
            return Result.Failure<Guid>(entitlementResult.Error);
        }

        DrawTemplate? existing = await drawTemplateRepository.GetByNameAsync(
            tenantContext.TenantId,
            gameCodeResult.Value,
            request.Name.Trim(),
            cancellationToken);
        if (existing is not null)
        {
            return Result.Failure<Guid>(GamingErrors.DrawTemplateNameDuplicated);
        }

        DateTime now = dateTimeProvider.UtcNow;
        Result<DrawTemplate> createResult = DrawTemplate.Create(
            tenantContext.TenantId,
            gameCodeResult.Value,
            request.Name,
            request.IsActive,
            now);

        if (createResult.IsFailure)
        {
            return Result.Failure<Guid>(createResult.Error);
        }

        DrawTemplate template = createResult.Value;

        PlayRuleRegistry registry = PlayRuleRegistry.CreateDefault();

        foreach (DrawTemplatePlayTypeInput playTypeInput in request.PlayTypes)
        {
            Result<PlayTypeCode> playTypeResult = PlayTypeCode.Create(playTypeInput.PlayTypeCode);
            if (playTypeResult.IsFailure)
            {
                return Result.Failure<Guid>(playTypeResult.Error);
            }

            Result playEntitlement = await entitlementChecker.EnsurePlayEnabledAsync(
                tenantContext.TenantId,
                gameCodeResult.Value,
                playTypeResult.Value,
                cancellationToken);
            if (playEntitlement.IsFailure)
            {
                return Result.Failure<Guid>(playEntitlement.Error);
            }

            IReadOnlyCollection<PlayTypeCode> allowed = registry.GetAllowedPlayTypes(gameCodeResult.Value);
            if (!allowed.Contains(playTypeResult.Value))
            {
                return Result.Failure<Guid>(GamingErrors.PlayTypeNotAllowed);
            }

            Result addPlayTypeResult = template.AddPlayType(playTypeResult.Value);
            if (addPlayTypeResult.IsFailure)
            {
                return Result.Failure<Guid>(addPlayTypeResult.Error);
            }

            IPlayRule rule = registry.GetRule(gameCodeResult.Value, playTypeResult.Value);
            foreach (DrawTemplatePrizeTierInput tierInput in playTypeInput.PrizeTiers)
            {
                Result<PrizeTier> tierResult = PrizeTier.Create(tierInput.Tier);
                if (tierResult.IsFailure)
                {
                    return Result.Failure<Guid>(tierResult.Error);
                }

                if (!rule.GetTiers().Contains(tierResult.Value))
                {
                    return Result.Failure<Guid>(GamingErrors.PrizeTierNotAllowed);
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
                    return Result.Failure<Guid>(optionResult.Error);
                }

                Result upsertResult = template.UpsertPrizeTier(
                    playTypeResult.Value,
                    tierResult.Value,
                    optionResult.Value);
                if (upsertResult.IsFailure)
                {
                    return Result.Failure<Guid>(upsertResult.Error);
                }
            }
        }

        foreach (Guid ticketTemplateId in request.AllowedTicketTemplateIds.Distinct())
        {
            TicketTemplate? ticketTemplate = await ticketTemplateRepository.GetByIdAsync(
                tenantContext.TenantId,
                ticketTemplateId,
                cancellationToken);
            if (ticketTemplate is null)
            {
                return Result.Failure<Guid>(GamingErrors.TicketTemplateNotFound);
            }

            Result addAllowedResult = template.AddAllowedTicketTemplate(ticketTemplate.Id, now);
            if (addAllowedResult.IsFailure)
            {
                return Result.Failure<Guid>(addAllowedResult.Error);
            }
        }

        drawTemplateRepository.Insert(template);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return template.Id;
    }
}
