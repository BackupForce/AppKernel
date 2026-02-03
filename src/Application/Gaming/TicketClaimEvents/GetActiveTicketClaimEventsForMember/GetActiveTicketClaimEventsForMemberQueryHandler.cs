using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Time;
using Dapper;
using Domain.Gaming.Shared;
using Domain.Gaming.TicketClaimEvents;
using SharedKernel;

namespace Application.Gaming.TicketClaimEvents.GetActiveTicketClaimEventsForMember;

internal sealed class GetActiveTicketClaimEventsForMemberQueryHandler(
    IDbConnectionFactory dbConnectionFactory,
    ITenantContext tenantContext,
    IDateTimeProvider dateTimeProvider)
    : IQueryHandler<GetActiveTicketClaimEventsForMemberQuery, IReadOnlyCollection<TicketClaimEventActiveListItemDto>>
{
    private sealed record EventRow(
        Guid Id,
        string Name,
        DateTime StartsAtUtc,
        DateTime EndsAtUtc,
        int PerMemberQuota,
        string ScopeType,
        Guid ScopeId,
        Guid? TicketTemplateId);

    private sealed record MemberCountRow(Guid EventId, int ClaimedCount);

    public async Task<Result<IReadOnlyCollection<TicketClaimEventActiveListItemDto>>> Handle(
        GetActiveTicketClaimEventsForMemberQuery request,
        CancellationToken cancellationToken)
    {
        if (request.TenantId != tenantContext.TenantId)
        {
            return Result.Failure<IReadOnlyCollection<TicketClaimEventActiveListItemDto>>(
                GamingErrors.TicketClaimEventTenantMismatch);
        }

        DateTime nowUtc = request.NowUtc ?? dateTimeProvider.UtcNow;

        const string eventSql = """
            SELECT
                e.id AS Id,
                e.name AS Name,
                e.starts_at_utc AS StartsAtUtc,
                e.ends_at_utc AS EndsAtUtc,
                e.per_member_quota AS PerMemberQuota,
                CASE
                    WHEN e.scope_type = 0 THEN 'SingleDraw'
                    WHEN e.scope_type = 1 THEN 'SingleDrawGroup'
                    ELSE 'SingleDraw'
                END AS ScopeType,
                e.scope_id AS ScopeId,
                e.ticket_template_id AS TicketTemplateId
            FROM gaming.ticket_claim_events e
            WHERE e.tenant_id = @TenantId
              AND e.status = @Status
              AND e.starts_at_utc <= @NowUtc
              AND @NowUtc < e.ends_at_utc
            ORDER BY e.starts_at_utc ASC, e.created_at_utc DESC
            """;

        using System.Data.IDbConnection connection = dbConnectionFactory.GetOpenConnection();

        IReadOnlyCollection<EventRow> events = (await connection.QueryAsync<EventRow>(new CommandDefinition(
            eventSql,
            new
            {
                request.TenantId,
                Status = TicketClaimEventStatus.Active,
                NowUtc = nowUtc
            },
            cancellationToken: cancellationToken))).ToList();

        if (events.Count == 0)
        {
            return Array.Empty<TicketClaimEventActiveListItemDto>();
        }

        Guid[] eventIds = events.Select(item => item.Id).ToArray();

        const string claimedSql = """
            SELECT
                c.event_id AS EventId,
                c.claimed_count AS ClaimedCount
            FROM gaming.ticket_claim_member_counters c
            WHERE c.member_id = @MemberId
              AND c.event_id = ANY(@EventIds)
            """;

        IReadOnlyCollection<MemberCountRow> counts = (await connection.QueryAsync<MemberCountRow>(new CommandDefinition(
            claimedSql,
            new
            {
                request.MemberId,
                EventIds = eventIds
            },
            cancellationToken: cancellationToken))).ToList();

        Dictionary<Guid, int> countMap = counts.ToDictionary(row => row.EventId, row => row.ClaimedCount);

        List<TicketClaimEventActiveListItemDto> results = new(events.Count);
        foreach (EventRow row in events)
        {
            int claimedCount = countMap.TryGetValue(row.Id, out int value) ? value : 0;
            bool canParticipate = row.PerMemberQuota <= 0 || claimedCount < row.PerMemberQuota;

            results.Add(new TicketClaimEventActiveListItemDto(
                row.Id,
                row.Name,
                row.StartsAtUtc,
                row.EndsAtUtc,
                row.PerMemberQuota,
                claimedCount,
                canParticipate,
                row.ScopeType,
                row.ScopeId,
                row.TicketTemplateId));
        }

        return results;
    }
}
