using System.Data;
using Application.Abstractions.Data;
using Domain.Gaming.Draws.Events;
using Dapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Gaming.DrawGroups.Outbox;

internal sealed class UpdateDrawGroupGrantWindowOnDrawChangedHandler(
    IDbConnectionFactory dbConnectionFactory,
    ILogger<UpdateDrawGroupGrantWindowOnDrawChangedHandler> logger)
    : INotificationHandler<DrawSalesCloseAtChangedDomainEvent>,
      INotificationHandler<DrawManuallyClosedDomainEvent>
{
    public Task Handle(DrawSalesCloseAtChangedDomainEvent notification, CancellationToken cancellationToken)
    {
        return HandleAsync(notification.TenantId, notification.DrawId, cancellationToken);
    }

    public Task Handle(DrawManuallyClosedDomainEvent notification, CancellationToken cancellationToken)
    {
        return HandleAsync(notification.TenantId, notification.DrawId, cancellationToken);
    }

    private async Task HandleAsync(Guid tenantId, Guid drawId, CancellationToken cancellationToken)
    {
        using IDbConnection connection = dbConnectionFactory.GetOpenConnection();

        const string groupsSql = """
        SELECT DISTINCT draw_group_id
        FROM gaming.draw_group_draws
        WHERE tenant_id = @TenantId
          AND draw_id = @DrawId
        """;

        List<Guid> drawGroupIds = (await connection.QueryAsync<Guid>(new CommandDefinition(
            groupsSql,
            new { TenantId = tenantId, DrawId = drawId },
            cancellationToken: cancellationToken))).ToList();

        if (drawGroupIds.Count == 0)
        {
            logger.LogInformation(
                "No draw groups found for draw {DrawId} in tenant {TenantId}",
                drawId,
                tenantId);
            return;
        }

        const string updateSql = """
        WITH affected AS (
            SELECT UNNEST(@DrawGroupIds) AS draw_group_id
        ),
        windows AS (
            SELECT
                a.draw_group_id AS draw_group_id,
                MIN(d.sales_open_at) AS grant_open_at_utc,
                MAX(COALESCE(d.manual_close_at, d.sales_close_at)) AS grant_close_at_utc
            FROM affected a
            LEFT JOIN gaming.draw_group_draws dgd
                ON dgd.tenant_id = @TenantId
                AND dgd.draw_group_id = a.draw_group_id
            LEFT JOIN gaming.draws d
                ON d.id = dgd.draw_id
                AND d.tenant_id = dgd.tenant_id
            GROUP BY a.draw_group_id
        )
        UPDATE gaming.draw_groups dg
        SET grant_open_at_utc = w.grant_open_at_utc,
            grant_close_at_utc = w.grant_close_at_utc
        FROM windows w
        WHERE dg.id = w.draw_group_id
          AND dg.tenant_id = @TenantId
        """;

        await connection.ExecuteAsync(new CommandDefinition(
            updateSql,
            new
            {
                TenantId = tenantId,
                DrawGroupIds = drawGroupIds.ToArray()
            },
            cancellationToken: cancellationToken));

        logger.LogInformation(
            "Updated draw group grant windows for draw {DrawId} in tenant {TenantId}. Groups affected: {GroupCount}",
            drawId,
            tenantId,
            drawGroupIds.Count);
    }
}
