using System.Data;
using System.Globalization;
using System.Text;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Gaming.Dtos;
using Dapper;
using Domain.Gaming.Campaigns;
using Domain.Gaming.Catalog;
using Domain.Gaming.Shared;
using SharedKernel;

namespace Application.Gaming.Campaigns.List;

internal sealed class ListCampaignsQueryHandler(
    IDbConnectionFactory dbConnectionFactory,
    ITenantContext tenantContext) : IQueryHandler<ListCampaignsQuery, PagedResult<CampaignSummaryDto>>
{
    public async Task<Result<PagedResult<CampaignSummaryDto>>> Handle(ListCampaignsQuery request, CancellationToken cancellationToken)
    {
        if (request.TenantId != tenantContext.TenantId)
        {
            return Result.Failure<PagedResult<CampaignSummaryDto>>(GamingErrors.CampaignTenantMismatch);
        }

        var builder = new StringBuilder(
            """
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
            WHERE c.tenant_id = @TenantId
            """);

        var parameters = new DynamicParameters();
        parameters.Add("TenantId", request.TenantId);

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            if (!Enum.TryParse(request.Status.Trim(), true, out CampaignStatus status))
            {
                return Result.Failure<PagedResult<CampaignSummaryDto>>(GamingErrors.CampaignStatusInvalid);
            }

            builder.Append(" AND c.status = @Status");
            parameters.Add("Status", (int)status);
        }

        if (!string.IsNullOrWhiteSpace(request.GameCode))
        {
            Result<GameCode> gameCodeResult = GameCode.Create(request.GameCode);
            if (gameCodeResult.IsFailure)
            {
                return Result.Failure<PagedResult<CampaignSummaryDto>>(gameCodeResult.Error);
            }

            builder.Append(" AND c.game_code = @GameCode");
            parameters.Add("GameCode", gameCodeResult.Value.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            builder.Append(" AND c.name ILIKE @Keyword");
            parameters.Add("Keyword", $"%{request.Keyword.Trim()}%");
        }

        const string countSql = "SELECT COUNT(*) FROM ({0}) AS counted";
        string baseSql = builder.ToString();
        string finalSql = $"{baseSql} ORDER BY c.created_at_utc DESC LIMIT @PageSize OFFSET @Offset";

        parameters.Add("PageSize", request.PageSize);
        parameters.Add("Offset", (request.Page - 1) * request.PageSize);

        using IDbConnection connection = dbConnectionFactory.GetOpenConnection();

        IEnumerable<CampaignSummaryDto> items = await connection.QueryAsync<CampaignSummaryDto>(finalSql, parameters);
        int totalCount = await connection.ExecuteScalarAsync<int>(string.Format(CultureInfo.InvariantCulture, countSql, baseSql), parameters);

        return PagedResult<CampaignSummaryDto>.Create(items, totalCount, request.Page, request.PageSize);
    }
}
