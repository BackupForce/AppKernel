using Application.Abstractions.Authentication;
using Application.Abstractions.Authorization;
using Application.Abstractions.Data;
using Application.Gaming.Dtos;
using Application.Gaming.Tickets.Admin;
using Application.Gaming.Tickets.Cancel;
using Application.Gaming.Tickets.Issue;
using Domain.Gaming.Tickets;
using Domain.Security;
using MediatR;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Gaming.Features.Tickets.Requests;

namespace Web.Api.Endpoints.Admin.Gaming.Features.Tickets.Endpoints;

public static class GetTicketsEndpoint
{
    public static RouteHandlerBuilder MapGetTicketsEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapGet(
                "/",
                async ([AsParameters] GetAdminTicketsRequest request,
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

                    if (request.IssuedFromUtc.HasValue
                        && request.IssuedToUtc.HasValue
                        && request.IssuedFromUtc > request.IssuedToUtc)
                    {
                        return Results.BadRequest("IssuedFromUtc must be earlier than or equal to IssuedToUtc.");
                    }

                    if (request.SubmittedFromUtc.HasValue
                        && request.SubmittedToUtc.HasValue
                        && request.SubmittedFromUtc > request.SubmittedToUtc)
                    {
                        return Results.BadRequest("SubmittedFromUtc must be earlier than or equal to SubmittedToUtc.");
                    }

                    if (request.CreatedFromUtc.HasValue
                        && request.CreatedToUtc.HasValue
                        && request.CreatedFromUtc > request.CreatedToUtc)
                    {
                        return Results.BadRequest("CreatedFromUtc must be earlier than or equal to CreatedToUtc.");
                    }

                    TicketSubmissionStatus? submissionStatus = null;
                    if (!string.IsNullOrWhiteSpace(request.Status))
                    {
                        if (!Enum.TryParse(request.Status, true, out TicketSubmissionStatus parsedStatus)
                            || parsedStatus is TicketSubmissionStatus.Expired)
                        {
                            return Results.BadRequest("Status must be NotSubmitted, Submitted, or Cancelled.");
                        }

                        submissionStatus = parsedStatus;
                    }

                    GetAdminTicketsQuery query = new GetAdminTicketsQuery(
                        tenantContext.TenantId,
                        request.DrawId,
                        submissionStatus,
                        request.MemberId,
                        request.MemberNo,
                        request.IssuedFromUtc,
                        request.IssuedToUtc,
                        request.SubmittedFromUtc,
                        request.SubmittedToUtc,
                        request.CreatedFromUtc,
                        request.CreatedToUtc,
                        request.Page,
                        request.PageSize);

                    return await UseCaseInvoker.Send<GetAdminTicketsQuery, PagedResult<AdminTicketListItemDto>>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Tickets.Read.Name)
            .Produces<PagedResult<AdminTicketListItemDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("AdminGetTickets");
    }
}
