namespace Web.Api.Endpoints.Roles.Requests;

public sealed record UpdateRolePermissionsRequest(List<string> PermissionCodes);
