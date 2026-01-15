using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Gaming.Shared;
using SharedKernel;

namespace Application.Gaming.TicketTemplates.Create;

/// <summary>
/// 建立票種模板。
/// </summary>
/// <remarks>
/// Application 層負責檢查代碼唯一性，避免 Domain 直接依賴資料庫。
/// </remarks>
internal sealed class CreateTicketTemplateCommandHandler(
    ITicketTemplateRepository ticketTemplateRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ITenantContext tenantContext) : ICommandHandler<CreateTicketTemplateCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateTicketTemplateCommand request, CancellationToken cancellationToken)
    {
        TicketTemplate? existing = await ticketTemplateRepository.GetByCodeAsync(
            tenantContext.TenantId,
            request.Code,
            cancellationToken);

        if (existing is not null)
        {
            return Result.Failure<Guid>(GamingErrors.TicketTemplateCodeDuplicated);
        }

        Result<TicketTemplate> createResult = TicketTemplate.Create(
            tenantContext.TenantId,
            request.Code,
            request.Name,
            request.Type,
            request.Price,
            request.ValidFrom,
            request.ValidTo,
            request.MaxLinesPerTicket,
            dateTimeProvider.UtcNow);

        if (createResult.IsFailure)
        {
            return Result.Failure<Guid>(createResult.Error);
        }

        TicketTemplate template = createResult.Value;
        ticketTemplateRepository.Insert(template);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return template.Id;
    }
}
