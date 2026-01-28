using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Gaming.Repositories;
using Domain.Gaming.Shared;
using SharedKernel;

namespace Application.Gaming.DrawTemplates.Activate;

internal sealed class ActivateDrawTemplateCommandHandler(
    IDrawTemplateRepository drawTemplateRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ITenantContext tenantContext) : ICommandHandler<ActivateDrawTemplateCommand>
{
    public async Task<Result> Handle(ActivateDrawTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await drawTemplateRepository.GetByIdAsync(
            tenantContext.TenantId,
            request.TemplateId,
            cancellationToken);
        if (template is null)
        {
            return Result.Failure(GamingErrors.DrawTemplateNotFound);
        }

        template.Activate();
        template.Touch(dateTimeProvider.UtcNow);
        drawTemplateRepository.Update(template);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
