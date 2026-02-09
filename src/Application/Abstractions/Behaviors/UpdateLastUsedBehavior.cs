using Application.Abstractions.Authentication;
using MediatR;
using SharedKernel;

namespace Application.Abstractions.Behaviors;

internal sealed class UpdateLastUsedBehavior<TRequest, TResponse>(
    ICurrentSession currentSession,
    IAuthSessionLastUsedUpdater updater,
    IDateTimeProvider dateTimeProvider) : IPipelineBehavior<TRequest, TResponse>
{
    private static readonly TimeSpan Throttle = TimeSpan.FromMinutes(1);

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        TResponse response = await next();

        if (!currentSession.IsAuthenticated)
        {
            return response;
        }

        Guid? tenantId = currentSession.TenantId;
        Guid? sessionId = currentSession.AuthSessionId;

        if (tenantId is null || sessionId is null)
        {
            return response;
        }

        DateTime nowUtc = dateTimeProvider.UtcNow;

        await updater.TryUpdateLastUsedUtcAsync(
            tenantId.Value,
            sessionId.Value,
            nowUtc,
            Throttle,
            ct);

        return response;
    }
}
