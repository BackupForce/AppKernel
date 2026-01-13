using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Gaming.Dtos;
using Dapper;
using SharedKernel;

namespace Application.Gaming.Draws.AllowedTicketTemplates.Get;

internal sealed class GetDrawAllowedTicketTemplatesQueryHandler(
    IDbConnectionFactory dbConnectionFactory,
    ITenantContext tenantContext) : IQueryHandler<GetDrawAllowedTicketTemplatesQuery, IReadOnlyCollection<DrawAllowedTicketTemplateDto>>
{
    public async Task<Result<IReadOnlyCollection<DrawAllowedTicketTemplateDto>>> Handle(
        GetDrawAllowedTicketTemplatesQuery request,
        CancellationToken cancellationToken)
    {
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
            FROM gaming_draw_allowed_ticket_templates a
            INNER JOIN gaming_ticket_templates t ON t.id = a.ticket_template_id
            WHERE a.tenant_id = @TenantId
              AND a.draw_id = @DrawId
            ORDER BY t.code ASC
            """;

        using System.Data.IDbConnection connection = dbConnectionFactory.GetOpenConnection();

        IEnumerable<DrawAllowedTicketTemplateDto> items = await connection.QueryAsync<DrawAllowedTicketTemplateDto>(
            sql,
            new { TenantId = tenantContext.TenantId, DrawId = request.DrawId });

        return items.ToList();
    }
}
