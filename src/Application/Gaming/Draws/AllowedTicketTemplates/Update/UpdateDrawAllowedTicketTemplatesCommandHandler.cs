using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Gaming;
using Application.Abstractions.Messaging;
using Domain.Gaming.Draws;
using Domain.Gaming.Repositories;
using Domain.Gaming.Shared;
using Domain.Gaming.TicketTemplates;
using SharedKernel;

namespace Application.Gaming.Draws.AllowedTicketTemplates.Update;

/// <summary>
/// 覆寫期數允許票種清單。
/// </summary>
/// <remarks>
/// 採覆寫語意可簡化後台操作，並避免累積無效資料。
/// </remarks>
internal sealed class UpdateDrawAllowedTicketTemplatesCommandHandler(
    IDrawRepository drawRepository,
    ITicketTemplateRepository ticketTemplateRepository,
    IDrawAllowedTicketTemplateRepository drawAllowedTicketTemplateRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ITenantContext tenantContext,
    IEntitlementChecker entitlementChecker) : ICommandHandler<UpdateDrawAllowedTicketTemplatesCommand>
{
    public async Task<Result> Handle(UpdateDrawAllowedTicketTemplatesCommand request, CancellationToken cancellationToken)
    {
        Draw? draw = await drawRepository.GetByIdAsync(tenantContext.TenantId, request.DrawId, cancellationToken);
        if (draw is null)
        {
            return Result.Failure(GamingErrors.DrawNotFound);
        }

        Result entitlementResult = await entitlementChecker.EnsureGameEnabledAsync(
            tenantContext.TenantId,
            draw.GameCode,
            cancellationToken);
        if (entitlementResult.IsFailure)
        {
            return Result.Failure(entitlementResult.Error);
        }

        HashSet<Guid> distinctIds = new HashSet<Guid>(request.TemplateIds);
        foreach (Guid templateId in distinctIds)
        {
            TicketTemplate? template = await ticketTemplateRepository.GetByIdAsync(
                tenantContext.TenantId,
                templateId,
                cancellationToken);
            if (template is null)
            {
                return Result.Failure(GamingErrors.TicketTemplateNotFound);
            }
        }

        IReadOnlyCollection<DrawAllowedTicketTemplate> existing =
            await drawAllowedTicketTemplateRepository.GetByDrawIdAsync(
                tenantContext.TenantId,
                draw.Id,
                cancellationToken);

        drawAllowedTicketTemplateRepository.RemoveRange(existing);

        DateTime now = dateTimeProvider.UtcNow;
        foreach (Guid templateId in distinctIds)
        {
            DrawAllowedTicketTemplate item = DrawAllowedTicketTemplate.Create(
                tenantContext.TenantId,
                draw.Id,
                templateId,
                now);
            drawAllowedTicketTemplateRepository.Insert(item);
        }

        // 中文註解：覆寫式更新確保同樣的輸入不會產生重複資料，具備冪等性。
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
