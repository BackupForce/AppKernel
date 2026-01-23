using SharedKernel;

namespace Domain.Gaming.Tickets;

/// <summary>
/// 票券後台操作的 Idempotency 記錄，避免重複發券或重複下注。
/// </summary>
public sealed class TicketIdempotencyRecord : Entity
{
    private TicketIdempotencyRecord(
        Guid id,
        Guid tenantId,
        string idempotencyKey,
        string operation,
        string requestHash,
        string responsePayload,
        DateTime createdAtUtc) : base(id)
    {
        TenantId = tenantId;
        IdempotencyKey = idempotencyKey;
        Operation = operation;
        RequestHash = requestHash;
        ResponsePayload = responsePayload;
        CreatedAtUtc = createdAtUtc;
    }

    private TicketIdempotencyRecord()
    {
    }

    public Guid TenantId { get; private set; }

    public string IdempotencyKey { get; private set; } = string.Empty;

    public string Operation { get; private set; } = string.Empty;

    public string RequestHash { get; private set; } = string.Empty;

    public string ResponsePayload { get; private set; } = string.Empty;

    public DateTime CreatedAtUtc { get; private set; }

    public static TicketIdempotencyRecord Create(
        Guid tenantId,
        string idempotencyKey,
        string operation,
        string requestHash,
        string responsePayload,
        DateTime createdAtUtc)
    {
        return new TicketIdempotencyRecord(
            Guid.NewGuid(),
            tenantId,
            idempotencyKey.Trim(),
            operation.Trim(),
            requestHash,
            responsePayload,
            createdAtUtc);
    }
}
