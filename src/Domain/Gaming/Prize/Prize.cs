using SharedKernel;

namespace Domain.Gaming;

/// <summary>
/// 獎品定義，描述可兌換的品項與成本。
/// </summary>
/// <remarks>
/// Cost 用於成本統計與報表，必須配合 RedeemRecord 的成本快照。
/// </remarks>
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

    /// <summary>
    /// 租戶識別。
    /// </summary>
    public Guid TenantId { get; private set; }

    /// <summary>
    /// 獎品名稱。
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// 獎品描述。
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// 成本（decimal 以保留精度），用於成本統計與財務報表。
    /// </summary>
    public decimal Cost { get; private set; }

    /// <summary>
    /// 是否啟用，停用後不得再作為新的結算目標。
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// 建立時間（UTC）。
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// 更新時間（UTC）。
    /// </summary>
    public DateTime UpdatedAt { get; private set; }

    /// <summary>
    /// 建立獎品，驗證名稱與成本。
    /// </summary>
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

    /// <summary>
    /// 更新獎品內容，成本變更需搭配 RedeemRecord 成本快照避免影響歷史報表。
    /// </summary>
    public void Update(string name, string? description, decimal cost, DateTime utcNow)
    {
        Name = name;
        Description = description;
        Cost = cost;
        UpdatedAt = utcNow;
    }

    /// <summary>
    /// 啟用獎品，使其可被規則引用。
    /// </summary>
    public void Activate(DateTime utcNow)
    {
        IsActive = true;
        UpdatedAt = utcNow;
    }

    /// <summary>
    /// 停用獎品，避免新結算指向此獎品。
    /// </summary>
    public void Deactivate(DateTime utcNow)
    {
        IsActive = false;
        UpdatedAt = utcNow;
    }
}
