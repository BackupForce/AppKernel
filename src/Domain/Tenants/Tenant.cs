using SharedKernel;

namespace Domain.Tenants;

public sealed class Tenant : Entity
{
    private Tenant()
    {
    }

    private Tenant(Guid id, string code, string name)
        : base(id)
    {
        Code = code;
        Name = name;
    }

    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;

    public static Tenant Create(Guid id, string code, string name)
    {
        return new Tenant(id, code, name);
    }
}
