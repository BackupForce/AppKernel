using Application.Abstractions.Authorization;
using Application.Tenants.UpdateTimeZone;
using Asp.Versioning;
using Domain.Tenants;
using MediatR;
using Web.Api.Common;
using Web.Api.Endpoints.Tenants.Requests;

namespace Web.Api.Endpoints.Tenants;

public sealed class TenantsEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/tenants")
            .WithGroupName("public-v1")
            .WithMetadata(new ApiVersion(1, 0))
            .WithTags("Tenants");

        RouteGroupBuilder settingsGroup = app.MapGroup("/tenants/{tenantId:guid}/settings")
            .WithGroupName("admin-v1")
            .WithMetadata(new ApiVersion(1, 0))
            .RequireAuthorization(AuthorizationPolicyNames.TenantUser)
            .WithTags("Tenants");

        // 中文註解：前端第一次進站或切租戶時，先用 tenantCode 換取 tenantId。
        // 中文註解：後續所有 tenant-scoped API 請使用 /tenants/{tenantId}/... 路由。
        group.MapGet(
                "/by-code/{tenantCode}",
                async (string tenantCode, ITenantRepository tenantRepository, CancellationToken ct) =>
                {
                    string normalizedCode = TenantCodeHelper.Normalize(tenantCode);
                    if (!TenantCodeHelper.IsValid(normalizedCode))
                    {
                        return Results.Problem(
                            title: "TenantCode 格式錯誤",
                            detail: "tenantCode 必須為 3 碼英數字。",
                            type: "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                            statusCode: StatusCodes.Status400BadRequest);
                    }

                    Tenant? tenant = await tenantRepository.GetByCodeAsync(normalizedCode, ct);
                    if (tenant is null)
                    {
                        return Results.Problem(
                            title: "Tenant 找不到",
                            detail: "找不到對應的 tenantCode。",
                            type: "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                            statusCode: StatusCodes.Status404NotFound);
                    }

                    TenantLookupResponse response = new TenantLookupResponse(tenant.Id, tenant.Code, tenant.Name, tenant.TimeZoneId);
                    return Results.Ok(response);
                })
            .AllowAnonymous()
            .Produces<TenantLookupResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("GetTenantByCode");

        settingsGroup.MapPut(
                "/timezone",
                async (Guid tenantId, UpdateTenantTimeZoneRequest request, ISender sender, CancellationToken ct) =>
                {
                    UpdateTenantTimeZoneCommand command = new UpdateTenantTimeZoneCommand(tenantId, request.TimeZoneId);
                    return await UseCaseInvoker.Send(command, sender, ct);
                })
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("UpdateTenantTimeZone");
    }

    public sealed record TenantLookupResponse(Guid TenantId, string TenantCode, string Name, string TimeZoneId);
}
