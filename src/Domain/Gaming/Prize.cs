using SharedKernel;

namespace Domain.Gaming;

public sealed class Prize : Entity
{
    private Prize(
        Guid id,
        Guid tenantId,
        string name,
        string? description,
        decimal cost,
        bool isActive,
        DateTime createdAt,
        DateTime updatedAt) : base(id)
    {
        TenantId = tenantId;
        Name = name;
        Description = description;
        Cost = cost;
        IsActive = isActive;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    private Prize()
    {
    }

    public Guid TenantId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public decimal Cost { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public static Result<Prize> Create(
        Guid tenantId,
        string name,
        string? description,
        decimal cost,
        DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Prize>(GamingErrors.PrizeNameRequired);
        }

        Prize prize = new Prize(Guid.NewGuid(), tenantId, name, description, cost, true, utcNow, utcNow);
        return prize;
    }

    public void Update(string name, string? description, decimal cost, DateTime utcNow)
    {
        Name = name;
        Description = description;
        Cost = cost;
        UpdatedAt = utcNow;
    }

    public void Activate(DateTime utcNow)
    {
        IsActive = true;
        UpdatedAt = utcNow;
    }

    public void Deactivate(DateTime utcNow)
    {
        IsActive = false;
        UpdatedAt = utcNow;
    }
}
