namespace Domain.Security;

public sealed partial class Permission
{
    public static class GamingWinnings
    {
        public static readonly Permission All = new(207, "GAMING:WINNINGS:*", "中獎兌獎所有權限", PermissionScope.Tenant);
        public static readonly Permission WinningNumbersRead = new(237, "GAMING:WINNINGS:WINNING_NUMBERS_READ", "後台檢視開獎號碼", PermissionScope.Tenant);
        public static readonly Permission Read = new(238, "GAMING:WINNINGS:READ", "後台檢視中獎兌獎資料", PermissionScope.Tenant);
        public static readonly Permission Redeem = new(239, "GAMING:WINNINGS:REDEEM", "後台執行中獎兌獎", PermissionScope.Tenant);
        public static readonly Permission RedeemedRead = new(240, "GAMING:WINNINGS:REDEEMED_READ", "後台依兌換時間檢視已兌獎資料", PermissionScope.Tenant);

        public static IEnumerable<Permission> AllPermissions => new[]
        {
            All, WinningNumbersRead, Read, Redeem, RedeemedRead
        };
    }
}
