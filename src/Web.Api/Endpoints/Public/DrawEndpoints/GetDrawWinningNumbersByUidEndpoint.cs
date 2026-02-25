using Application.Gaming.Draws.GetWinningNumbersByUid;
using Asp.Versioning;
using MediatR;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Public.DrawEndpoints;

public sealed class GetDrawWinningNumbersByUidEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/public/draws")
            .WithGroupName("public-v1")
            .WithMetadata(new ApiVersion(1, 0))
            .WithTags("Public Draws");

        group.MapGet(
                "/{uid}/winning-numbers",
                async (string uid, ISender sender, CancellationToken ct) =>
                {
                    GetDrawWinningNumbersByUidQuery query = new GetDrawWinningNumbersByUidQuery(uid);
                    SharedKernel.Result<DrawWinningNumbersDto> result = await sender.Send(query, ct);
                    return result.Match(Results.Ok, CustomResults.Problem);
                })
            .Produces<DrawWinningNumbersDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("GetDrawWinningNumbersByUid");
    }
}
