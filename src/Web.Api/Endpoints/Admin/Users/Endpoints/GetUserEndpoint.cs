using Application.Users.Create;
using Application.Users.GetById;
using Domain.Security;
using MediatR;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Roles.Requests;

namespace Web.Api.Endpoints.Admin.Users.Endpoints;

public static class GetUserEndpoint
{
    public static RouteHandlerBuilder MapGetUserEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapGet("/{id:guid}",
            async (Guid id, ISender sender, CancellationToken ct) =>
            {
                GetUserByIdQuery request = new GetUserByIdQuery(id);
                return await UseCaseInvoker.Send<GetUserByIdQuery, UserResponse>(
                    request,
                    sender,
                    value => Results.Ok(value),
                    ct);
            })
        .Produces<UserResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithName("GetUserById");
    }
}
