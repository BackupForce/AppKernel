using Application.Roles.Dtos;
using Application.Roles.GetById;
using Domain.Security;
using MediatR;
using Web.Api.Common;

namespace Web.Api.Endpoints.Admin.Roles.Endpoints;

public static class GetRoleEndpoint
{
    public static RouteHandlerBuilder MapGetRoleEnpoint(
        this RouteGroupBuilder group)
    {
        return group.MapGet(
                "/{id:int}",
                async (int id, ISender sender, CancellationToken ct) =>
                {
                    var request = new GetRoleByIdQuery(id);
                    return await UseCaseInvoker.Send<GetRoleByIdQuery, RoleDetailDto>(
                        request,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Roles.View.Name)
            .Produces<RoleDetailDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("GetRoleById");
    }
}
