using Application.Abstractions.Authorization;
using Application.Gaming.Tickets.Admin;
using Application.Members.Activate;
using Domain.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Members.Requests;
using Web.Api.Endpoints.Admin.Requests;

namespace Web.Api.Endpoints.Admin.Members.Endpoints;

public static class IssueMemberTicketsEndpoint
{
    public static RouteHandlerBuilder MapIssueMemberTicketsEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapPost(
                "/{memberId:guid}/tickets",
                async (Guid memberId,
                    IssueMemberTicketsRequest request,
                    [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
                    ISender sender,
                    CancellationToken ct) =>
                {
                    IssueMemberTicketsCommand command = new IssueMemberTicketsCommand(
                        memberId,
                        request.GameCode,
                        request.DrawId,
                        request.Quantity,
                        request.Reason,
                        request.Note,
                        idempotencyKey);

                    return await UseCaseInvoker.Send<IssueMemberTicketsCommand, IssueMemberTicketsResult>(
                        command,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Tickets.Issue.Name)
            .Produces<IssueMemberTicketsResult>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .WithName("AdminIssueMemberTickets");
    }
}
