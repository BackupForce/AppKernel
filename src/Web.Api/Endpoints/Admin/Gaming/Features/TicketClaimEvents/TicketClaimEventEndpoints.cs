using Asp.Versioning;
using Web.Api.Endpoints.Admin.Gaming.Features.TicketClaimEvents.Endpoints;
using Web.Api.Endpoints.Admin.Gaming.Features.Winnings.Endpoints;

namespace Web.Api.Endpoints.Admin.Gaming.Features.TicketClaimEvents;

internal static class TicketClaimEventEndpoints
{
    public static void Map(RouteGroupBuilder parent)
    {
        RouteGroupBuilder group = parent.MapGroup("/ticket-claim-events")
            .WithMetadata(new ApiVersion(1, 0))
            .WithTags("Gaming.TicketClaimEvents");

        //CRUD
        group.MapCreateTicketClaimEventEndpoint();
        group.MapUpdateTicketClaimEventEndpoint();
        group.MapGetTicketClaimEventEndpoint();
        group.MapGetTicketClaimEventsEndpoint();

        group.MapGetTicketClaimEventClaimsEndpoint();
        
        //Actions
        group.MapActivateTicketClaimEventEndpoint();
        group.MapDisableTicketClaimEventEndpoint();
        group.MapEndTicketClaimEventEndpoint();
        
    }
}
