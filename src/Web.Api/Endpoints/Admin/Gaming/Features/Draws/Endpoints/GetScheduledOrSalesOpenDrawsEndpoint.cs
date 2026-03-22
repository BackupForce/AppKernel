using Application.Gaming.Draws.GetScheduledOrSalesOpen;
using Domain.Security;
using MediatR;
using Web.Api.Common;

namespace Web.Api.Endpoints.Admin.Gaming.Features.Draws.Endpoints;

public static class GetScheduledOrSalesOpenDrawsEndpoint
{
    public static RouteHandlerBuilder MapGetScheduledOrSalesOpenDrawsEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapGet(
                "/scheduled-sales-open",
                async (ISender sender, CancellationToken ct) =>
                {
                    GetScheduledOrSalesOpenDrawsQuery query = new();
                    return await UseCaseInvoker.Send<GetScheduledOrSalesOpenDrawsQuery, IReadOnlyCollection<ScheduledOrSalesOpenDrawDto>>(
                        query,
                        sender,
                        Results.Ok,
                        ct);
                })
            .RequireAuthorization(Permission.GamingDraw.Manage.Name)
            .Produces<IReadOnlyCollection<ScheduledOrSalesOpenDrawDto>>(StatusCodes.Status200OK)
            .WithName("GetScheduledOrSalesOpenGameDraws");
    }
}
