using Application.Abstractions.Authorization;
using Application.Gaming.Tickets.Cancel;
using Application.Gaming.Tickets.Claim;
using Application.Gaming.Tickets.Issue;
using Application.Gaming.Tickets.Redeem;
using Application.Gaming.Tickets.Submit;
using Asp.Versioning;
using Domain.Security;
using MediatR;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Gaming.Features.Tickets.Endpoints;

namespace Web.Api.Endpoints.Admin.Gaming.Features.Tickets;

internal static class TicketEndpoints
{
    public static void Map(RouteGroupBuilder parent)
    {
        RouteGroupBuilder group = parent.MapGroup("/tickets")
            .WithMetadata(new ApiVersion(1, 0))
            .WithTags("Gaming.Tickets");

        group.MapGetTicketsEndpoint();

        group.MapIssueTicketEndpoint();
        group.MapCancelTicketEndpoint();
        group.MapClaimDrawGroupTicketEndpoint();
        group.MapRedeemTicketDrawEndpoint();
    }
}
