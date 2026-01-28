using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Gaming.Dtos;
using Domain.Gaming.Catalog;
using Dapper;
using SharedKernel;

namespace Application.Gaming.DrawTemplates.GetList;

internal sealed class GetDrawTemplatesQueryHandler(
    IDbConnectionFactory dbConnectionFactory,
    ITenantContext tenantContext) : IQueryHandler<GetDrawTemplatesQuery, IReadOnlyCollection<DrawTemplateSummaryDto>>
{
    public async Task<Result<IReadOnlyCollection<DrawTemplateSummaryDto>>> Handle(
        GetDrawTemplatesQuery request,
        CancellationToken cancellationToken)
    {
        string? normalizedGameCode = null;
        if (!string.IsNullOrWhiteSpace(request.GameCode))
        {
            Result<GameCode> gameCodeResult = GameCode.Create(request.GameCode);
            if (gameCodeResult.IsFailure)
            {
                return Result.Failure<IReadOnlyCollection<DrawTemplateSummaryDto>>(gameCodeResult.Error);
            }

            normalizedGameCode = gameCodeResult.Value.Value;
        }

        const string sql = """
            SELECT
                t.id AS Id,
                t.game_code AS GameCode,
                t.name AS Name,
                t.is_active AS IsActive,
                t.is_locked AS IsLocked,
                t.version AS Version,
                t.created_at_utc AS CreatedAtUtc,
                t.updated_at_utc AS UpdatedAtUtc
            FROM gaming.draw_templates t
            WHERE t.tenant_id = @TenantId
              AND (@GameCode IS NULL OR t.game_code = @GameCode)
              AND (@IsActive IS NULL OR t.is_active = @IsActive)
            ORDER BY t.updated_at_utc DESC
            """;

        using System.Data.IDbConnection connection = dbConnectionFactory.GetOpenConnection();

        IEnumerable<DrawTemplateSummaryDto> rows = await connection.QueryAsync<DrawTemplateSummaryDto>(
            sql,
            new
            {
                TenantId = tenantContext.TenantId,
                GameCode = normalizedGameCode,
                request.IsActive
            });

        return rows.ToList();
    }
}
