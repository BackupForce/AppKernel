using Domain.Gaming.Draws;

namespace Application.Gaming.Tickets.Services;

internal static class DrawEligibility
{
    public static bool IsEligiblePrimary(Draw draw, DateTime nowUtc)
    {
        if (draw.Status == DrawStatus.Cancelled)
        {
            return false;
        }

        if (draw.SettledAtUtc.HasValue)
        {
            return false;
        }

        if (draw.DrawnAt.HasValue || !string.IsNullOrWhiteSpace(draw.WinningNumbersRaw))
        {
            return false;
        }

        if (draw.IsManuallyClosed)
        {
            return false;
        }

        return nowUtc < draw.SalesCloseAt;
    }

    public static bool IsRedeemable(Draw draw)
    {
        if (draw.Status == DrawStatus.Cancelled)
        {
            return false;
        }

        if (draw.SettledAtUtc.HasValue)
        {
            return false;
        }

        return !draw.DrawnAt.HasValue && string.IsNullOrWhiteSpace(draw.WinningNumbersRaw);
    }
}
