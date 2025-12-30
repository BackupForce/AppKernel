using MediatR;
using Web.Api.Endpoints.Users;

namespace Web.Api.Endpoints.Account;

public class Me : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/account/me", (HttpContext http) =>
        {
            string userId = http.User.FindFirst("sub")?.Value;
            return $"Hi, userId = {userId}";
        })
        .WithGroupName("frontend-v1")
        .RequireAuthorization()// ✅ 僅需登入即可
        .WithTags(Tags.Users);
    }
}
