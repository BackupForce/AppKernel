using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Gaming.DrawGroups;
using Domain.Gaming.Repositories;
using Domain.Gaming.Shared;
using SharedKernel;

namespace Application.Gaming.DrawGroups.End;

internal sealed class EndDrawGroupCommandHandler(
    IDrawGroupRepository drawGroupRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ITenantContext tenantContext) : ICommandHandler<EndDrawGroupCommand>
{
    public async Task<Result> Handle(EndDrawGroupCommand request, CancellationToken cancellationToken)
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

        Result endResult = drawGroup.End(dateTimeProvider.UtcNow);
        if (endResult.IsFailure)
        {
            return endResult;
        }

        drawGroupRepository.Update(drawGroup);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
