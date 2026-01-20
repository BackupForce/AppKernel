using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Gaming.Dtos;
using Dapper;
using Domain.Gaming.Shared;
using SharedKernel;

namespace Application.Gaming.Campaigns.GetById;

internal sealed class GetCampaignByIdQueryHandler(
    IDbConnectionFactory dbConnectionFactory,
    ITenantContext tenantContext) : IQueryHandler<GetCampaignByIdQuery, CampaignDetailDto>
{
    public async Task<Result<CampaignDetailDto>> Handle(GetCampaignByIdQuery request, CancellationToken cancellationToken)
    {
        if (request.TenantId != tenantContext.TenantId)
        {
            return Result.Failure<CampaignDetailDto>(GamingErrors.CampaignTenantMismatch);
        }

        const string campaignSql = """
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
            WHERE c.tenant_id = @TenantId AND c.id = @CampaignId
            """;

        const string drawsSql = """
            SELECT
                cd.draw_id AS DrawId,
                cd.created_at_utc AS CreatedAtUtc
            FROM gaming.campaign_draws cd
            WHERE cd.tenant_id = @TenantId AND cd.campaign_id = @CampaignId
            ORDER BY cd.created_at_utc ASC
            """;

        using System.Data.IDbConnection connection = dbConnectionFactory.GetOpenConnection();

        CampaignSummaryDto? summary = await connection.QueryFirstOrDefaultAsync<CampaignSummaryDto>(
            campaignSql,
            new { request.TenantId, request.CampaignId });

        if (summary is null)
        {
            return Result.Failure<CampaignDetailDto>(GamingErrors.CampaignNotFound);
        }

        IReadOnlyCollection<CampaignDrawDto> draws = (await connection.QueryAsync<CampaignDrawDto>(
            drawsSql,
            new { request.TenantId, request.CampaignId }))
            .ToList();

        CampaignDetailDto detail = new CampaignDetailDto(
            summary.Id,
            summary.Name,
            summary.Status,
            summary.GameCode,
            summary.PlayTypeCode,
            summary.GrantOpenAtUtc,
            summary.GrantCloseAtUtc,
            summary.DrawCount,
            summary.CreatedAtUtc,
            draws);

        return detail;
    }
}
