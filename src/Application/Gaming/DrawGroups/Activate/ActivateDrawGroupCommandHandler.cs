using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Gaming.DrawGroups;
using Domain.Gaming.Repositories;
using Domain.Gaming.Shared;
using SharedKernel;

namespace Application.Gaming.DrawGroups.Activate;

internal sealed class ActivateDrawGroupCommandHandler(
    IDrawGroupRepository drawGroupRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ITenantContext tenantContext) : ICommandHandler<ActivateDrawGroupCommand>
{
    public async Task<Result> Handle(ActivateDrawGroupCommand request, CancellationToken cancellationToken)
    {
        if (request.TenantId != tenantContext.TenantId)
        {
            return Result.Failure(GamingErrors.DrawGroupTenantMismatch);
        }

        DrawGroup? drawGroup = await drawGroupRepository.GetByIdAsync(request.TenantId, request.DrawGroupId, cancellationToken);
        if (drawGroup is null)
        {
            return Result.Failure(GamingErrors.DrawGroupNotFound);
        }

        Result activateResult = drawGroup.Activate(dateTimeProvider.UtcNow);
        if (activateResult.IsFailure)
        {
            return activateResult;
        }

        drawGroupRepository.Update(drawGroup);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
