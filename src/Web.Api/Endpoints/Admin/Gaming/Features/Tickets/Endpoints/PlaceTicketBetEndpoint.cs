using Application.Abstractions.Authorization;
using Application.Gaming.Tickets.Admin;
using Application.Gaming.Tickets.Cancel;
using Application.Gaming.Tickets.Issue;
using Domain.Gaming.Shared;
using Domain.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Gaming.Features.Tickets.Requests;
using Web.Api.Endpoints.Admin.Requests;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Admin.Gaming.Features.Tickets.Endpoints;

public static class PlaceTicketBetEndpoint
{
    private static readonly HashSet<string> UnprocessableErrorCodes = new(StringComparer.Ordinal)
    {
        GamingErrors.LotteryNumbersRequired.Code,
        GamingErrors.LotteryNumbersCountInvalid.Code,
        GamingErrors.LotteryNumbersOutOfRange.Code,
        GamingErrors.LotteryNumbersDuplicated.Code,
        GamingErrors.LotteryNumbersFormatInvalid.Code
    };
    public static RouteHandlerBuilder MapPlaceTicketBetEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapPost(
                "/{ticketId:guid}/bet",
                async (Guid ticketId,
                    PlaceTicketBetRequest request,
                    [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
                    ISender sender,
                    CancellationToken ct) =>
                {
                    PlaceTicketBetCommand command = new PlaceTicketBetCommand(
                        ticketId,
                        request.PlayTypeCode,
                        request.Numbers,
                        request.ClientReference,
                        request.Note,
                        idempotencyKey);

                    Result<PlaceTicketBetResult> result = await sender.Send(command, ct);
                    return ToBetResult(result);
                })
            .RequireAuthorization(Permission.Tickets.PlaceBet.Name)
            .Produces<PlaceTicketBetResult>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .WithName("AdminPlaceTicketBet");


    }

    private static IResult ToBetResult(Result<PlaceTicketBetResult> result)
    {
        if (result.IsSuccess)
        {
            return Results.Ok(result.Value);
        }

        if (UnprocessableErrorCodes.Contains(result.Error.Code))
        {
            return Results.Problem(
                title: result.Error.Code,
                detail: result.Error.Description,
                type: "https://tools.ietf.org/html/rfc4918#section-11.2",
                statusCode: StatusCodes.Status422UnprocessableEntity,
                extensions: BuildExtensions(result.Error));
        }

        return CustomResults.Problem(result);
    }

    private static Dictionary<string, object?> BuildExtensions(Error error)
    {
        Dictionary<string, object?> extensions = new()
        {
            { "errorCode", error.Code }
        };

        if (error is not ValidationError validationError)
        {
            return extensions;
        }

        extensions["errors"] = validationError.Errors;
        return extensions;
    }
}
