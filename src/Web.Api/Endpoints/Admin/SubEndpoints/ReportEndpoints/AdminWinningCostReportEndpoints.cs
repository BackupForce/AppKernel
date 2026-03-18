using Application.Abstractions.Authentication;
using Application.Abstractions.Authorization;
using Application.Gaming.Reports.Admin;
using Asp.Versioning;
using Domain.Security;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Requests;

namespace Web.Api.Endpoints.Admin.SubEndpoints.ReportEndpoints;

public sealed class AdminWinningCostReportEndpoints : IEndpoint
{
    private const int MaxPageSize = 200;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/tenants/{tenantId:guid}/admin/reports")
            .WithGroupName("admin-v1")
            .WithMetadata(new ApiVersion(1, 0))
            .RequireAuthorization(AuthorizationPolicyNames.TenantUser)
            .WithTags("Admin Reports");

        group.MapGet(
                "/winnings",
                async ([AsParameters] GetWinningCostReportRequest request,
                    ITenantContext tenantContext,
                    ISender sender,
                    CancellationToken ct) =>
                {
                    IResult? validation = ValidateRangeAndPaging(
                        request.FromUtc,
                        request.ToUtc,
                        request.Page,
                        request.PageSize);
                    if (validation is not null)
                    {
                        return validation;
                    }

                    GetWinningCostReportQuery query = new(
                        tenantContext.TenantId,
                        request.FromUtc,
                        request.ToUtc,
                        request.GameCode,
                        request.DrawGroupId,
                        request.IncludeDetails,
                        request.Page,
                        request.PageSize);

                    return await UseCaseInvoker.Send<GetWinningCostReportQuery, WinningCostReportPageDto>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            // TODO: replace with dedicated report permission when permission catalog is extended.
            .RequireAuthorization(Permission.GamingDraw.Settle.Name)
            .Produces<WinningCostReportPageDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("AdminGetWinningCostReport");

        group.MapGet(
                "/winnings/{drawId:guid}",
                async (Guid drawId,
                    [AsParameters] GetWinningCostByDrawRequest request,
                    ITenantContext tenantContext,
                    ISender sender,
                    CancellationToken ct) =>
                {
                    if (request.DetailPage < 1)
                    {
                        return Results.Problem(
                            title: "Invalid pagination",
                            detail: "detailPage must be greater than or equal to 1.",
                            statusCode: StatusCodes.Status400BadRequest);
                    }

                    if (request.DetailPageSize < 1 || request.DetailPageSize > MaxPageSize)
                    {
                        return Results.Problem(
                            title: "Invalid pagination",
                            detail: $"detailPageSize must be between 1 and {MaxPageSize}.",
                            statusCode: StatusCodes.Status400BadRequest);
                    }

                    GetWinningCostByDrawQuery query = new(
                        tenantContext.TenantId,
                        drawId,
                        request.DetailPage,
                        request.DetailPageSize);

                    return await UseCaseInvoker.Send<GetWinningCostByDrawQuery, WinningCostPerDrawDto>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            // TODO: replace with dedicated report permission when permission catalog is extended.
            .RequireAuthorization(Permission.GamingDraw.Settle.Name)
            .Produces<WinningCostPerDrawDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("AdminGetWinningCostByDraw");
    }

    private static IResult? ValidateRangeAndPaging(DateTime fromUtc, DateTime toUtc, int page, int pageSize)
    {
        if (fromUtc >= toUtc)
        {
            return Results.Problem(
                title: "Invalid date range",
                detail: "fromUtc must be earlier than toUtc.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        if (page < 1)
        {
            return Results.Problem(
                title: "Invalid pagination",
                detail: "page must be greater than or equal to 1.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        if (pageSize < 1 || pageSize > MaxPageSize)
        {
            return Results.Problem(
                title: "Invalid pagination",
                detail: $"pageSize must be between 1 and {MaxPageSize}.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        return null;
    }
}
