using Application.Abstractions.Data;
using Domain.Admin.OperationLogs;
using Domain.Gaming.DrawGroups.Events;
using MediatR;

namespace Application.Admin.OperationLogs;

internal sealed class DrawGroupOperationLogHandler(
    IAdminOperationLogRepository adminOperationLogRepository,
    IUnitOfWork unitOfWork)
    : INotificationHandler<DrawGroupEnabledDomainEvent>,
      INotificationHandler<DrawGroupDisabledDomainEvent>
{
    public async Task Handle(DrawGroupEnabledDomainEvent notification, CancellationToken cancellationToken)
    {
        await HandleAsync(
            nameof(DrawGroupEnabledDomainEvent),
            notification.TenantId,
            notification.DrawGroupId,
            "Enable",
            notification.OperatorUserId,
            notification.OccurredAtUtc,
            notification.Reason,
            cancellationToken);
    }

    public async Task Handle(DrawGroupDisabledDomainEvent notification, CancellationToken cancellationToken)
    {
        await HandleAsync(
            nameof(DrawGroupDisabledDomainEvent),
            notification.TenantId,
            notification.DrawGroupId,
            "Disable",
            notification.OperatorUserId,
            notification.OccurredAtUtc,
            notification.Reason,
            cancellationToken);
    }

    private async Task HandleAsync(
        string eventName,
        Guid tenantId,
        Guid drawGroupId,
        string action,
        Guid operatorUserId,
        DateTime occurredAtUtc,
        string? reason,
        CancellationToken cancellationToken)
    {
        string dedupKey = AdminOperationLog.BuildDedupKey(eventName, tenantId, drawGroupId, occurredAtUtc);
        if (await adminOperationLogRepository.ExistsByDedupKeyAsync(dedupKey, cancellationToken))
        {
            return;
        }

        AdminOperationLog log = AdminOperationLog.Create(
            tenantId,
            "DrawGroup",
            drawGroupId,
            action,
            operatorUserId,
            operatorType: null,
            reason,
            metadataJson: null,
            occurredAtUtc,
            dedupKey);

        await adminOperationLogRepository.AddAsync(log, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
