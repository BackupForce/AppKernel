using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Gaming.Dtos;
using Application.Gaming.TicketClaimEvents.Claims;
using Application.Gaming.TicketClaimEvents.Create;
using Application.Gaming.TicketClaimEvents.GetById;
using Application.Gaming.TicketClaimEvents.List;
using Domain.Gaming.TicketClaimEvents;
using Domain.Security;
using MediatR;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Requests;

namespace Web.Api.Endpoints.Admin.Gaming.Features.TicketClaimEvents.Endpoints;

public static class GetTicketClaimEventClaimsEndpoint
{
    public static RouteHandlerBuilder MapGetTicketClaimEventClaimsEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapGet(
                "/{id:guid}/claims",
                async (Guid tenantId,
                    Guid id,
                    [AsParameters] GetTicketClaimEventClaimsRequest request,
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

                    if (request.ClaimedFromUtc.HasValue
                        && request.ClaimedToUtc.HasValue
                        && request.ClaimedFromUtc > request.ClaimedToUtc)
                    {
                        return Results.BadRequest("ClaimedFromUtc must be earlier than or equal to ClaimedToUtc.");
                    }

                    GetTicketClaimEventClaimsQuery query = new(
                        tenantId,
                        id,
                        request.MemberId,
                        request.ClaimedFromUtc,
                        request.ClaimedToUtc,
                        request.Page,
                        request.PageSize);

                    return await UseCaseInvoker.Send<GetTicketClaimEventClaimsQuery, PagedResult<TicketClaimRecordDto>>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.GamingTicketClaimEvent.ClaimRead.Name)
            .Produces<PagedResult<TicketClaimRecordDto>>(StatusCodes.Status200OK)
            .WithSummary("Get ticket claim event claims")
            .WithDescription("Returns paged claim records for a ticket claim event, including memberId, memberNo, displayName, quantity, claimedAtUtc, and issuedTicketIds.")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("AdminGetTicketClaimEventClaims");
    }
}
