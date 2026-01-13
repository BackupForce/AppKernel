using SharedKernel;

namespace Domain.Tenants;

public static class TenantErrors
{
    public static Error NotFound(Guid tenantId) => Error.NotFound(
        "Tenant.NotFound",
        $"找不到 Tenant: {tenantId}");
}
