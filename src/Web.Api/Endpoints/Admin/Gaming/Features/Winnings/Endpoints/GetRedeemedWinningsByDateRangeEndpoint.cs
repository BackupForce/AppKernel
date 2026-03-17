using Application.Abstractions.Authentication;
using Application.Abstractions.Authorization;
using Application.Abstractions.Data;
using Application.Gaming.Dtos;
using Application.Gaming.Tickets.Admin.Winnings;
using Domain.Security;
using MediatR;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Gaming.Features.Tickets.Requests;
using Web.Api.Endpoints.Admin.Requests;

namespace Web.Api.Endpoints.Admin.Gaming.Features.Winnings.Endpoints;

public static class GetRedeemedWinningsByDateRangeEndpoint
{
    public static RouteHandlerBuilder MapGetRedeemedWinningsByDateRangeEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapGet(
                "/redeemed",
                async ([AsParameters] GetRedeemedWinningsByDateRangeRequest request,
                    ITenantContext tenantContext,
                    ISender sender,
                    CancellationToken ct) =>
                {
                    if (request.Page < 1)
                    {
                        return Results.BadRequest("Page must be greater than or equal to 1.");
                    }

                    if (request.PageSize < 1 || request.PageSize > 200)
                    {
                        return Results.BadRequest("PageSize must be between 1 and 200.");
                    }

                    if (request.RedeemedFromUtc.HasValue
                        && request.RedeemedToUtc.HasValue
                        && request.RedeemedFromUtc > request.RedeemedToUtc)
                    {
                        return Results.BadRequest("RedeemedFromUtc must be earlier than or equal to RedeemedToUtc.");
                    }

                    GetRedeemedWinningsByDateRangeQuery query = new GetRedeemedWinningsByDateRangeQuery(
                        tenantContext.TenantId,
                        request.RedeemedFromUtc,
                        request.RedeemedToUtc,
                        request.Page,
                        request.PageSize);

                    return await UseCaseInvoker.Send<GetRedeemedWinningsByDateRangeQuery, PagedResult<RedeemedWinningListItemDto>>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Gaming.WinningRedeemedRead.Name)
            .Produces<PagedResult<RedeemedWinningListItemDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("AdminGetRedeemedWinningsByDateRange");
    }
}
