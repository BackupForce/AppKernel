using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Gaming.Draws.PrizePool;
using Application.Gaming.Dtos;
using Domain.Gaming.Shared;
using Dapper;
using SharedKernel;

namespace Application.Gaming.DrawTemplates.GetDetail;

internal sealed class GetDrawTemplateDetailQueryHandler(
    IDbConnectionFactory dbConnectionFactory,
    ITenantContext tenantContext) : IQueryHandler<GetDrawTemplateDetailQuery, DrawTemplateDetailDto>
{
    private sealed record DrawTemplateRow(
        Guid Id,
        string GameCode,
        string Name,
        bool IsActive,
        bool IsLocked,
        int Version,
        DateTime CreatedAtUtc,
        DateTime UpdatedAtUtc);

    private sealed record DrawTemplatePlayTypeRow(string PlayTypeCode);

    private sealed record DrawTemplatePrizeTierRow(
        string PlayTypeCode,
        string Tier,
        Guid? PrizeId,
        string PrizeName,
        decimal PrizeCost,
        int? PrizeRedeemValidDays,
        string? PrizeDescription);

    private sealed record DrawTemplateAllowedTicketRow(Guid TicketTemplateId);

    public async Task<Result<DrawTemplateDetailDto>> Handle(
        GetDrawTemplateDetailQuery request,
        CancellationToken cancellationToken)
    {
        const string templateSql = """
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
              AND t.id = @TemplateId
            """;

        const string playTypesSql = """
            SELECT
                p.play_type_code AS PlayTypeCode
            FROM gaming.draw_template_play_types p
            WHERE p.tenant_id = @TenantId
              AND p.template_id = @TemplateId
            """;

        const string tiersSql = """
            SELECT
                t.play_type_code AS PlayTypeCode,
                t.tier AS Tier,
                t.prize_id_snapshot AS PrizeId,
                t.prize_name_snapshot AS PrizeName,
                t.prize_cost_snapshot AS PrizeCost,
                t.prize_redeem_valid_days_snapshot AS PrizeRedeemValidDays,
                t.prize_description_snapshot AS PrizeDescription
            FROM gaming.draw_template_prize_tiers t
            WHERE t.tenant_id = @TenantId
              AND t.template_id = @TemplateId
            """;

        const string allowedSql = """
            SELECT
                a.ticket_template_id AS TicketTemplateId
            FROM gaming.draw_template_allowed_ticket_templates a
            WHERE a.tenant_id = @TenantId
              AND a.template_id = @TemplateId
            """;

        using System.Data.IDbConnection connection = dbConnectionFactory.GetOpenConnection();

        DrawTemplateRow? template = await connection.QueryFirstOrDefaultAsync<DrawTemplateRow>(
            templateSql,
            new
            {
                tenantContext.TenantId,
                request.TemplateId
            });

        if (template is null)
        {
            return Result.Failure<DrawTemplateDetailDto>(GamingErrors.DrawTemplateNotFound);
        }

        IEnumerable<DrawTemplatePlayTypeRow> playTypeRows = await connection.QueryAsync<DrawTemplatePlayTypeRow>(
            playTypesSql,
            new
            {
                tenantContext.TenantId,
                request.TemplateId
            });

        IEnumerable<DrawTemplatePrizeTierRow> tierRows = await connection.QueryAsync<DrawTemplatePrizeTierRow>(
            tiersSql,
            new
            {
                tenantContext.TenantId,
                request.TemplateId
            });

        IEnumerable<DrawTemplateAllowedTicketRow> allowedRows = await connection.QueryAsync<DrawTemplateAllowedTicketRow>(
            allowedSql,
            new
            {
                tenantContext.TenantId,
                request.TemplateId
            });

        Dictionary<string, List<DrawTemplatePrizeTierDto>> tierMap = new(StringComparer.OrdinalIgnoreCase);
        foreach (DrawTemplatePrizeTierRow row in tierRows)
        {
            if (!tierMap.TryGetValue(row.PlayTypeCode, out List<DrawTemplatePrizeTierDto>? list))
            {
                list = new List<DrawTemplatePrizeTierDto>();
                tierMap[row.PlayTypeCode] = list;
            }

            list.Add(new DrawTemplatePrizeTierDto(
                row.Tier,
                new PrizeOptionDto(
                    row.PrizeId,
                    row.PrizeName,
                    row.PrizeCost,
                    row.PrizeRedeemValidDays,
                    row.PrizeDescription)));
        }

        List<DrawTemplatePlayTypeDto> playTypes = playTypeRows
            .Select(row =>
            {
                tierMap.TryGetValue(row.PlayTypeCode, out List<DrawTemplatePrizeTierDto>? tiers);
                return new DrawTemplatePlayTypeDto(
                    row.PlayTypeCode,
                    tiers?.OrderBy(item => item.Tier, StringComparer.OrdinalIgnoreCase).ToList()
                    ?? new List<DrawTemplatePrizeTierDto>());
            })
            .OrderBy(item => item.PlayTypeCode, StringComparer.OrdinalIgnoreCase)
            .ToList();

        DrawTemplateDetailDto detail = new DrawTemplateDetailDto(
            template.Id,
            template.GameCode,
            template.Name,
            template.IsActive,
            template.IsLocked,
            template.Version,
            template.CreatedAtUtc,
            template.UpdatedAtUtc,
            playTypes,
            allowedRows.Select(row => row.TicketTemplateId).ToList());

        return detail;
    }
}
