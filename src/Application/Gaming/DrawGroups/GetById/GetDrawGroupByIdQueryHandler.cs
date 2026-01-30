using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Gaming.Dtos;
using Dapper;
using Domain.Gaming.Shared;
using SharedKernel;

namespace Application.Gaming.DrawGroups.GetById;

internal sealed class GetDrawGroupByIdQueryHandler(
    IDbConnectionFactory dbConnectionFactory,
    ITenantContext tenantContext) : IQueryHandler<GetDrawGroupByIdQuery, DrawGroupDetailDto>
{
    public async Task<Result<DrawGroupDetailDto>> Handle(GetDrawGroupByIdQuery request, CancellationToken cancellationToken)
    {
        if (request.TenantId != tenantContext.TenantId)
        {
            return Result.Failure<DrawGroupDetailDto>(GamingErrors.DrawGroupTenantMismatch);
        }

        const string drawGroupSql = """
            SELECT
                c.id AS Id,
                c.name AS Name,
                CASE
                    WHEN c.status = 0 THEN 'Draft'
                    WHEN c.status = 1 THEN 'Active'
                    WHEN c.status = 2 THEN 'Ended'
                    ELSE 'Draft'
                END AS Status,
                c.game_code AS GameCode,
                c.play_type_code AS PlayTypeCode,
                c.grant_open_at_utc AS GrantOpenAtUtc,
                c.grant_close_at_utc AS GrantCloseAtUtc,
                c.created_at_utc AS CreatedAtUtc,
                (
                    SELECT COUNT(*)
                    FROM gaming.campaign_draws cd
                    WHERE cd.campaign_id = c.id
                ) AS DrawCount
            FROM gaming.campaigns c
            WHERE c.tenant_id = @TenantId AND c.id = @DrawGroupId
            """;

        const string drawsSql = """
            SELECT
                cd.draw_id AS DrawId,
                cd.created_at_utc AS CreatedAtUtc
            FROM gaming.campaign_draws cd
            WHERE cd.tenant_id = @TenantId AND cd.campaign_id = @DrawGroupId
            ORDER BY cd.created_at_utc ASC
            """;

        using System.Data.IDbConnection connection = dbConnectionFactory.GetOpenConnection();

        DrawGroupSummaryDto? summary = await connection.QueryFirstOrDefaultAsync<DrawGroupSummaryDto>(
            drawGroupSql,
            new { request.TenantId, request.DrawGroupId });

        if (summary is null)
        {
            return Result.Failure<DrawGroupDetailDto>(GamingErrors.DrawGroupNotFound);
        }

        IReadOnlyCollection<DrawGroupDrawDto> draws = (await connection.QueryAsync<DrawGroupDrawDto>(
            drawsSql,
            new { request.TenantId, request.DrawGroupId }))
            .ToList();

        DrawGroupDetailDto detail = new DrawGroupDetailDto(
            summary.Id,
            summary.Name,
            summary.Status,
            summary.GameCode,
            summary.PlayTypeCode,
            summary.GrantOpenAtUtc,
            summary.GrantCloseAtUtc,
            (int)summary.DrawCount, // 轉型：將 long 轉為 int
            summary.CreatedAtUtc,
            draws);

        return detail;
    }
}
