using System.Data;
using System.Text.Json;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Dapper;
using Domain.Gaming.Tickets;
using Domain.Users;
using Microsoft.Extensions.Caching.Distributed;
using SharedKernel;

namespace Application.Admin.Dashboard;

internal sealed class GetAdminDashboardMemberMetricsQueryHandler(
    IDistributedCache cache,
    IDbConnectionFactory dbConnectionFactory,
    IDateTimeProvider dateTimeProvider,
    IUserContext userContext)
    : IQueryHandler<GetAdminDashboardMemberMetricsQuery, AdminDashboardMemberMetricsDto>
{
    private const int OnlineWindowMinutes = 5;

    public async Task<Result<AdminDashboardMemberMetricsDto>> Handle(
        GetAdminDashboardMemberMetricsQuery request,
        CancellationToken cancellationToken)
    {
        Guid tenantId = userContext.TenantId
            ?? throw new ApplicationException("TenantId is required.");

        string cacheKey = $"admin:dashboard:member-metrics:{tenantId}";

        string? cached = await cache.GetStringAsync(cacheKey, cancellationToken);
        if (!string.IsNullOrWhiteSpace(cached))
        {
            AdminDashboardMemberMetricsDto? cachedDto = JsonSerializer.Deserialize<AdminDashboardMemberMetricsDto>(cached);
            if (cachedDto is not null)
            {
                return cachedDto;
            }
        }

        DateTime nowUtc = dateTimeProvider.UtcNow;
        DateTime todayStartUtc = nowUtc.Date;
        DateTime todayEndUtc = todayStartUtc.AddDays(1);

        int diff = ((int)nowUtc.DayOfWeek + 6) % 7;
        DateTime weekStartUtc = nowUtc.Date.AddDays(-diff);
        DateTime weekEndUtc = weekStartUtc.AddDays(7);

        DateTime onlineWindowEndUtc = nowUtc;
        DateTime onlineWindowStartUtc = nowUtc.AddMinutes(-OnlineWindowMinutes);

        using IDbConnection connection = dbConnectionFactory.GetOpenConnection();

        MetricCounts registered = await connection.QuerySingleAsync<MetricCounts>(
            new CommandDefinition(
                """
                SELECT
                    COUNT(*) FILTER (
                        WHERE m.created_at >= @todayStartUtc AND m.created_at < @todayEndUtc
                    ) AS Today,
                    COUNT(*) FILTER (
                        WHERE m.created_at >= @weekStartUtc AND m.created_at < @weekEndUtc
                    ) AS Week
                FROM members m
                JOIN users u ON u.id = m.user_id
                WHERE m.tenant_id = @tenantId
                  AND u.type = @memberType;
                """,
                new
                {
                    tenantId,
                    memberType = (int)UserType.Member,
                    todayStartUtc,
                    todayEndUtc,
                    weekStartUtc,
                    weekEndUtc
                },
                cancellationToken: cancellationToken));

        int onlineMembers = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(
                """
                SELECT COUNT(DISTINCT s.user_id)
                FROM auth_sessions s
                JOIN users u ON u.id = s.user_id
                WHERE s.tenant_id = @tenantId
                  AND s.last_used_at_utc >= @onlineWindowStartUtc
                  AND u.type = @memberType;
                """,
                new
                {
                    tenantId,
                    onlineWindowStartUtc,
                    memberType = (int)UserType.Member
                },
                cancellationToken: cancellationToken));

        MetricCounts activeCounts = await connection.QuerySingleAsync<MetricCounts>(
            new CommandDefinition(
                """
                SELECT
                    COUNT(DISTINCT user_id) FILTER (
                        WHERE occurred_at_utc >= @todayStartUtc AND occurred_at_utc < @todayEndUtc
                    ) AS Today,
                    COUNT(DISTINCT user_id) FILTER (
                        WHERE occurred_at_utc >= @weekStartUtc AND occurred_at_utc < @weekEndUtc
                    ) AS Week
                FROM (
                    SELECT m.user_id, r.claimed_at_utc AS occurred_at_utc
                    FROM gaming.ticket_claim_records r
                    JOIN members m ON m.id = r.member_id
                    JOIN users u ON u.id = m.user_id
                    WHERE r.tenant_id = @tenantId
                      AND r.claimed_at_utc >= @weekStartUtc
                      AND r.claimed_at_utc < @weekEndUtc
                      AND u.type = @memberType

                    UNION ALL

                    SELECT m.user_id, t.submitted_at_utc AS occurred_at_utc
                    FROM gaming.tickets t
                    JOIN members m ON m.id = t.member_id
                    JOIN users u ON u.id = m.user_id
                    WHERE t.tenant_id = @tenantId
                      AND t.submission_status = @submittedStatus
                      AND t.submitted_at_utc >= @weekStartUtc
                      AND t.submitted_at_utc < @weekEndUtc
                      AND u.type = @memberType

                    UNION ALL

                    SELECT m.user_id, td.redeemed_at_utc AS occurred_at_utc
                    FROM gaming.ticket_draws td
                    JOIN gaming.tickets t ON t.id = td.ticket_id
                    JOIN members m ON m.id = t.member_id
                    JOIN users u ON u.id = m.user_id
                    WHERE td.tenant_id = @tenantId
                      AND td.redeemed_at_utc >= @weekStartUtc
                      AND td.redeemed_at_utc < @weekEndUtc
                      AND u.type = @memberType
                ) actions
                WHERE user_id IS NOT NULL;
                """,
                new
                {
                    tenantId,
                    todayStartUtc,
                    todayEndUtc,
                    weekStartUtc,
                    weekEndUtc,
                    memberType = (int)UserType.Member,
                    submittedStatus = (int)TicketSubmissionStatus.Submitted
                },
                cancellationToken: cancellationToken));

        var dto = new AdminDashboardMemberMetricsDto(
            registered.Today,
            registered.Week,
            onlineMembers,
            activeCounts.Today,
            activeCounts.Week,
            todayStartUtc,
            todayEndUtc,
            weekStartUtc,
            weekEndUtc,
            onlineWindowStartUtc,
            onlineWindowEndUtc);

        string serialized = JsonSerializer.Serialize(dto);
        await cache.SetStringAsync(
            cacheKey,
            serialized,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60)
            },
            cancellationToken);

        return dto;
    }

    private sealed record MetricCounts(int Today, int Week);
}
