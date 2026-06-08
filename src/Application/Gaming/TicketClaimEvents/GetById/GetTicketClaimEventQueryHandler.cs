using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Gaming.Dtos;
using Dapper;
using Domain.Gaming.Shared;
using SharedKernel;

namespace Application.Gaming.TicketClaimEvents.GetById;

internal sealed class GetTicketClaimEventQueryHandler(
    IDbConnectionFactory dbConnectionFactory,
    ITenantContext tenantContext) : IQueryHandler<GetTicketClaimEventQuery, TicketClaimEventDetailDto>
{
    public async Task<Result<TicketClaimEventDetailDto>> Handle(GetTicketClaimEventQuery request, CancellationToken cancellationToken)
    {
        if (request.TenantId != tenantContext.TenantId)
        {
            return Result.Failure<TicketClaimEventDetailDto>(GamingErrors.TicketClaimEventTenantMismatch);
        }

        const string sql = """
            SELECT
                e.id AS Id,
                e.name AS Name,
                CASE
                    WHEN e.status = 0 THEN 'Draft'
                    WHEN e.status = 1 THEN 'Active'
                    WHEN e.status = 2 THEN 'Ended'
                    WHEN e.status = 3 THEN 'SoldOut'
                    WHEN e.status = 4 THEN 'Disabled'
                    ELSE 'Draft'
                END AS Status,
                e.starts_at_utc AS StartsAtUtc,
                e.ends_at_utc AS EndsAtUtc,
                e.total_quota AS TotalQuota,
                e.total_claimed AS TotalClaimed,
                e.per_member_quota AS PerMemberQuota,
                CASE
                    WHEN e.scope_type = 0 THEN 'SingleDraw'
                    WHEN e.scope_type = 1 THEN 'SingleDrawGroup'
                    ELSE 'SingleDraw'
                END AS ScopeType,
                e.scope_id AS ScopeId,
                e.ticket_template_id AS TicketTemplateId,
                e.created_at_utc AS CreatedAtUtc,
                e.updated_at_utc AS UpdatedAtUtc,
                COALESCE((
                    SELECT array_agg(r.tag_id ORDER BY r.tag_id)
                    FROM gaming.ticket_claim_event_tag_rules r
                    WHERE r.event_id = e.id
                ), ARRAY[]::uuid[]) AS AllowedTagIds
            FROM gaming.ticket_claim_events e
            WHERE e.tenant_id = @TenantId AND e.id = @EventId
            """;

        using System.Data.IDbConnection connection = dbConnectionFactory.GetOpenConnection();

        dynamic? detail = await connection.QueryFirstOrDefaultAsync(
            sql,
            new { request.TenantId, request.EventId });

        if (detail is null)
        {
            return Result.Failure<TicketClaimEventDetailDto>(GamingErrors.TicketClaimEventNotFound);
        }

        return ToDto(detail);
    }

    private static TicketClaimEventDetailDto ToDto(dynamic row)
    {
        return new TicketClaimEventDetailDto(
            row.Id,
            row.Name,
            row.Status,
            row.StartsAtUtc,
            row.EndsAtUtc,
            row.TotalQuota,
            row.TotalClaimed,
            row.PerMemberQuota,
            row.ScopeType,
            row.ScopeId,
            row.TicketTemplateId,
            row.CreatedAtUtc,
            row.UpdatedAtUtc,
            ToGuidArray(row.AllowedTagIds));
    }

    private static Guid[] ToGuidArray(object? values)
    {
        return values is Array array ? array.Cast<Guid>().ToArray() : Array.Empty<Guid>();
    }
}
