using Application.Abstractions.Data;
using Application.Abstractions.Gaming;
using Application.Abstractions.Messaging;
using Application.Abstractions.Time;
using Application.Gaming.Dtos;
using Domain.Gaming.Catalog;
using Domain.Gaming.Rules;
using Domain.Gaming.Tickets;
using Dapper;
using SharedKernel;

namespace Application.Gaming.Tickets.AvailableForBet;

internal sealed class GetAvailableTicketsForBetQueryHandler(
    IDbConnectionFactory dbConnectionFactory,
    IEntitlementChecker entitlementChecker,
    IDateTimeProvider dateTimeProvider)
    : IQueryHandler<GetAvailableTicketsForBetQuery, AvailableTicketsResponse>
{
    private const int DefaultLimit = 200;
    private const int MaxLimit = 500;

    private sealed record TicketRow(
        Guid TicketId,
        string GameCode,
        string? TicketPlayTypeCode,
        Guid? DrawId,
        Guid? DrawGroupId,
        string? DrawCode,
        string? DrawGroupName,
        DateTime? SalesCloseAtUtc,
        DateTime ExpiresAtUtc,
        string? DrawTemplateName);

    private sealed record DrawPlayTypeRow(Guid DrawId, string PlayTypeCode);

    public async Task<Result<AvailableTicketsResponse>> Handle(
        GetAvailableTicketsForBetQuery request,
        CancellationToken cancellationToken)
    {
        int limit = request.Limit switch
        {
            > 0 and <= MaxLimit => request.Limit.Value,
            > MaxLimit => MaxLimit,
            _ => DefaultLimit
        };

        const string sql = """
            SELECT
                t.id AS TicketId,
                t.game_code AS GameCode,
                t.play_type_code AS TicketPlayTypeCode,
                t.draw_id AS DrawId,
                t.draw_group_id AS DrawGroupId,
                d.draw_code AS DrawCode,
                dg.name AS DrawGroupName,
                d.sales_close_at AS SalesCloseAtUtc,
                COALESCE(d.sales_close_at, dg.grant_close_at_utc, t.created_at) AS ExpiresAtUtc,
                COALESCE(dt.name, dgt.template_name) AS DrawTemplateName
            FROM gaming.tickets t
            LEFT JOIN gaming.draws d ON d.id = t.draw_id
            LEFT JOIN gaming.draw_templates dt ON dt.id = d.source_template_id
            LEFT JOIN gaming.draw_groups dg ON dg.id = t.draw_group_id
            LEFT JOIN LATERAL (
                SELECT dtg.name AS template_name
                FROM gaming.draw_group_draws ggd
                JOIN gaming.draws dg_draw ON dg_draw.id = ggd.draw_id
                LEFT JOIN gaming.draw_templates dtg ON dtg.id = dg_draw.source_template_id
                WHERE ggd.draw_group_id = dg.id
                ORDER BY dg_draw.created_at ASC
                LIMIT 1
            ) dgt ON TRUE
            WHERE t.tenant_id = @TenantId
              AND t.member_id = @MemberId
              AND t.submission_status = @SubmissionStatus
              AND (@DrawId IS NULL OR t.draw_id = @DrawId)
              AND NOT EXISTS (
                  SELECT 1
                  FROM gaming.ticket_lines l
                  WHERE l.ticket_id = t.id
              )
              AND (
                  t.draw_id IS NULL
                  OR (
                      d.is_manually_closed = FALSE
                      AND d.sales_close_at > @NowUtc
                  )
              )
            ORDER BY COALESCE(d.sales_close_at, t.created_at) ASC, t.created_at DESC
            LIMIT @Limit
            """;

        using System.Data.IDbConnection connection = dbConnectionFactory.GetOpenConnection();

        IEnumerable<TicketRow> rows = await connection.QueryAsync<TicketRow>(new CommandDefinition(
            sql,
            new
            {
                request.TenantId,
                request.MemberId,
                SubmissionStatus = TicketSubmissionStatus.NotSubmitted,
                NowUtc = dateTimeProvider.UtcNow,
                request.DrawId,
                Limit = limit
            },
            cancellationToken: cancellationToken));

        List<TicketRow> ticketRows = rows.ToList();
        if (ticketRows.Count == 0)
        {
            return new AvailableTicketsResponse(Array.Empty<AvailableTicketItemDto>());
        }

        HashSet<Guid> drawIds = ticketRows
            .Where(row => row.DrawId.HasValue)
            .Select(row => row.DrawId!.Value)
            .ToHashSet();

        Dictionary<Guid, HashSet<string>> drawPlayTypes = await LoadDrawPlayTypesAsync(
            connection,
            request.TenantId,
            drawIds,
            cancellationToken);

        PlayRuleRegistry registry = PlayRuleRegistry.CreateDefault();
        TenantEntitlementsDto entitlements = await entitlementChecker.GetTenantEntitlementsAsync(
            request.TenantId,
            cancellationToken);

        Dictionary<string, HashSet<string>> entitlementMap = entitlements.EnabledPlayTypesByGame
            .ToDictionary(
                pair => pair.Key,
                pair => new HashSet<string>(pair.Value, StringComparer.OrdinalIgnoreCase),
                StringComparer.OrdinalIgnoreCase);

        Dictionary<string, List<string>> gamePlayTypes = ticketRows
            .Select(row => row.GameCode)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                gameCode => gameCode,
                gameCode =>
                {
                    IReadOnlyCollection<PlayTypeCode> allowed = registry.GetAllowedPlayTypes(new GameCode(gameCode));
                    if (!entitlementMap.TryGetValue(gameCode, out HashSet<string>? entitled))
                    {
                        return new List<string>();
                    }

                    return allowed
                        .Select(code => code.Value)
                        .Where(code => entitled.Contains(code))
                        .OrderBy(code => code, StringComparer.Ordinal)
                        .ToList();
                },
                StringComparer.OrdinalIgnoreCase);

        List<AvailableTicketItemDto> items = new List<AvailableTicketItemDto>();
        foreach (TicketRow row in ticketRows)
        {
            List<string> availableCodes = gamePlayTypes.TryGetValue(row.GameCode, out List<string>? codes)
                ? new List<string>(codes)
                : new List<string>();

            if (row.DrawId.HasValue && drawPlayTypes.TryGetValue(row.DrawId.Value, out HashSet<string>? drawCodes))
            {
                availableCodes = availableCodes
                    .Where(code => drawCodes.Contains(code))
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(row.TicketPlayTypeCode))
            {
                string normalized = PlayTypeCode.Normalize(row.TicketPlayTypeCode);
                availableCodes = availableCodes
                    .Where(code => string.Equals(code, normalized, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (availableCodes.Count == 0)
            {
                continue;
            }

            string displayText = BuildDisplayText(row);
            string scopeDisplayName = ResolveScopeDisplayName(row);
            List<TicketPlayTypeDto> playTypes = availableCodes
                .OrderBy(code => code, StringComparer.Ordinal)
                .Select(code => new TicketPlayTypeDto(code, code))
                .ToList();

            items.Add(new AvailableTicketItemDto(
                row.TicketId,
                displayText,
                scopeDisplayName,
                row.GameCode,
                row.DrawId,
                row.DrawGroupId,
                row.SalesCloseAtUtc,
                row.ExpiresAtUtc,
                row.DrawTemplateName,
                playTypes));
        }

        return new AvailableTicketsResponse(items);
    }

    private static string BuildDisplayText(TicketRow row)
    {
        List<string> segments = new List<string>
        {
            $"Ticket {row.TicketId:N}",
            row.GameCode
        };

        if (!string.IsNullOrWhiteSpace(row.TicketPlayTypeCode))
        {
            segments.Add(row.TicketPlayTypeCode);
        }

        if (row.SalesCloseAtUtc.HasValue)
        {
            segments.Add($"Close {row.SalesCloseAtUtc:O}");
        }

        return string.Join(" | ", segments);
    }

    private static string ResolveScopeDisplayName(TicketRow row)
    {
        if (row.DrawId.HasValue)
        {
            return row.DrawCode ?? string.Empty;
        }

        if (row.DrawGroupId.HasValue)
        {
            return row.DrawGroupName ?? string.Empty;
        }

        return string.Empty;
    }

    private static async Task<Dictionary<Guid, HashSet<string>>> LoadDrawPlayTypesAsync(
        System.Data.IDbConnection connection,
        Guid tenantId,
        HashSet<Guid> drawIds,
        CancellationToken cancellationToken)
    {
        if (drawIds.Count == 0)
        {
            return new Dictionary<Guid, HashSet<string>>();
        }

        const string sql = """
        SELECT
            p.draw_id AS DrawId,
            p.play_type_code AS PlayTypeCode
        FROM gaming.draw_enabled_play_types p
        WHERE p.tenant_id = @TenantId
          AND p.draw_id = ANY(@DrawIds)
        """;

        IEnumerable<DrawPlayTypeRow> rows = await connection.QueryAsync<DrawPlayTypeRow>(new CommandDefinition(
            sql,
            new
            {
                TenantId = tenantId,
                DrawIds = drawIds.ToArray()
            },
            cancellationToken: cancellationToken));

        Dictionary<Guid, HashSet<string>> map = new(drawIds.Count);
        foreach (Guid drawId in drawIds)
        {
            map[drawId] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        foreach (DrawPlayTypeRow row in rows)
        {
            if (drawIds.Contains(row.DrawId))
            {
                map[row.DrawId].Add(row.PlayTypeCode);
            }
        }

        return map;
    }


}
