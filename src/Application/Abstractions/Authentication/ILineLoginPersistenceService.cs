using Domain.Auth;
using Domain.Members;
using Domain.Users;

namespace Application.Abstractions.Authentication;

public interface ILineLoginPersistenceService
{
    Task<LineLoginPersistenceResult> PersistAsync(
        Guid tenantId,
        string lineUserId,
        string displayName,
        Uri? pictureUrl,
        string? email,
        string? userAgent,
        string? ip,
        string? deviceId,
        CancellationToken cancellationToken);
}

public sealed record LineLoginPersistenceResult(
    User User,
    Member? Member,
    AuthSession Session,
    string RefreshToken,
    DateTime IssuedAtUtc,
    bool IsNewMember = false);
