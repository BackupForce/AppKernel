using Domain.Security;

namespace Application.Authorization;

// 中文註解：提供 UI 友善的權限目錄資料，並在初始化時做完整驗證。
public sealed class PermissionUiCatalogProvider
{
    private static readonly PermissionCatalogDto Catalog = BuildCatalog();

    public PermissionCatalogDto GetCatalog()
    {
        return Catalog;
    }

    private static PermissionCatalogDto BuildCatalog()
    {
        PermissionCatalogDto catalog = new PermissionCatalogDto(
            "1.0",
            new List<ScopeGroupDto>
            {
                BuildPlatformScope(),
                BuildTenantScope(),
                BuildSelfScope()
            });

        ValidateCatalog(catalog);

        return catalog;
    }

    private static ScopeGroupDto BuildPlatformScope()
    {
        ModuleGroupDto tenantsModule = new ModuleGroupDto(
            "TENANTS",
            "租戶管理",
            "TENANTS:*",
            new List<PermissionItemDto>
            {
                new PermissionItemDto(
                    "TENANTS:CREATE",
                    "建立租戶",
                    "建立新的租戶",
                    10,
                    false,
                    false)
            });

        return new ScopeGroupDto(
            PermissionScope.Platform,
            "平台",
            new List<ModuleGroupDto> { tenantsModule });
    }

    private static ScopeGroupDto BuildTenantScope()
    {
        ModuleGroupDto usersModule = new ModuleGroupDto(
            "USERS",
            "使用者管理",
            "USERS:*",
            new List<PermissionItemDto>
            {
                new PermissionItemDto(
                    "USERS:VIEW",
                    "檢視使用者",
                    "檢視使用者資料",
                    10,
                    false,
                    false),
                new PermissionItemDto(
                    "USERS:CREATE",
                    "建立使用者",
                    "建立使用者",
                    20,
                    false,
                    false),
                new PermissionItemDto(
                    "USERS:UPDATE",
                    "更新使用者",
                    "更新使用者資料",
                    30,
                    false,
                    false),
                new PermissionItemDto(
                    "USERS:DELETE",
                    "刪除使用者",
                    "刪除使用者",
                    40,
                    true,
                    false),
                new PermissionItemDto(
                    "USERS:RESET-PASSWORD",
                    "重設密碼",
                    "重設使用者密碼",
                    50,
                    true,
                    false)
            });

        ModuleGroupDto membersModule = new ModuleGroupDto(
            "MEMBERS",
            "會員管理",
            "MEMBERS:*",
            new List<PermissionItemDto>
            {
                new PermissionItemDto(
                    "MEMBERS:READ",
                    "檢視會員",
                    "檢視會員資料",
                    10,
                    false,
                    false),
                new PermissionItemDto(
                    "MEMBERS:CREATE",
                    "建立會員",
                    "建立會員",
                    20,
                    false,
                    false),
                new PermissionItemDto(
                    "MEMBERS:UPDATE",
                    "更新會員",
                    "更新會員資料",
                    30,
                    false,
                    false),
                new PermissionItemDto(
                    "MEMBERS:SUSPEND",
                    "停權會員",
                    "停權或解除停權會員",
                    40,
                    false,
                    false)
            });

        ModuleGroupDto memberPointsModule = new ModuleGroupDto(
            "MEMBER_POINTS",
            "會員點數",
            "MEMBER_POINTS:*",
            new List<PermissionItemDto>
            {
                new PermissionItemDto(
                    "MEMBER_POINTS:READ",
                    "檢視會員點數",
                    "檢視會員點數",
                    10,
                    false,
                    false),
                new PermissionItemDto(
                    "MEMBER_POINTS:ADJUST",
                    "調整會員點數",
                    "人工調整會員點數",
                    20,
                    false,
                    false),
                new PermissionItemDto(
                    "MEMBER_POINTS:TRANSFER",
                    "點數轉帳",
                    "會員點數轉帳",
                    30,
                    false,
                    false)
            });

        ModuleGroupDto memberAssetsModule = new ModuleGroupDto(
            "MEMBER_ASSETS",
            "會員資產",
            "MEMBER_ASSETS:*",
            new List<PermissionItemDto>
            {
                new PermissionItemDto(
                    "MEMBER_ASSETS:READ",
                    "檢視會員資產",
                    "檢視會員資產",
                    10,
                    false,
                    false),
                new PermissionItemDto(
                    "MEMBER_ASSETS:ADJUST",
                    "調整會員資產",
                    "調整會員資產",
                    20,
                    false,
                    false)
            });

        ModuleGroupDto memberAuditModule = new ModuleGroupDto(
            "MEMBER_AUDIT",
            "會員稽核",
            "MEMBER_AUDIT:*",
            new List<PermissionItemDto>
            {
                new PermissionItemDto(
                    "MEMBER_AUDIT:READ",
                    "檢視稽核紀錄",
                    "檢視會員操作歷程",
                    10,
                    false,
                    false)
            });

        ModuleGroupDto rolesModule = new ModuleGroupDto(
            "ROLES",
            "角色管理",
            "ROLES:*",
            new List<PermissionItemDto>
            {
                new PermissionItemDto(
                    "ROLES:VIEW",
                    "檢視角色",
                    "檢視角色",
                    10,
                    false,
                    false),
                new PermissionItemDto(
                    "ROLES:CREATE",
                    "建立角色",
                    "建立角色",
                    20,
                    false,
                    false),
                new PermissionItemDto(
                    "ROLES:UPDATE",
                    "更新角色",
                    "更新角色",
                    30,
                    false,
                    false),
                new PermissionItemDto(
                    "ROLES:DELETE",
                    "刪除角色",
                    "刪除角色",
                    40,
                    true,
                    false)
            });

        return new ScopeGroupDto(
            PermissionScope.Tenant,
            "租戶",
            new List<ModuleGroupDto>
            {
                usersModule,
                membersModule,
                memberPointsModule,
                memberAssetsModule,
                memberAuditModule,
                rolesModule
            });
    }

