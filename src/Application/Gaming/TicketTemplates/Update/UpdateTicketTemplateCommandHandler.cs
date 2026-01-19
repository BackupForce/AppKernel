using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Gaming.Repositories;
using Domain.Gaming.Shared;
using Domain.Gaming.TicketTemplates;
using SharedKernel;

namespace Application.Gaming.TicketTemplates.Update;

/// <summary>
/// 更新票種模板資料。
/// </summary>
internal sealed class UpdateTicketTemplateCommandHandler(
    ITicketTemplateRepository ticketTemplateRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ITenantContext tenantContext) : ICommandHandler<UpdateTicketTemplateCommand>
{
    public async Task<Result> Handle(UpdateTicketTemplateCommand request, CancellationToken cancellationToken)
    {
        TicketTemplate? template = await ticketTemplateRepository.GetByIdAsync(
            tenantContext.TenantId,
            request.TemplateId,
            cancellationToken);
        if (template is null)
        {
            return Result.Failure(GamingErrors.TicketTemplateNotFound);
        }

        TicketTemplate? existing = await ticketTemplateRepository.GetByCodeAsync(
            tenantContext.TenantId,
            request.Code,
            cancellationToken);
        if (existing is not null && existing.Id != template.Id)
        {
            return Result.Failure(GamingErrors.TicketTemplateCodeDuplicated);
        }

        Result updateResult = template.Update(
            request.Code,
            request.Name,
            request.Type,
            request.Price,
            request.ValidFrom,
            request.ValidTo,
            request.MaxLinesPerTicket,
            dateTimeProvider.UtcNow);

        if (updateResult.IsFailure)
        {
            return Result.Failure(updateResult.Error);
        }

        ticketTemplateRepository.Update(template);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
