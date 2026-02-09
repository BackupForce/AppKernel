namespace Application.Abstractions.Authentication;

public interface IAuthSessionLastUsedUpdater
{
    /// <summary>
    /// Try to update LastUsedAtUtc with throttle logic.
    /// Returns true if updated; false if skipped.
    /// </summary>
    Task<bool> TryUpdateLastUsedUtcAsync(
        Guid tenantId,
        Guid sessionId,
        DateTime nowUtc,
        TimeSpan throttle,
        CancellationToken ct);
}
