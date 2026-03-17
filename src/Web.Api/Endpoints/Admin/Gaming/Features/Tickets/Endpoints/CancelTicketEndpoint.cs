using Application.Abstractions.Authorization;
using Application.Gaming.Tickets.Cancel;
using Application.Gaming.Tickets.Issue;
using MediatR;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Gaming.Features.Tickets.Requests;

namespace Web.Api.Endpoints.Admin.Gaming.Features.Tickets.Endpoints;

public static class CancelTicketEndpoint
{
    public static RouteHandlerBuilder MapCancelTicketEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapPost(
                "/{ticketId:guid}/cancel",
                async (Guid ticketId, CancelTicketRequest request, ISender sender, CancellationToken ct) =>
                {
                    var command = new CancelTicketCommand(ticketId, request.Reason);
                    return await UseCaseInvoker.Send(
                        command,
                        sender,
                        ct);
                })
            .RequireAuthorization(AuthorizationPolicyNames.TenantUser)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("CancelTicket");
    }
}
