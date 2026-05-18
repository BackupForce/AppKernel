namespace Application.Abstractions.Authentication;

public interface ITenantContext
{
    Guid TenantId { get; }

    bool TryGetTenantId(out Guid tenantId);
}
