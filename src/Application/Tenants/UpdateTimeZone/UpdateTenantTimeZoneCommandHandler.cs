using Application.Abstractions.Caching;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Time;
using Application.Time;
using Domain.Tenants;
using SharedKernel;

namespace Application.Tenants.UpdateTimeZone;

internal sealed class UpdateTenantTimeZoneCommandHandler(
    ITenantRepository tenantRepository,
    IUnitOfWork unitOfWork,
    ITimeZoneResolver timeZoneResolver,
    ICacheService cacheService)
    : ICommandHandler<UpdateTenantTimeZoneCommand>
{
    private const string TenantTimeZoneCacheKeyPrefix = "tenant:";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(20);

    public async Task<Result> Handle(UpdateTenantTimeZoneCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.TimeZoneId))
        {
            return Result.Failure(TimeZoneErrors.Required);
        }

        Tenant? tenant = await tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant is null)
        {
            return Result.Failure(TenantErrors.NotFound(request.TenantId));
        }

        try
        {
            timeZoneResolver.Resolve(request.TimeZoneId.Trim());
        }
        catch (ArgumentException)
        {
            return Result.Failure(TimeZoneErrors.Invalid(request.TimeZoneId));
        }
        catch (InvalidTimeZoneException)
        {
            return Result.Failure(TimeZoneErrors.Invalid(request.TimeZoneId));
        }
        catch (TimeZoneNotFoundException)
        {
            return Result.Failure(TimeZoneErrors.Invalid(request.TimeZoneId));
        }

        tenant.SetTimeZone(request.TimeZoneId);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        string cacheKey = $"{TenantTimeZoneCacheKeyPrefix}{tenant.Id}:timezone";
        await cacheService.SetAsync(cacheKey, tenant.TimeZoneId, CacheTtl, cancellationToken);

        return Result.Success();
    }
}