    private static ScopeGroupDto BuildSelfScope()
    {
        ModuleGroupDto pointsModule = new ModuleGroupDto(
            "POINTS",
            "自身點數",
            "POINTS:ME:*",
            new List<PermissionItemDto>
            {
                new PermissionItemDto(
                    "POINTS:ME:VIEW",
                    "檢視點數",
                    "檢視自身點數",
                    10,
                    false,
                    false)
            });

        return new ScopeGroupDto(
            PermissionScope.Self,
            "個人",
            new List<ModuleGroupDto> { pointsModule });
    }

    private static void ValidateCatalog(PermissionCatalogDto catalog)
    {
        HashSet<string> knownCodes = new HashSet<string>(PermissionCatalog.AllPermissionCodes);

        foreach (ScopeGroupDto scopeGroup in catalog.Scopes)
        {
            foreach (ModuleGroupDto module in scopeGroup.Modules)
            {
                foreach (PermissionItemDto item in module.Items)
                {
                    string normalizedCode = NormalizeCode(item.Code);

                    if (!knownCodes.Contains(normalizedCode))
                    {
                        throw new InvalidOperationException(
                            $"Permission catalog contains unknown code: {normalizedCode}");
                    }

                    if (!PermissionCatalog.TryGetScope(normalizedCode, out PermissionScope scope))
                    {
                        throw new InvalidOperationException(
                            $"Permission catalog contains unknown scope for code: {normalizedCode}");
                    }

                    if (scope != scopeGroup.Scope)
                    {
                        throw new InvalidOperationException(
                            $"Permission catalog scope mismatch for code: {normalizedCode}");
                    }
                }
            }
        }
    }

    private static string NormalizeCode(string code)
    {
        // 中文註解：權限代碼統一 Trim 後轉大寫，避免大小寫差異。
        return string.IsNullOrWhiteSpace(code)
            ? string.Empty
            : code.Trim().ToUpperInvariant();
    }
}
