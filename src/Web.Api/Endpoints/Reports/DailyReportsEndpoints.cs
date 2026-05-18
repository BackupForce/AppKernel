using Application.Abstractions.Authorization;
using Application.Reports.Daily;
using Asp.Versioning;
using MediatR;
using Web.Api.Common;

namespace Web.Api.Endpoints.Reports;

public sealed class DailyReportsEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/tenants/{tenantId:guid}/reports")
            .WithGroupName("admin-v1")
            .WithMetadata(new ApiVersion(1, 0))
            .RequireAuthorization(AuthorizationPolicyNames.TenantUser)
            .WithTags("Reports");

        group.MapGet(
                "/daily",
                async (Guid tenantId, DateOnly date, ISender sender, CancellationToken ct) =>
                {
                    GetDailyReportQuery query = new GetDailyReportQuery(tenantId, date);
                    return await UseCaseInvoker.Send<GetDailyReportQuery, DailyReportResponse>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .Produces<DailyReportResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("GetDailyReport");
    }
}
