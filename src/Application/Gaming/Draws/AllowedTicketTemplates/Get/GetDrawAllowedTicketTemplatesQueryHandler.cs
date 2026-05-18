using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Gaming;
using Application.Abstractions.Messaging;
using Application.Gaming.Dtos;
using Dapper;
using SharedKernel;
using Domain.Gaming.Draws;
using Domain.Gaming.Repositories;
using Domain.Gaming.Shared;

namespace Application.Gaming.Draws.AllowedTicketTemplates.Get;

internal sealed class GetDrawAllowedTicketTemplatesQueryHandler(
    IDbConnectionFactory dbConnectionFactory,
    ITenantContext tenantContext,
    IDrawRepository drawRepository,
    IEntitlementChecker entitlementChecker) : IQueryHandler<GetDrawAllowedTicketTemplatesQuery, IReadOnlyCollection<DrawAllowedTicketTemplateDto>>
{
    public async Task<Result<IReadOnlyCollection<DrawAllowedTicketTemplateDto>>> Handle(
        GetDrawAllowedTicketTemplatesQuery request,
        CancellationToken cancellationToken)
    {
        Draw? draw = await drawRepository.GetByIdAsync(tenantContext.TenantId, request.DrawId, cancellationToken);
        if (draw is null)
        {
            return Result.Failure<IReadOnlyCollection<DrawAllowedTicketTemplateDto>>(GamingErrors.DrawNotFound);
        }

        Result entitlementResult = await entitlementChecker.EnsureGameEnabledAsync(
            tenantContext.TenantId,
            draw.GameCode,
            cancellationToken);
        if (entitlementResult.IsFailure)
        {
            return Result.Failure<IReadOnlyCollection<DrawAllowedTicketTemplateDto>>(entitlementResult.Error);
        }

        const string sql = """
            SELECT
                t.id AS TicketTemplateId,
                t.code AS Code,
                t.name AS Name,
                CASE
                    WHEN t.type = 0 THEN 'Standard'
                    WHEN t.type = 1 THEN 'Promo'
                    WHEN t.type = 2 THEN 'Free'
                    WHEN t.type = 3 THEN 'Vip'
                    WHEN t.type = 4 THEN 'Event'
                    ELSE 'Standard'
                END AS Type,
                t.price AS Price,
                t.is_active AS IsActive
            FROM gaming.draw_allowed_ticket_templates a
            INNER JOIN gaming.ticket_templates t ON t.id = a.ticket_template_id
            WHERE a.tenant_id = @TenantId
              AND a.draw_id = @DrawId
            ORDER BY t.code ASC
            """;

        using System.Data.IDbConnection connection = dbConnectionFactory.GetOpenConnection();

        IEnumerable<DrawAllowedTicketTemplateDto> items = await connection.QueryAsync<DrawAllowedTicketTemplateDto>(
            sql,
            new { tenantContext.TenantId, request.DrawId });

        return items.ToList();
    }
}
