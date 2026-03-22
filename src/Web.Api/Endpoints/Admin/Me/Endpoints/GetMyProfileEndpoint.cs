using Application.Users.GetMyProfile;
using MediatR;
using Web.Api.Common;

namespace Web.Api.Endpoints.Admin.Me.Endpoints;

public static class GetMyProfileEndpoint
{
    public static RouteHandlerBuilder MapGetMyProfileEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapGet(
                "/profile",
                async (ISender sender, CancellationToken ct) =>
                {
                    GetMyProfileQuery query = new();
                    return await UseCaseInvoker.Send<GetMyProfileQuery, MyProfileDto>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .Produces<MyProfileDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Get current user profile")
            .WithDescription("Returns the authenticated admin user's own basic profile within the current tenant.")
            .WithName("AdminGetMyProfile");
    }
}
