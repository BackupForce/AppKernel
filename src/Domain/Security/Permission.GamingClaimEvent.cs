namespace Domain.Security;

public sealed partial class Permission
{
    public static class GamingTicketClaimEvent
    {
        public static readonly Permission All = new(206, "GAMING_TICKET_CLAIM_EVENT:*", "領券活動所有權限", PermissionScope.Tenant);
        public static readonly Permission Read = new(230, "GAMING_TICKET_CLAIM_EVENT:READ", "檢視領券活動", PermissionScope.Tenant);
        public static readonly Permission Create = new(231, "GAMING_TICKET_CLAIM_EVENT:CREATE", "建立領券活動", PermissionScope.Tenant);
        public static readonly Permission Update = new(232, "GAMING_TICKET_CLAIM_EVENT:UPDATE", "更新領券活動", PermissionScope.Tenant);
        public static readonly Permission Activate = new(233, "GAMING_TICKET_CLAIM_EVENT:ACTIVATE", "啟用領券活動", PermissionScope.Tenant);
        public static readonly Permission Disable = new(234, "GAMING_TICKET_CLAIM_EVENT:DISABLE", "停用領券活動", PermissionScope.Tenant);
        public static readonly Permission End = new(235, "GAMING_TICKET_CLAIM_EVENT:END", "結束領券活動", PermissionScope.Tenant);
        public static readonly Permission ClaimRead = new(236, "GAMING_TICKET_CLAIM_EVENT:CLAIM_READ", "檢視領券紀錄", PermissionScope.Tenant);
        public static readonly Permission Manage = new(243, "GAMING_TICKET_CLAIM_EVENT:MANAGE", "管理領券活動", PermissionScope.Tenant);

        public static IEnumerable<Permission> AllPermissions => new[]
        {
            All, Read, Create, Update, Activate, Disable, End, ClaimRead, Manage
        };
    }
}
