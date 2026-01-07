using SharedKernel;

namespace Domain.Users;

public sealed class UserTenant : Entity
{
    private UserTenant()
    {
    }

    private UserTenant(Guid userId, Guid tenantId)
        : base(Guid.NewGuid())
    {
        UserId = userId;
        TenantId = tenantId;
    }

    public Guid UserId { get; private set; }
    public Guid TenantId { get; private set; }

    public User User { get; private set; } = null!;

    public static UserTenant Create(Guid userId, Guid tenantId)
    {
        return new UserTenant(userId, tenantId);
    }
}
