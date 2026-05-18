using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Gaming.Repositories;
using Domain.Gaming.Shared;
using Domain.Gaming.TicketTemplates;
using SharedKernel;

namespace Application.Gaming.TicketTemplates.Deactivate;

/// <summary>
/// 停用票種模板。
/// </summary>
internal sealed class DeactivateTicketTemplateCommandHandler(
    ITicketTemplateRepository ticketTemplateRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ITenantContext tenantContext) : ICommandHandler<DeactivateTicketTemplateCommand>
{
    public async Task<Result> Handle(DeactivateTicketTemplateCommand request, CancellationToken cancellationToken)
    {
        TicketTemplate? template = await ticketTemplateRepository.GetByIdAsync(
            tenantContext.TenantId,
            request.TemplateId,
            cancellationToken);
        if (template is null)
        {
            return Result.Failure(GamingErrors.TicketTemplateNotFound);
        }

        template.Deactivate(dateTimeProvider.UtcNow);
        ticketTemplateRepository.Update(template);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
