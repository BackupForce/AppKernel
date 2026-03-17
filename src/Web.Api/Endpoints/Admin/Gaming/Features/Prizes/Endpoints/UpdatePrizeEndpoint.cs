using Application.Gaming.Dtos;
using Application.Gaming.Prizes.Create;
using Application.Gaming.Prizes.GetList;
using Application.Gaming.Prizes.Update;
using Domain.Security;
using MediatR;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Gaming.Features.Prizes.Requests;

namespace Web.Api.Endpoints.Admin.Gaming.Features.Prizes.Endpoints;

public static class UpdatePrizeEndpoint
{
    public static RouteHandlerBuilder MapUpdatePrizeEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapPut(
                "/{prizeId:guid}",
                async (Guid prizeId, UpdatePrizeRequest request, ISender sender, CancellationToken ct) =>
                {
                    var command = new UpdatePrizeCommand(prizeId, request.Name, request.Description, request.Cost);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("UpdatePrize");
    }
}
