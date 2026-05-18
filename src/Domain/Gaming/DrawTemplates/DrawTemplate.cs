using Domain.Gaming.Catalog;
using Domain.Gaming.Rules;
using Domain.Gaming.Shared;
using SharedKernel;

namespace Domain.Gaming.DrawTemplates;

/// <summary>
/// 期數模板聚合根，負責設定玩法、獎項與允許票種。
/// </summary>
public sealed class DrawTemplate : Entity
{
    private readonly List<DrawTemplatePlayType> _playTypes = new();
    private readonly List<DrawTemplatePrizeTier> _prizeTiers = new();
    private readonly List<DrawTemplateAllowedTicketTemplate> _allowedTicketTemplates = new();

    private DrawTemplate(
        Guid id,
        Guid tenantId,
        GameCode gameCode,
        string name,
        bool isActive,
        int version,
        bool isLocked,
        DateTime createdAtUtc,
        DateTime updatedAtUtc) : base(id)
    {
        TenantId = tenantId;
        GameCode = gameCode;
        Name = name;
        IsActive = isActive;
        Version = version;
        IsLocked = isLocked;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
    }

    private DrawTemplate()
    {
    }

    public Guid TenantId { get; private set; }

    public GameCode GameCode { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public bool IsActive { get; private set; }

    public int Version { get; private set; }

    public bool IsLocked { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<DrawTemplatePlayType> PlayTypes => _playTypes;

    public IReadOnlyCollection<DrawTemplatePrizeTier> PrizeTiers => _prizeTiers;

    public IReadOnlyCollection<DrawTemplateAllowedTicketTemplate> AllowedTicketTemplates => _allowedTicketTemplates;

    public static Result<DrawTemplate> Create(
        Guid tenantId,
        GameCode gameCode,
        string name,
        bool isActive,
        DateTime utcNow)
    {
        if (tenantId == Guid.Empty)
        {
            return Result.Failure<DrawTemplate>(GamingErrors.DrawTemplateTenantRequired);
        }

        if (string.IsNullOrWhiteSpace(gameCode.Value))
        {
            return Result.Failure<DrawTemplate>(GamingErrors.GameCodeRequired);
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<DrawTemplate>(GamingErrors.DrawTemplateNameRequired);
        }

        string normalizedName = name.Trim();
        if (normalizedName.Length > 64)
        {
            return Result.Failure<DrawTemplate>(GamingErrors.DrawTemplateNameTooLong);
        }

        return new DrawTemplate(
            Guid.NewGuid(),
            tenantId,
            gameCode,
            normalizedName,
            isActive,
            1,
            false,
            utcNow,
            utcNow);
    }

    public Result UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure(GamingErrors.DrawTemplateNameRequired);
        }

        string normalizedName = name.Trim();
        if (normalizedName.Length > 64)
        {
            return Result.Failure(GamingErrors.DrawTemplateNameTooLong);
        }

        Name = normalizedName;
        return Result.Success();
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public Result AddPlayType(PlayTypeCode playTypeCode)
    {
        bool exists = _playTypes.Any(item => item.PlayTypeCode == playTypeCode);
        if (exists)
        {
            return Result.Failure(GamingErrors.DrawTemplatePlayTypeDuplicated);
        }

        _playTypes.Add(DrawTemplatePlayType.Create(TenantId, Id, playTypeCode));
        return Result.Success();
    }

    public Result RemovePlayType(PlayTypeCode playTypeCode)
    {
        if (IsLocked)
        {
            return Result.Failure(GamingErrors.DrawTemplateLocked);
        }

        DrawTemplatePlayType? existing = _playTypes.Find(item => item.PlayTypeCode == playTypeCode);
        if (existing is null)
        {
            return Result.Failure(GamingErrors.DrawTemplatePlayTypeNotFound);
        }

        _playTypes.Remove(existing);
        _prizeTiers.RemoveAll(item => item.PlayTypeCode == playTypeCode);
        return Result.Success();
    }

    public Result UpsertPrizeTier(PlayTypeCode playTypeCode, PrizeTier tier, PrizeOption option)
    {
        DrawTemplatePlayType? existing = _playTypes.Find(item => item.PlayTypeCode == playTypeCode);
        if (existing is null)
        {
            return Result.Failure(GamingErrors.DrawTemplatePlayTypeNotFound);
        }

        DrawTemplatePrizeTier? tierItem = _prizeTiers
            .Find(item => item.PlayTypeCode == playTypeCode && item.Tier == tier);
        if (tierItem is null)
        {
            _prizeTiers.Add(DrawTemplatePrizeTier.Create(TenantId, Id, playTypeCode, tier, option));
            return Result.Success();
        }

        tierItem.Update(option);
        return Result.Success();
    }

    public Result RemovePrizeTier(PlayTypeCode playTypeCode, PrizeTier tier)
    {
        if (IsLocked)
        {
            return Result.Failure(GamingErrors.DrawTemplateLocked);
        }

        DrawTemplatePlayType? existing = _playTypes.Find(item => item.PlayTypeCode == playTypeCode);
        if (existing is null)
        {
            return Result.Failure(GamingErrors.DrawTemplatePlayTypeNotFound);
        }

        DrawTemplatePrizeTier? tierItem = _prizeTiers
            .Find(item => item.PlayTypeCode == playTypeCode && item.Tier == tier);
        if (tierItem is null)
        {
            return Result.Failure(GamingErrors.DrawTemplatePrizeTierNotFound);
        }

        _prizeTiers.Remove(tierItem);
        return Result.Success();
    }

    public Result AddAllowedTicketTemplate(Guid ticketTemplateId, DateTime utcNow)
    {
        if (ticketTemplateId == Guid.Empty)
        {
            return Result.Failure(GamingErrors.DrawTemplateTicketTemplateRequired);
        }

        bool exists = _allowedTicketTemplates.Any(item => item.TicketTemplateId == ticketTemplateId);
        if (exists)
        {
            return Result.Failure(GamingErrors.DrawTemplateAllowedTicketTemplateDuplicated);
        }

        _allowedTicketTemplates.Add(
            DrawTemplateAllowedTicketTemplate.Create(TenantId, Id, ticketTemplateId, utcNow));
        return Result.Success();
    }

    public Result RemoveAllowedTicketTemplate(Guid ticketTemplateId)
    {
        if (IsLocked)
        {
            return Result.Failure(GamingErrors.DrawTemplateLocked);
        }

        DrawTemplateAllowedTicketTemplate? existing = _allowedTicketTemplates
            .Find(item => item.TicketTemplateId == ticketTemplateId);
        if (existing is null)
        {
            return Result.Failure(GamingErrors.DrawTemplateAllowedTicketTemplateNotFound);
        }

        _allowedTicketTemplates.Remove(existing);
        return Result.Success();
    }

    public void Lock()
    {
        if (IsLocked)
        {
            return;
        }

        IsLocked = true;
    }

    public void Touch(DateTime utcNow)
    {
        UpdatedAtUtc = utcNow;
        Version += 1;
    }
}
