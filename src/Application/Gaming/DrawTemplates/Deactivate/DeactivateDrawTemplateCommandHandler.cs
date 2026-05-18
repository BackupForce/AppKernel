using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Gaming.Repositories;
using Domain.Gaming.Shared;
using SharedKernel;

namespace Application.Gaming.DrawTemplates.Deactivate;

internal sealed class DeactivateDrawTemplateCommandHandler(
    IDrawTemplateRepository drawTemplateRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ITenantContext tenantContext) : ICommandHandler<DeactivateDrawTemplateCommand>
{
    public async Task<Result> Handle(DeactivateDrawTemplateCommand request, CancellationToken cancellationToken)
    {
        Domain.Gaming.DrawTemplates.DrawTemplate? template = await drawTemplateRepository.GetByIdAsync(
            tenantContext.TenantId,
            request.TemplateId,
            cancellationToken);
        if (template is null)
        {
            return Result.Failure(GamingErrors.DrawTemplateNotFound);
        }

        template.Deactivate();
        template.Touch(dateTimeProvider.UtcNow);
        drawTemplateRepository.Update(template);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
