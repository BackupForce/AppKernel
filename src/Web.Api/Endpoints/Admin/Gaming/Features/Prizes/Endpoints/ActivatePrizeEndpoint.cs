using Application.Gaming.Dtos;
using Application.Gaming.Prizes.Activate;
using Application.Gaming.Prizes.Create;
using Application.Gaming.Prizes.GetList;
using Domain.Security;
using MediatR;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Gaming.Features.Prizes.Requests;

namespace Web.Api.Endpoints.Admin.Gaming.Features.Prizes.Endpoints;

public static class ActivatePrizeEndpoint
{
    public static RouteHandlerBuilder MapActivatePrizeEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapPatch(
                "/{prizeId:guid}/activate",
                async (Guid prizeId, ISender sender, CancellationToken ct) =>
                {
                    var command = new ActivatePrizeCommand(prizeId);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .Produces(StatusCodes.Status200OK)
            .WithName("ActivatePrize");
    }
}
