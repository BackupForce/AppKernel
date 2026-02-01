using Application.Abstractions.Authorization;
using Application.Gaming.Dtos;
using Application.Gaming.TicketClaimEvents.Activate;
using Application.Gaming.TicketClaimEvents.Claims;
using Application.Gaming.TicketClaimEvents.Create;
using Application.Gaming.TicketClaimEvents.Disable;
using Application.Gaming.TicketClaimEvents.End;
using Application.Gaming.TicketClaimEvents.GetById;
using Application.Gaming.TicketClaimEvents.List;
using Application.Gaming.TicketClaimEvents.Update;
using Asp.Versioning;
using Domain.Gaming.TicketClaimEvents;
using Domain.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Requests;

namespace Web.Api.Endpoints.Admin;

public sealed class AdminTicketClaimEventEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/tenants/{tenantId:guid}/admin")
            .WithGroupName("admin-v1")
            .WithMetadata(new ApiVersion(1, 0))
            .RequireAuthorization(AuthorizationPolicyNames.TenantUser)
            .WithTags("Admin Ticket Claim Events");

        group.MapPost(
                "/ticket-claim-events",
                async (Guid tenantId, CreateTicketClaimEventRequest request, ISender sender, CancellationToken ct) =>
                {
                    if (!TryParseScopeType(request.ScopeType, out TicketClaimEventScopeType scopeType))
                    {
                        return Results.BadRequest("ScopeType must be SingleDraw or SingleDrawGroup.");
                    }

                    CreateTicketClaimEventCommand command = new(
                        tenantId,
                        request.Name,
                        request.StartsAtUtc,
                        request.EndsAtUtc,
                        request.TotalQuota,
                        request.PerMemberQuota,
                        scopeType,
                        request.ScopeId,
                        request.TicketTemplateId);

                    return await UseCaseInvoker.Send<CreateTicketClaimEventCommand, Guid>(
                        command,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Gaming.TicketClaimEventCreate.Name)
            .Produces<Guid>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("AdminCreateTicketClaimEvent");

        group.MapPut(
                "/ticket-claim-events/{id:guid}",
                async (Guid tenantId, Guid id, UpdateTicketClaimEventRequest request, ISender sender, CancellationToken ct) =>
                {
                    if (!TryParseScopeType(request.ScopeType, out TicketClaimEventScopeType scopeType))
                    {
                        return Results.BadRequest("ScopeType must be SingleDraw or SingleDrawGroup.");
                    }

                    UpdateTicketClaimEventCommand command = new(
                        tenantId,
                        id,
                        request.Name,
                        request.StartsAtUtc,
                        request.EndsAtUtc,
                        request.TotalQuota,
                        request.PerMemberQuota,
                        scopeType,
                        request.ScopeId,
                        request.TicketTemplateId);

                    return await UseCaseInvoker.Send<UpdateTicketClaimEventCommand>(command, sender, ct);
                })
            .RequireAuthorization(Permission.Gaming.TicketClaimEventUpdate.Name)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("AdminUpdateTicketClaimEvent");

        group.MapPost(
                "/ticket-claim-events/{id:guid}/activate",
                async (Guid tenantId, Guid id, ISender sender, CancellationToken ct) =>
                {
                    ActivateTicketClaimEventCommand command = new(tenantId, id);
                    return await UseCaseInvoker.Send<ActivateTicketClaimEventCommand>(command, sender, ct);
                })
            .RequireAuthorization(Permission.Gaming.TicketClaimEventActivate.Name)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("AdminActivateTicketClaimEvent");

        group.MapPost(
                "/ticket-claim-events/{id:guid}/disable",
                async (Guid tenantId, Guid id, ISender sender, CancellationToken ct) =>
                {
                    DisableTicketClaimEventCommand command = new(tenantId, id);
                    return await UseCaseInvoker.Send<DisableTicketClaimEventCommand>(command, sender, ct);
                })
            .RequireAuthorization(Permission.Gaming.TicketClaimEventDisable.Name)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("AdminDisableTicketClaimEvent");

        group.MapPost(
                "/ticket-claim-events/{id:guid}/end",
                async (Guid tenantId, Guid id, ISender sender, CancellationToken ct) =>
                {
                    EndTicketClaimEventCommand command = new(tenantId, id);
                    return await UseCaseInvoker.Send<EndTicketClaimEventCommand>(command, sender, ct);
                })
            .RequireAuthorization(Permission.Gaming.TicketClaimEventEnd.Name)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("AdminEndTicketClaimEvent");

        group.MapGet(
                "/ticket-claim-events/{id:guid}",
                async (Guid tenantId, Guid id, ISender sender, CancellationToken ct) =>
                {
                    GetTicketClaimEventQuery query = new(tenantId, id);
                    return await UseCaseInvoker.Send<GetTicketClaimEventQuery, TicketClaimEventDetailDto>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Gaming.TicketClaimEventRead.Name)
            .Produces<TicketClaimEventDetailDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("AdminGetTicketClaimEvent");

        group.MapGet(
                "/ticket-claim-events",
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

        group.MapGet(
                "/ticket-claim-events/{id:guid}/claims",
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
            .RequireAuthorization(Permission.Gaming.TicketClaimEventClaimRead.Name)
            .Produces<PagedResult<TicketClaimRecordDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("AdminGetTicketClaimEventClaims");
    }

    private static bool TryParseScopeType(string? value, out TicketClaimEventScopeType scopeType)
    {
        scopeType = TicketClaimEventScopeType.SingleDraw;
        return !string.IsNullOrWhiteSpace(value)
               && Enum.TryParse(value.Trim(), true, out scopeType);
    }
}
