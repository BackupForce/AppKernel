namespace Web.Api.Endpoints.Admin.Roles.Requests;

public sealed record UpdateRolePermissionsRequest(List<string> PermissionCodes);
