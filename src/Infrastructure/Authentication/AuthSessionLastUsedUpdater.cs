using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Dapper;

namespace Infrastructure.Authentication;

internal sealed class AuthSessionLastUsedUpdater(IDbConnectionFactory db)
    : IAuthSessionLastUsedUpdater
{
    public async Task<bool> TryUpdateLastUsedUtcAsync(
        Guid tenantId,
        Guid sessionId,
        DateTime nowUtc,
        TimeSpan throttle,
        CancellationToken ct)
    {
        DateTime thresholdUtc = nowUtc - throttle;

        const string sql = """
            UPDATE auth_auth_sessions
            SET last_used_at_utc = @nowUtc
            WHERE tenant_id = @tenantId
              AND id = @sessionId
              AND (last_used_at_utc IS NULL OR last_used_at_utc <= @thresholdUtc);
            """;

        using System.Data.IDbConnection conn = db.GetOpenConnection();
        int affected = await conn.ExecuteAsync(
            new CommandDefinition(
                sql,
                new { tenantId, sessionId, nowUtc, thresholdUtc },
                cancellationToken: ct));

        return affected > 0;
    }
}
