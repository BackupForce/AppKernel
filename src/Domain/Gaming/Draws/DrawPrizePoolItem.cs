using Domain.Gaming.Catalog;
using Domain.Gaming.Rules;
using Domain.Gaming.Shared;
using SharedKernel;

namespace Domain.Gaming.Draws;

/// <summary>
/// Draw 內的獎項配置（玩法 + 獎級）。
/// </summary>
public sealed class DrawPrizePoolItem : Entity
{
    private DrawPrizePoolItem(
        Guid id,
        Guid tenantId,
        Guid drawId,
        PlayTypeCode playTypeCode,
        PrizeTier tier,
        PrizeOption option) : base(id)
    {
        TenantId = tenantId;
        DrawId = drawId;
        PlayTypeCode = playTypeCode;
        Tier = tier;
        Option = option;
    }

    private DrawPrizePoolItem()
    {
    }

    public Guid TenantId { get; private set; }

    public Guid DrawId { get; private set; }

    public PlayTypeCode PlayTypeCode { get; private set; }

    public PrizeTier Tier { get; private set; }

    public PrizeOption Option { get; private set; } = null!;

    public static DrawPrizePoolItem Create(
        Guid tenantId,
        Guid drawId,
        PlayTypeCode playTypeCode,
        PrizeTier tier,
        PrizeOption option)
    {
        return new DrawPrizePoolItem(Guid.NewGuid(), tenantId, drawId, playTypeCode, tier, option);
    }

    /// <summary>
    /// 更新獎項快照（由期數設定流程覆寫）。
    /// </summary>
    public void Update(PrizeOption option)
    {
        Option = option;
    }
}
