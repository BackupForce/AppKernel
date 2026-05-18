using Domain.Gaming.Catalog;
using Domain.Gaming.Rules;
using Domain.Gaming.Shared;
using SharedKernel;

namespace Domain.Gaming.DrawTemplates;

/// <summary>
/// 模板內獎項設定（玩法 + 獎級）。
/// </summary>
public sealed class DrawTemplatePrizeTier : Entity
{
    private DrawTemplatePrizeTier(
        Guid id,
        Guid tenantId,
        Guid templateId,
        PlayTypeCode playTypeCode,
        PrizeTier tier,
        PrizeOption option) : base(id)
    {
        TenantId = tenantId;
        TemplateId = templateId;
        PlayTypeCode = playTypeCode;
        Tier = tier;
        Option = option;
    }

    private DrawTemplatePrizeTier()
    {
    }

    public Guid TenantId { get; private set; }

    public Guid TemplateId { get; private set; }

    public PlayTypeCode PlayTypeCode { get; private set; }

    public PrizeTier Tier { get; private set; }

    public PrizeOption Option { get; private set; } = null!;

    public static DrawTemplatePrizeTier Create(
        Guid tenantId,
        Guid templateId,
        PlayTypeCode playTypeCode,
        PrizeTier tier,
        PrizeOption option)
    {
        return new DrawTemplatePrizeTier(Guid.NewGuid(), tenantId, templateId, playTypeCode, tier, option);
    }

    public void Update(PrizeOption option)
    {
        Option = option;
    }
}
