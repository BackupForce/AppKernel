using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Gaming;
using Application.Abstractions.Messaging;
using Domain.Gaming.Catalog;
using Domain.Gaming.DrawTemplates;
using Domain.Gaming.Draws;
using Domain.Gaming.Repositories;
using Domain.Gaming.Rules;
using Domain.Gaming.Shared;
using SharedKernel;

namespace Application.Gaming.Draws.Create;

/// <summary>
/// 建立期數並在需要時預先寫入 commit hash。
/// </summary>
/// <remarks>
/// 僅協調時間與持久化，RNG 由 Infrastructure 實作。
/// </remarks>
internal sealed class CreateDrawCommandHandler(
    IDrawRepository drawRepository,
    IDrawTemplateRepository drawTemplateRepository,
    IDrawCodeGenerator drawCodeGenerator,
    IDrawAllowedTicketTemplateRepository drawAllowedTicketTemplateRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ITenantContext tenantContext,
    ILottery539RngService rngService,
    IServerSeedStore serverSeedStore,
    IEntitlementChecker entitlementChecker) : ICommandHandler<CreateDrawCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateDrawCommand request, CancellationToken cancellationToken)
    {
        DateTime now = dateTimeProvider.UtcNow;

        DrawTemplate? template = await drawTemplateRepository.GetByIdAsync(
            tenantContext.TenantId,
            request.TemplateId,
            cancellationToken);
        if (template is null)
        {
            return Result.Failure<Guid>(GamingErrors.DrawTemplateNotFound);
        }

        if (!template.IsActive)
        {
            return Result.Failure<Guid>(GamingErrors.DrawTemplateInactive);
        }

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

        if (template.GameCode != gameCodeResult.Value)
        {
            return Result.Failure<Guid>(GamingErrors.DrawTemplateGameCodeMismatch);
        }

        foreach (DrawTemplatePlayType playType in template.PlayTypes)
        {
            Result playEntitlementResult = await entitlementChecker.EnsurePlayEnabledAsync(
                tenantContext.TenantId,
                template.GameCode,
                playType.PlayTypeCode,
                cancellationToken);
            if (playEntitlementResult.IsFailure)
            {
                return Result.Failure<Guid>(playEntitlementResult.Error);
            }
        }

        PlayRuleRegistry registry = PlayRuleRegistry.CreateDefault();
        string drawCode = await drawCodeGenerator.IssueDrawCodeAsync(
            tenantContext.TenantId,
            gameCodeResult.Value,
            request.DrawAt,
            now,
            cancellationToken);
        string? normalizedDrawCode = string.IsNullOrWhiteSpace(drawCode)
            ? null
            : drawCode.Trim();
        if (normalizedDrawCode is null)
        {
            return Result.Failure<Guid>(GamingErrors.DrawCodeRequired);
        }

        Result<Draw> drawResult = Draw.Create(
            tenantContext.TenantId,
            gameCodeResult.Value,
            normalizedDrawCode,
            request.SalesStartAt,
            request.SalesCloseAt,
            request.DrawAt,
            request.RedeemValidDays,
            now,
            registry);

        if (drawResult.IsFailure)
        {
            return Result.Failure<Guid>(drawResult.Error);
        }

        Draw draw = drawResult.Value;

        Result applyResult = draw.ApplyTemplate(template, registry, now);
        if (applyResult.IsFailure)
        {
            return Result.Failure<Guid>(applyResult.Error);
        }

        if (draw.GetEffectiveStatus(now) == DrawStatus.SalesOpen)
        {
            // 若建立時已在銷售期，先寫入 commit hash 以支持後續驗證。
            string serverSeed = rngService.CreateServerSeed();
            string serverSeedHash = rngService.ComputeServerSeedHash(serverSeed);
            draw.OpenSales(serverSeedHash, now);
            TimeSpan ttl = request.DrawAt > now ? request.DrawAt - now + TimeSpan.FromDays(1) : TimeSpan.FromDays(1);
            await serverSeedStore.StoreAsync(draw.Id, serverSeed, ttl, cancellationToken);
        }

        if (template.AllowedTicketTemplates.Count > 0)
        {
            foreach (DrawTemplateAllowedTicketTemplate allowed in template.AllowedTicketTemplates)
            {
                DrawAllowedTicketTemplate item = DrawAllowedTicketTemplate.Create(
                    tenantContext.TenantId,
                    draw.Id,
                    allowed.TicketTemplateId,
                    now);
                drawAllowedTicketTemplateRepository.Insert(item);
            }
        }

        if (!template.IsLocked)
        {
            template.Lock();
            template.Touch(now);
            drawTemplateRepository.Update(template);
        }

        drawRepository.Insert(draw);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return draw.Id;
    }
}
