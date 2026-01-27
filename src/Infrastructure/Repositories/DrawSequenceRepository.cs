using System.Data;
using Application.Abstractions.Data;
using Application.Abstractions.Gaming;
using Domain.Gaming.Catalog;
using Dapper;

namespace Infrastructure.Repositories;

internal sealed class DrawSequenceRepository(IDbConnectionFactory dbConnectionFactory) : IDrawSequenceRepository
{
    public async Task<int> GetNextSequenceAsync(
        Guid tenantId,
        GameCode gameCode,
        string prefix,
        CancellationToken cancellationToken)
    {
        const string sql =
            """
            INSERT INTO gaming.draw_sequences (tenant_id, game_code, prefix, last_seq)
            VALUES (@TenantId, @GameCode, @Prefix, 1)
            ON CONFLICT (tenant_id, game_code, prefix)
            DO UPDATE SET last_seq = gaming.draw_sequences.last_seq + 1
            RETURNING last_seq;
            """;

        using IDbConnection connection = dbConnectionFactory.GetOpenConnection();
        CommandDefinition command = new(
            sql,
            new
            {
                TenantId = tenantId,
                GameCode = gameCode.Value,
                Prefix = prefix
            },
            cancellationToken: cancellationToken);

        return await connection.ExecuteScalarAsync<int>(command);
    }
}
