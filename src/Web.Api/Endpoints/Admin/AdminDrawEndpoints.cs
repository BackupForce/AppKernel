using System.Text.Json;
using Application.Abstractions.Authorization;
using Application.Gaming.Draws.GetDrawBetNumberStats;
using Application.Gaming.Draws.SetWinningNumbers;
using Asp.Versioning;
using Domain.Gaming.Shared;
using Domain.Security;
using MediatR;
using SharedKernel;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Requests;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Admin;

public sealed class AdminDrawEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/tenants/{tenantId:guid}/admin/gaming")
            .WithGroupName("admin-v1")
            .WithMetadata(new ApiVersion(1, 0))
            .RequireAuthorization(AuthorizationPolicyNames.TenantUser)
            .WithTags("Admin Gaming Draws");

        group.MapPost(
                "/draws/{drawId:guid}/winning-numbers",
                async (Guid drawId, SetDrawWinningNumbersRequest request, ISender sender, CancellationToken ct) =>
                {
                    (string? raw, IReadOnlyCollection<int>? numbers, Result? error) = ResolveWinningNumbers(request.WinningNumbers);
                    if (error is not null)
                    {
                        return CustomResults.Problem(error);
                    }

                    SetDrawWinningNumbersCommand command = new(
                        drawId,
                        raw,
                        numbers,
                        request.ForceRecalculate,
                        request.SourceNote);

                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .RequireAuthorization(Permission.Gaming.DrawSettle.Name)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("AdminSetDrawWinningNumbers");

        group.MapGet(
                "/draws/{drawId:guid}/bet-number-stats",
                async (Guid drawId, ISender sender, CancellationToken ct) =>
                {
                    GetDrawBetNumberStatsQuery query = new(drawId);
                    return await UseCaseInvoker.Send<GetDrawBetNumberStatsQuery, IReadOnlyCollection<DrawBetNumberStatDto>>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Gaming.WinningNumbersRead.Name)
            .Produces<IReadOnlyCollection<DrawBetNumberStatDto>>(StatusCodes.Status200OK)
            .WithName("AdminGetDrawBetNumberStats");
    }

    private static (string? Raw, IReadOnlyCollection<int>? Numbers, Result? Error) ResolveWinningNumbers(JsonElement input)
    {
        switch (input.ValueKind)
        {
            case JsonValueKind.String:
                return (input.GetString(), null, null);
            case JsonValueKind.Array:
            {
                var numbers = new List<int>();
                foreach (JsonElement element in input.EnumerateArray())
                {
                    if (element.ValueKind != JsonValueKind.Number || !element.TryGetInt32(out int value))
                    {
                        return (null, null, Result.Failure(GamingErrors.LotteryNumbersFormatInvalid));
                    }

                    numbers.Add(value);
                }

                return (null, numbers, null);
            }
            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
                return (null, null, null);
            default:
                return (null, null, Result.Failure(GamingErrors.LotteryNumbersFormatInvalid));
        }
    }
}
