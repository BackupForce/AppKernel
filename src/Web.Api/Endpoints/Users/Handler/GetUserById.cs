using Application.Users.GetById;
using Web.Api.Common;

namespace Web.Api.Endpoints.Users.Handler;

public static class GetUserById
{
    public static void Handler()
    {
        UseCaseInvoker.FromRoute<GetUserByIdQuery, Guid, UserResponse>(id => new GetUserByIdQuery(id));
    }
}
