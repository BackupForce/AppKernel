using Application.Abstractions.Authorization;
using Application.Gaming.Dtos;
using Application.Gaming.TicketClaimEvents.Claim;
using Domain.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Common;

namespace Web.Api.Endpoints.Frontends.SubEndpoints;

internal static class TicketClaimEventEndpoints
{
    public static void Map(RouteGroupBuilder parent)
    {
        RouteGroupBuilder group = parent.MapGroup("/ticket-claim-events")
            .WithTags("FE.TicketClaimEvents");

        group.MapPost("/{eventId:guid}/claim",
            async (Guid eventId,
                [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
                ISender sender,
                CancellationToken ct) =>
            {
                ClaimTicketFromEventCommand command = new(eventId, idempotencyKey);
                return await UseCaseInvoker.Send<ClaimTicketFromEventCommand, TicketClaimResult>(
                    command,
                    sender,
                    value => Results.Ok(value),
                    ct);
            })
        .Produces<TicketClaimResult>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("活動領票")
        .WithDescription("根據活動Id領取活動票據")
        .WithName("ClaimTicketFromEvent");
    }
}
