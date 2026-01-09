namespace Application.Abstractions.Authentication;

public static class JwtClaimNames
{
    // 中文註解：統一 JWT claim 名稱，避免散落造成拼寫錯誤。
    public const string UserType = "user_type";
    public const string TenantId = "tenant_id";
}
