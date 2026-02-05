using Application.Abstractions.Authorization;
using Asp.Versioning;
using Web.Api.Endpoints.Frontends.SubEndpoints;

namespace Web.Api.Endpoints.Frontends;

public sealed class FrontendEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/tenants/{tenantId:guid}/me")
            .WithGroupName("frontend-v1")
            .WithMetadata(new ApiVersion(1, 0))
            .RequireAuthorization(AuthorizationPolicyNames.Member)
            .WithTags("Me");

        AwardEndpoints.Map(group);
        TicketEndpoints.Map(group);
        TicketClaimEventEndpoints.Map(group);

        RouteGroupBuilder meGroup = app.MapGroup("/frontend/me")
            .WithGroupName("frontend-v1")
            .WithMetadata(new ApiVersion(1, 0))
            .RequireAuthorization(AuthorizationPolicyNames.Member)
            .WithTags("Me");

        TicketEndpoints.MapWinningTickets(meGroup);
    }
}
