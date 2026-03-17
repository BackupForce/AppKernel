using Application.Abstractions.Authentication;
using Application.Abstractions.Authorization;
using Application.Gaming.Dtos;
using Application.Gaming.Tickets.AvailableForBet;
using Application.Members.Activate;
using Domain.Gaming.Shared;
using Domain.Members;
using Domain.Security;
using MediatR;
using SharedKernel;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Members.Requests;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Admin.Members.Endpoints;

public static class GetAvailableTicketsForBetEndpoint
{
    public static RouteHandlerBuilder MapGetAvailableTicketsForBetEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapGet(
                "/{memberId:guid}/tickets/available-for-bet",
                async (Guid memberId,
                    [AsParameters] GetMemberAvailableTicketsForBetRequest request,
                    IMemberRepository memberRepository,
                    ITenantContext tenantContext,
                    ISender sender,
                    CancellationToken ct) =>
                {
                    Member? member = await memberRepository.GetByIdAsync(tenantContext.TenantId, memberId, ct);
                    if (member is null)
                    {
                        return CustomResults.Problem(Result.Failure(GamingErrors.MemberNotFound));
                    }

                    GetAvailableTicketsForBetQuery query = new GetAvailableTicketsForBetQuery(
                        tenantContext.TenantId,
                        member.Id,
                        request.DrawId,
                        request.Limit);
                    return await UseCaseInvoker.Send<GetAvailableTicketsForBetQuery, AvailableTicketsResponse>(
                        query,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Tickets.Read.Name)
            .Produces<AvailableTicketsResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithName("AdminGetMemberAvailableTicketsForBet");
    }
}
