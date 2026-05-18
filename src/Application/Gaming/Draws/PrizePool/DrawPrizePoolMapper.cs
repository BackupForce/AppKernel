using Domain.Gaming.Draws;
using Domain.Gaming.Shared;

namespace Application.Gaming.Draws.PrizePool;

internal static class DrawPrizePoolMapper
{
    internal static DrawPrizePoolDto ToDto(Draw draw)
    {
        IReadOnlyCollection<DrawPrizePoolItemDto> items = draw.PrizePoolItems
            .Select(item => new DrawPrizePoolItemDto(
                item.PlayTypeCode.Value,
                item.Tier.Value,
                item.IsConfigured,
                item.Option is null ? null : ToDto(item.Option)))
            .ToList();

        return new DrawPrizePoolDto(draw.Id, draw.GameCode.Value, items);
    }

    internal static PrizeOptionDto ToDto(PrizeOption option)
    {
        return new PrizeOptionDto(
            option.PrizeId,
            option.Name,
            option.Cost,
            option.PayoutAmount,
            option.RedeemValidDays,
            option.Description);
    }
}
