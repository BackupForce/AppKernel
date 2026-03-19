namespace Domain.Security;

public sealed partial class Permission
{
    public static class GamingTicket
    {
        public static readonly Permission All = new(205, "GAMING:TICKET:*", "票券所有權限", PermissionScope.Tenant);
        public static readonly Permission Read = new(228, "GAMING:TICKET:READ", "後台查詢票券", PermissionScope.Tenant);
        public static readonly Permission Place = new(229, "GAMING:TICKET:PLACE", "後台代客下注", PermissionScope.Tenant);
        public static readonly Permission Cancel = new(241, "GAMING:TICKET:CANCEL", "後台取消票券", PermissionScope.Tenant);
        public static readonly Permission Manage = new(242, "GAMING:TICKET:MANAGE", "管理票券", PermissionScope.Tenant);

        public static IEnumerable<Permission> AllPermissions => new[]
        {
            All, Read, Place, Cancel, Manage
        };
    }
}
