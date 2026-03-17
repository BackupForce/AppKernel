using Application.Members.Create;
using Domain.Security;
using MediatR;
using Web.Api.Common;
using Web.Api.Endpoints.Admin.Members.Requests;

namespace Web.Api.Endpoints.Admin.Members.Endpoints;

public static class CreateMemberEndpoint
{
    public static RouteHandlerBuilder MapCreateMemberEndpoint(
        this RouteGroupBuilder group)
    {
        return group.MapPost(
                "/",
                async (CreateMemberRequest request, ISender sender, CancellationToken ct) =>
                {
                    var command = new CreateMemberCommand(request.UserId, request.DisplayName, request.MemberNo);
                    return await UseCaseInvoker.Send<CreateMemberCommand, Guid>(
                        command,
                        sender,
                        value => Results.Ok(value),
                        ct);
                })
            .RequireAuthorization(Permission.Members.Create.Name)
            .Produces<Guid>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithName("CreateMember");
    }
}
