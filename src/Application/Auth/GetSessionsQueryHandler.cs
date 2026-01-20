using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Domain.Auth;
using SharedKernel;

namespace Application.Auth;

internal sealed class GetSessionsQueryHandler(
    IAuthSessionRepository authSessionRepository,
    IUserContext userContext,
    IDateTimeProvider dateTimeProvider)
    : IQueryHandler<GetSessionsQuery, IReadOnlyCollection<AuthSessionDto>>
{
    public async Task<Result<IReadOnlyCollection<AuthSessionDto>>> Handle(
        GetSessionsQuery query,
        CancellationToken cancellationToken)
    {
        Guid? tenantId = userContext.TenantId;
        if (!tenantId.HasValue)
        {
            return Result.Failure<IReadOnlyCollection<AuthSessionDto>>(AuthErrors.TenantContextMissing);
        }

        IReadOnlyList<AuthSession> sessions = await authSessionRepository.GetByUserAsync(
            tenantId.Value,
            userContext.UserId,
            cancellationToken);

        DateTime utcNow = dateTimeProvider.UtcNow;

        IReadOnlyCollection<AuthSessionDto> result = sessions
            .Where(s => s.ExpiresAtUtc > utcNow)
            .Select(s => new AuthSessionDto
            {
                Id = s.Id,
                CreatedAtUtc = s.CreatedAtUtc,
                LastUsedAtUtc = s.LastUsedAtUtc,
                ExpiresAtUtc = s.ExpiresAtUtc,
                RevokedAtUtc = s.RevokedAtUtc,
                RevokeReason = s.RevokeReason,
                UserAgent = s.UserAgent,
                Ip = s.Ip,
                DeviceId = s.DeviceId
            })
            .ToArray();

        return Result.Success(result);
    }
}
