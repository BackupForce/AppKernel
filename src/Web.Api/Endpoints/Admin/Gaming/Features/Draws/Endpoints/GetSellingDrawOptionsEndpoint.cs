using Application.Gaming.Draws.Create;
using Application.Gaming.Draws.SellingOptions;
using Application.Gaming.Dtos;
using Domain.Security;
using MediatR;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Gaming.Features.Draws.Requests;
using Web.Api.Endpoints.Admin.Roles.Requests;

namespace Web.Api.Endpoints.Admin.Gaming.Features.Draws.Endpoints;

public static class GetSellingDrawOptionsEndpoint
{
    public static RouteHandlerBuilder MapGetSellingDrawOptionsEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapGet(
                "/selling/options",
                async ([AsParameters] GetSellingDrawOptionsRequest request, ISender sender, CancellationToken ct) =>
                {
                    var query = new GetSellingDrawOptionsQuery(
                        request.GameCode,
                        request.PlayTypeCode,
                        request.Take);
                    return await UseCaseInvoker.Send<GetSellingDrawOptionsQuery, IReadOnlyList<DrawSellingOptionDto>>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .AllowAnonymous()
            .Produces<IReadOnlyList<DrawSellingOptionDto>>(StatusCodes.Status200OK)
            .WithSummary("可售票期數下拉選項")
            .WithDescription("可售票期數下拉選項")
            .WithName("GetSellingDrawOptions");
    }
}
