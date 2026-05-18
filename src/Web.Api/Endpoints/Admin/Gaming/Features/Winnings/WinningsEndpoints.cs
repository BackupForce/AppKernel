using Asp.Versioning;
using Web.Api.Endpoints.Admin.Gaming.Features.Tickets.Endpoints;
using Web.Api.Endpoints.Admin.Gaming.Features.Winnings.Endpoints;

namespace Web.Api.Endpoints.Admin.Gaming.Features.Winnings;
internal static class WinningsEndpoints
{
    public static void Map(RouteGroupBuilder parent)
    {
        RouteGroupBuilder group = parent.MapGroup("/winnings")
            .WithMetadata(new ApiVersion(1, 0))
            .WithTags("Gaming.Winnings");

        group.MapGetWinningEndpoint();

        group.MapRedeemWinningEndpoint();

        group.MapGetRedeemedWinningsByDateRangeEndpoint();
    }
}
