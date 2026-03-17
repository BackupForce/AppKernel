using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Gaming.Dtos;
using Application.Gaming.TicketClaimEvents.Create;
using Application.Gaming.TicketClaimEvents.GetById;
using Application.Gaming.TicketClaimEvents.List;
using Domain.Gaming.TicketClaimEvents;
using Domain.Security;
using MediatR;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Requests;

namespace Web.Api.Endpoints.Admin.Gaming.Features.TicketClaimEvents.Endpoints;

public static class GetTicketClaimEventsEndpoint
{
    public static RouteHandlerBuilder MapGetTicketClaimEventsEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapGet(
                "/",
                async (Guid tenantId, [AsParameters] ListTicketClaimEventsRequest request, ISender sender, CancellationToken ct) =>
                {
                    if (request.Page < 1)
                    {
                        return Results.BadRequest("Page must be greater than or equal to 1.");
                    }

                    if (request.PageSize < 1 || request.PageSize > 200)
                    {
                        return Results.BadRequest("PageSize must be between 1 and 200.");
                    }

                    if (request.StartsFromUtc.HasValue
                        && request.StartsToUtc.HasValue
                        && request.StartsFromUtc > request.StartsToUtc)
                    {
                        return Results.BadRequest("StartsFromUtc must be earlier than or equal to StartsToUtc.");
                    }

                    if (request.EndsFromUtc.HasValue
                        && request.EndsToUtc.HasValue
                        && request.EndsFromUtc > request.EndsToUtc)
                    {
                        return Results.BadRequest("EndsFromUtc must be earlier than or equal to EndsToUtc.");
                    }

                    ListTicketClaimEventsQuery query = new(
                        tenantId,
                        request.Status,
                        request.StartsFromUtc,
                        request.StartsToUtc,
                        request.EndsFromUtc,
                        request.EndsToUtc,
                        request.Keyword,
                        request.Page,
                        request.PageSize);

                    return await UseCaseInvoker.Send<ListTicketClaimEventsQuery, PagedResult<TicketClaimEventSummaryDto>>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Gaming.TicketClaimEventRead.Name)
            .Produces<PagedResult<TicketClaimEventSummaryDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("AdminListTicketClaimEvents");
    }
}
