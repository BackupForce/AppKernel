using Domain.Gaming.Shared;
using SharedKernel;

namespace Domain.Gaming.TicketTemplates;

/// <summary>
/// 票種模板，用於描述可販售的票種、價格與限制。
/// </summary>
/// <remarks>
/// 拆出 TicketTemplate 可避免票券本身變動，並允許後台彈性管理票種。
/// </remarks>
public sealed class TicketTemplate : Entity
{
    private TicketTemplate(
        Guid id,
        Guid tenantId,
        string code,
        string name,
        TicketTemplateType type,
        decimal price,
        bool isActive,
        DateTime? validFrom,
        DateTime? validTo,
        int maxLinesPerTicket,
        DateTime createdAt,
        DateTime updatedAt) : base(id)
    {
        TenantId = tenantId;
        Code = code;
        Name = name;
        Type = type;
        Price = price;
        IsActive = isActive;
        ValidFrom = validFrom;
        ValidTo = validTo;
        MaxLinesPerTicket = maxLinesPerTicket;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    private TicketTemplate()
    {
    }

    /// <summary>
    /// 租戶識別。
    /// </summary>
    public Guid TenantId { get; private set; }

    /// <summary>
    /// 租戶內唯一代碼，方便後台辨識。
    /// </summary>
    public string Code { get; private set; } = string.Empty;

    /// <summary>
    /// 票種名稱。
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// 票種分類。
    /// </summary>
    public TicketTemplateType Type { get; private set; }

    /// <summary>
    /// 票價（點數）。
    /// </summary>
    public decimal Price { get; private set; }

    /// <summary>
    /// 是否啟用。
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// 生效起始時間（UTC）。
    /// </summary>
    public DateTime? ValidFrom { get; private set; }

    /// <summary>
    /// 生效結束時間（UTC）。
    /// </summary>
    public DateTime? ValidTo { get; private set; }

    /// <summary>
    /// 每張票最多注數。
    /// </summary>
    public int MaxLinesPerTicket { get; private set; }

    /// <summary>
    /// 建立時間（UTC）。
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// 更新時間（UTC）。
    /// </summary>
    public DateTime UpdatedAt { get; private set; }

    /// <summary>
    /// 建立票種模板。
    /// </summary>
    public static Result<TicketTemplate> Create(
        Guid tenantId,
        string code,
        string name,
        TicketTemplateType type,
        decimal price,
        DateTime? validFrom,
        DateTime? validTo,
        int maxLinesPerTicket,
        DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Result.Failure<TicketTemplate>(GamingErrors.TicketTemplateCodeRequired);
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<TicketTemplate>(GamingErrors.TicketTemplateNameRequired);
        }

        if (price < 0)
        {
            return Result.Failure<TicketTemplate>(GamingErrors.TicketTemplatePriceInvalid);
        }

        if (maxLinesPerTicket <= 0)
        {
            return Result.Failure<TicketTemplate>(GamingErrors.TicketTemplateMaxLinesInvalid);
        }

        if (validFrom.HasValue && validTo.HasValue && validFrom.Value >= validTo.Value)
        {
            return Result.Failure<TicketTemplate>(GamingErrors.TicketTemplateValidityInvalid);
        }

        var template = new TicketTemplate(
            Guid.NewGuid(),
            tenantId,
            code.Trim(),
            name.Trim(),
            type,
            price,
            true,
            validFrom,
            validTo,
            maxLinesPerTicket,
            utcNow,
            utcNow);

        return template;
    }

    /// <summary>
    /// 更新票種模板設定。
    /// </summary>
    public Result Update(
        string code,
        string name,
        TicketTemplateType type,
        decimal price,
        DateTime? validFrom,
        DateTime? validTo,
        int maxLinesPerTicket,
        DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Result.Failure(GamingErrors.TicketTemplateCodeRequired);
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure(GamingErrors.TicketTemplateNameRequired);
        }

        if (price < 0)
        {
            return Result.Failure(GamingErrors.TicketTemplatePriceInvalid);
        }

        if (maxLinesPerTicket <= 0)
        {
            return Result.Failure(GamingErrors.TicketTemplateMaxLinesInvalid);
        }

        if (validFrom.HasValue && validTo.HasValue && validFrom.Value >= validTo.Value)
        {
            return Result.Failure(GamingErrors.TicketTemplateValidityInvalid);
        }

        Code = code.Trim();
        Name = name.Trim();
        Type = type;
        Price = price;
        ValidFrom = validFrom;
        ValidTo = validTo;
        MaxLinesPerTicket = maxLinesPerTicket;
        UpdatedAt = utcNow;

        return Result.Success();
    }

    /// <summary>
    /// 啟用票種模板。
    /// </summary>
    public void Activate(DateTime utcNow)
    {
        IsActive = true;
        UpdatedAt = utcNow;
    }

    /// <summary>
    /// 停用票種模板。
    /// </summary>
    public void Deactivate(DateTime utcNow)
    {
        IsActive = false;
        UpdatedAt = utcNow;
    }

    /// <summary>
    /// 判斷模板是否在指定時間可用。
    /// </summary>
    public bool IsAvailable(DateTime utcNow)
    {
        if (!IsActive)
        {
            return false;
        }

        if (ValidFrom.HasValue && utcNow < ValidFrom.Value)
        {
            return false;
        }

        if (ValidTo.HasValue && utcNow > ValidTo.Value)
        {
            return false;
        }

        return true;
    }
}
