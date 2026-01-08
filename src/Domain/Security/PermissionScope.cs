namespace Domain.Security;

public enum PermissionScope
{
    // 中文註解：平台級權限，不依賴租戶。
    Platform = 0,
    // 中文註解：租戶級權限，必須帶入租戶識別碼。
    Tenant = 1,
    // 中文註解：自身權限，必須是呼叫者本人。
    Self = 2
}
