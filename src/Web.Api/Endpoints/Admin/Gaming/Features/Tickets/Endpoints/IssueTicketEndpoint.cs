using Application.Abstractions.Authorization;
using Application.Gaming.Tickets.Issue;
using MediatR;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Gaming.Features.Tickets.Requests;

namespace Web.Api.Endpoints.Admin.Gaming.Features.Tickets.Endpoints;

public static class IssueTicketEndpoint
{
    public static RouteHandlerBuilder MapIssueTicketEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapPost(
                "/issue",
                async (IssueTicketRequest request, ISender sender, CancellationToken ct) =>
                {
                    var command = new IssueTicketCommand(
                        request.MemberId,
                        request.ResolveDrawGroupId(),
                        request.TicketTemplateId,
                        request.IssuedReason);
                    return await UseCaseInvoker.Send<IssueTicketCommand, IssueTicketResult>(
                        command,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(AuthorizationPolicyNames.TenantUser)
            .Produces<IssueTicketResult>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("IssueTicket");
    }
}
