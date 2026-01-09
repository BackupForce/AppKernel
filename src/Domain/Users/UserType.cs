namespace Domain.Users;

public enum UserType
{
    // 中文註解：平台級使用者，與租戶無關。
    Platform = 0,
    // 中文註解：租戶管理者或租戶側使用者。
    Tenant = 1,
    // 中文註解：會員端使用者。
    Member = 2
}

public static class UserTypeParser
{
    public static UserType Parse(string raw)
    {
        // 中文註解：字串解析失敗時直接丟例外，確保 Fail Closed。
        if (TryParse(raw, out UserType parsed))
        {
            return parsed;
        }

        throw new InvalidOperationException($"無法解析的 UserType：{raw}");
    }

    public static bool TryParse(string? raw, out UserType parsed)
    {
        parsed = UserType.Tenant;

        if (string.IsNullOrWhiteSpace(raw))
        {
            return false;
        }

        string normalized = raw.Trim().ToUpperInvariant();
        switch (normalized)
        {
            case "PLATFORM":
                parsed = UserType.Platform;
                return true;
            case "TENANT":
                parsed = UserType.Tenant;
                return true;
            case "MEMBER":
                parsed = UserType.Member;
                return true;
            default:
                return false;
        }
    }
}
