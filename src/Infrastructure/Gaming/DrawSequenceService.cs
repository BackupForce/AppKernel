using System.Data;
using Application.Abstractions.Data;
using Application.Abstractions.Gaming;
using Domain.Gaming.Catalog;
using Dapper;

namespace Infrastructure.Gaming;

internal sealed class DrawSequenceService(IDbConnectionFactory dbConnectionFactory) : IDrawSequenceService
{
    public async Task<long> IssueNextAsync(
        Guid tenantId,
        GameCode gameCode,
        DateTime nowUtc,
        CancellationToken cancellationToken)
    {
        const string sql =
            """
            WITH upsert AS (
                INSERT INTO gaming.draw_sequences (tenant_id, game_code, next_value, updated_at_utc)
                VALUES (@TenantId, @GameCode, 2, @NowUtc)
                ON CONFLICT (tenant_id, game_code)
                DO UPDATE SET next_value = gaming.draw_sequences.next_value + 1,
                              updated_at_utc = @NowUtc
                RETURNING next_value
            )
            SELECT (next_value - 1) AS issued_value FROM upsert;
            """;

        using IDbConnection connection = dbConnectionFactory.GetOpenConnection();
        CommandDefinition command = new(
            sql,
            new
            {
                TenantId = tenantId,
                GameCode = gameCode.Value,
                NowUtc = nowUtc
            },
            cancellationToken: cancellationToken);

        return await connection.ExecuteScalarAsync<long>(command);
    }
}
