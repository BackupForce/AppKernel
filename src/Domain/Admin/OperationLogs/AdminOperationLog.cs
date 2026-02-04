using SharedKernel;

namespace Domain.Admin.OperationLogs;

public sealed class AdminOperationLog : Entity
{
    private AdminOperationLog(
        Guid id,
        Guid tenantId,
        string targetType,
        Guid targetId,
        string action,
        Guid? operatorUserId,
        string? operatorType,
        string? reason,
        string? metadataJson,
        DateTime occurredAtUtc,
        DateTime createdAtUtc,
        string? dedupKey) : base(id)
    {
        TenantId = tenantId;
        TargetType = targetType;
        TargetId = targetId;
        Action = action;
        OperatorUserId = operatorUserId;
        OperatorType = operatorType;
        Reason = reason;
        MetadataJson = metadataJson;
        OccurredAtUtc = occurredAtUtc;
        CreatedAtUtc = createdAtUtc;
        DedupKey = dedupKey;
    }

    private AdminOperationLog()
    {
    }

    public Guid TenantId { get; private set; }

    public string TargetType { get; private set; } = string.Empty;

    public Guid TargetId { get; private set; }

    public string Action { get; private set; } = string.Empty;

    public Guid? OperatorUserId { get; private set; }

    public string? OperatorType { get; private set; }

    public string? Reason { get; private set; }

    public string? MetadataJson { get; private set; }

    public DateTime OccurredAtUtc { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public string? DedupKey { get; private set; }

    public static AdminOperationLog Create(
        Guid tenantId,
        string targetType,
        Guid targetId,
        string action,
        Guid? operatorUserId,
        string? operatorType,
        string? reason,
        string? metadataJson,
        DateTime occurredAtUtc,
        string? dedupKey = null)
    {
        return new AdminOperationLog(
            Guid.NewGuid(),
            tenantId,
            targetType,
            targetId,
            action,
            operatorUserId,
            operatorType,
            reason,
            metadataJson,
            occurredAtUtc,
            occurredAtUtc,
            dedupKey);
    }

    public static string BuildDedupKey(
        string eventName,
        Guid tenantId,
        Guid targetId,
        DateTime occurredAtUtc)
    {
        return $"{eventName}:{tenantId}:{targetId}:{occurredAtUtc.Ticks}";
    }
}
