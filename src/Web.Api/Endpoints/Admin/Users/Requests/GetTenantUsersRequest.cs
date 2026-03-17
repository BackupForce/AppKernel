namespace Web.Api.Endpoints.Admin.Users.Requests;

public sealed record GetTenantUsersRequest(int Page = 1, int PageSize = 20);
