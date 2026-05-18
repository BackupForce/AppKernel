namespace Web.Api.Endpoints.Admin.Roles.Requests;

public sealed record UpdateRolePermissionsRequest(IReadOnlyCollection<string> PermissionCodes);
