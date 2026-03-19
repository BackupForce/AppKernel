using Application.Abstractions.Authorization;
using Application.Members.Tags.Create;
using Domain.Security;
using MediatR;
using SharedKernel;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Members.Requests;
using Web.Api.Endpoints.Admin.MemberTags.Requests;

namespace Web.Api.Endpoints.Admin.MemberTags.Endpoints;

public static class CreateMemberTagEndpoint
{
    public static RouteHandlerBuilder MapCreateMemberTagEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapPost(
                "/member-tags",
                async (Guid tenantId, CreateMemberTagRequest request, ISender sender, CancellationToken ct) =>
                {
                    CreateMemberTagCommand command = new(tenantId, request.TagCode, request.DisplayName);
                    return await UseCaseInvoker.Send<CreateMemberTagCommand, Guid>(
                        command,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.MemberTag.Create.Name)
            .Produces<Guid>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("AdminCreateMemberTag");
    }
}
