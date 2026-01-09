namespace Application.Abstractions.Authorization;

public static class AuthorizationPolicyNames
{
    // 中文註解：平台與租戶/會員分流的 Policy 名稱。
    public const string Platform = "Platform";
    public const string TenantUser = "TenantUser";
    public const string Member = "Member";
    public const string MemberActive = "MemberActive";
    public const string MemberOwner = "MemberOwner";
}
