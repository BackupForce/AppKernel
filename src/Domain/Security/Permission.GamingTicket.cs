namespace Domain.Security;

public sealed partial class Permission
{
    public static class GamingTicket
    {
        public static readonly Permission All = new(205, "GAMING_TICKET:*", "票券所有權限", PermissionScope.Tenant);
        public static readonly Permission Read = new(228, "GAMING_TICKET:READ", "後台查詢票券", PermissionScope.Tenant);
        public static readonly Permission Place = new(229, "GAMING_TICKET:PLACE", "後台代客下注", PermissionScope.Tenant);
        public static readonly Permission Cancel = new(241, "GAMING_TICKET:CANCEL", "後台取消票券", PermissionScope.Tenant);
        public static readonly Permission Manage = new(242, "GAMING_TICKET:MANAGE", "管理票券", PermissionScope.Tenant);

        public static IEnumerable<Permission> AllPermissions => new[]
        {
            All, Read, Place, Cancel, Manage
        };
    }
}
