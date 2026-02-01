using System.Data;
using System.Globalization;
using System.Text;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Gaming.Dtos;
using Dapper;
using Domain.Gaming.Shared;
using SharedKernel;

namespace Application.Gaming.TicketClaimEvents.List;

internal sealed class ListTicketClaimEventsQueryHandler(
    IDbConnectionFactory dbConnectionFactory,
    ITenantContext tenantContext) : IQueryHandler<ListTicketClaimEventsQuery, PagedResult<TicketClaimEventSummaryDto>>
{
    public async Task<Result<PagedResult<TicketClaimEventSummaryDto>>> Handle(
        ListTicketClaimEventsQuery request,
        CancellationToken cancellationToken)
    {
        if (request.TenantId != tenantContext.TenantId)
        {
            return Result.Failure<PagedResult<TicketClaimEventSummaryDto>>(GamingErrors.TicketClaimEventTenantMismatch);
        }

        var builder = new StringBuilder(
            """
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
                e.updated_at_utc AS UpdatedAtUtc
            FROM gaming.ticket_claim_events e
            WHERE e.tenant_id = @TenantId
            """);

        var parameters = new DynamicParameters();
        parameters.Add("TenantId", request.TenantId);

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            if (!Enum.TryParse(request.Status.Trim(), true, out Domain.Gaming.TicketClaimEvents.TicketClaimEventStatus status))
            {
                return Result.Failure<PagedResult<TicketClaimEventSummaryDto>>(GamingErrors.TicketClaimEventStatusInvalid);
            }

            builder.Append(" AND e.status = @Status");
            parameters.Add("Status", (int)status);
        }

        if (request.StartsFromUtc.HasValue)
        {
            builder.Append(" AND e.starts_at_utc >= @StartsFromUtc");
            parameters.Add("StartsFromUtc", request.StartsFromUtc);
        }

        if (request.StartsToUtc.HasValue)
        {
            builder.Append(" AND e.starts_at_utc <= @StartsToUtc");
            parameters.Add("StartsToUtc", request.StartsToUtc);
        }

        if (request.EndsFromUtc.HasValue)
        {
            builder.Append(" AND e.ends_at_utc >= @EndsFromUtc");
            parameters.Add("EndsFromUtc", request.EndsFromUtc);
        }

        if (request.EndsToUtc.HasValue)
        {
            builder.Append(" AND e.ends_at_utc <= @EndsToUtc");
            parameters.Add("EndsToUtc", request.EndsToUtc);
        }

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            builder.Append(" AND e.name ILIKE @Keyword");
            parameters.Add("Keyword", $"%{request.Keyword.Trim()}%");
        }

        const string countSql = "SELECT COUNT(*) FROM ({0}) AS counted";
        string baseSql = builder.ToString();
        string finalSql = $"{baseSql} ORDER BY e.created_at_utc DESC LIMIT @PageSize OFFSET @Offset";

        parameters.Add("PageSize", request.PageSize);
        parameters.Add("Offset", (request.Page - 1) * request.PageSize);

        using IDbConnection connection = dbConnectionFactory.GetOpenConnection();

        IEnumerable<TicketClaimEventSummaryDto> items = await connection.QueryAsync<TicketClaimEventSummaryDto>(
            finalSql,
            parameters);
        int totalCount = await connection.ExecuteScalarAsync<int>(
            string.Format(CultureInfo.InvariantCulture, countSql, baseSql),
            parameters);

        return PagedResult<TicketClaimEventSummaryDto>.Create(items, totalCount, request.Page, request.PageSize);
    }
}
