using Domain.Security;

namespace Application.Authorization;

// 中文註解：提供 UI 友善的權限目錄資料，並在初始化時做完整驗證。
public sealed class PermissionUiCatalogProvider
{
    private static readonly PermissionCatalogDto Catalog = BuildCatalog();

    public static PermissionCatalogDto GetCatalog()
    {
        return Catalog;
    }

    private static PermissionCatalogDto BuildCatalog()
    {
        PermissionCatalogDto catalog = new PermissionCatalogDto(
            "2.0",
            new List<ScopeGroupDto>
            {
                BuildPlatformScope(),
                BuildTenantScope()
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
                Item("TENANTS:CREATE", "建立租戶", "建立新的租戶")
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
                Item("USERS:VIEW", "檢視使用者", "檢視使用者資料"),
                Item("USERS:CREATE", "建立使用者", "建立使用者"),
                Item("USERS:UPDATE", "更新使用者", "更新使用者資料"),
                Item("USERS:DELETE", "刪除使用者", "刪除使用者", true),
                Item("USERS:RESET_PASSWORD", "重設密碼", "重設使用者密碼", true)
            });

        ModuleGroupDto rolesModule = new ModuleGroupDto(
            "ROLES",
            "角色管理",
            "ROLES:*",
            new List<PermissionItemDto>
            {
                Item("ROLES:VIEW", "檢視角色", "檢視角色"),
                Item("ROLES:CREATE", "建立角色", "建立角色"),
                Item("ROLES:UPDATE", "更新角色", "更新角色"),
                Item("ROLES:DELETE", "刪除角色", "刪除角色", true)
            });

        ModuleGroupDto membersModule = new ModuleGroupDto(
            "MEMBERS",
            "會員管理",
            "MEMBERS:*",
            new List<PermissionItemDto>
            {
                Item("MEMBERS:VIEW", "檢視會員", "檢視會員資料"),
                Item("MEMBERS:CREATE", "建立會員", "建立會員"),
                Item("MEMBERS:UPDATE", "更新會員", "更新會員資料"),
                Item("MEMBERS:SUSPEND", "停權會員", "停權或解除停權會員", true)
            });


        ModuleGroupDto memberPointsModule = new ModuleGroupDto(
            "MEMBER_POINTS",
            "會員點數",
            "MEMBER_POINTS:*",
            new List<PermissionItemDto>
            {
                Item("MEMBER_POINTS:READ", "檢視會員點數", "檢視會員點數"),
                Item("MEMBER_POINTS:ADJUST", "調整會員點數", "人工調整會員點數", true),
                Item("MEMBER_POINTS:TRANSFER", "點數轉帳", "會員點數轉帳", true)
            });

        ModuleGroupDto memberAssetsModule = new ModuleGroupDto(
            "MEMBER_ASSETS",
            "會員資產",
            "MEMBER_ASSETS:*",
            new List<PermissionItemDto>
            {
                Item("MEMBER_ASSETS:READ", "檢視會員資產", "檢視會員資產"),
                Item("MEMBER_ASSETS:ADJUST", "調整會員資產", "調整會員資產", true)
            });

        ModuleGroupDto pointsModule = new ModuleGroupDto(
            "POINTS",
            "自身點數",
            "POINTS:ME:*",
            new List<PermissionItemDto>
            {
                Item("POINTS:ME:VIEW", "檢視點數", "檢視自身點數")
            });

        ModuleGroupDto memberAuditModule = new ModuleGroupDto(
            "MEMBER_AUDIT",
            "會員稽核",
            "MEMBER_AUDIT:*",
            new List<PermissionItemDto>
            {
                Item("MEMBER_AUDIT:READ", "檢視稽核紀錄", "檢視會員操作歷程")
            });

        ModuleGroupDto gamingDrawModule = new ModuleGroupDto(
            "DRAW",
            "遊戲期數",
            "GAMING:DRAW:*",
            new List<PermissionItemDto>
            {
                Item("GAMING:DRAW:CREATE", "建立期數", "建立遊戲期數"),
                Item("GAMING:DRAW:EXECUTE", "執行開獎", "執行期數開獎", true),
                Item("GAMING:DRAW:SETTLE", "結算開獎", "結算期數", true),
                Item("GAMING:DRAW:MANUAL_CLOSE", "手動封盤", "手動封盤期數", true),
                Item("GAMING:DRAW:REOPEN", "重新開盤", "重新開盤期數", true),
                Item("GAMING:DRAW:UPDATE_ALLOWED_TEMPLATES", "更新允許票種", "更新期數允許票種", true),
                Item("GAMING:DRAW:MANAGE", "管理期數", "高階管理期數", true)
            });

        ModuleGroupDto gamingDrawGroupModule = new ModuleGroupDto(
            "DRAW_GROUP",
            "期數群組",
            "GAMING:DRAW_GROUP:*",
            new List<PermissionItemDto>
            {
                Item("GAMING:DRAW_GROUP:READ", "檢視期數群組", "檢視期數群組"),
                Item("GAMING:DRAW_GROUP:CREATE", "建立期數群組", "建立期數群組"),
                Item("GAMING:DRAW_GROUP:UPDATE", "更新期數群組", "更新期數群組"),
                Item("GAMING:DRAW_GROUP:DELETE", "刪除期數群組", "刪除期數群組", true),
                Item("GAMING:DRAW_GROUP:ACTIVATE", "啟用期數群組", "啟用期數群組", true),
                Item("GAMING:DRAW_GROUP:END", "結束期數群組", "結束期數群組", true),
                Item("GAMING:DRAW_GROUP:MANAGE", "管理期數群組", "高階管理期數群組", true),
                Item("GAMING:DRAW_GROUP_DRAW:MANAGE", "管理群組期數", "管理期數群組期數", true)
            });

        ModuleGroupDto gamingTicketModule = new ModuleGroupDto(
            "TICKET",
            "票券管理",
            "GAMING:TICKET:*",
            new List<PermissionItemDto>
            {
                Item("GAMING:TICKET:READ", "查詢票券", "後台查詢票券"),
                Item("GAMING:TICKET:PLACE", "代客下注", "後台代客下注"),
                Item("GAMING:TICKET:CANCEL", "取消票券", "後台取消票券", true),
                Item("GAMING:TICKET:MANAGE", "管理票券", "高階管理票券", true)
            });

        ModuleGroupDto gamingTicketClaimEventModule = new ModuleGroupDto(
            "TICKET_CLAIM_EVENT",
            "領券活動",
            "GAMING:TICKET_CLAIM_EVENT:*",
            new List<PermissionItemDto>
            {
                Item("GAMING:TICKET_CLAIM_EVENT:READ", "檢視領券活動", "檢視領券活動"),
                Item("GAMING:TICKET_CLAIM_EVENT:CREATE", "建立領券活動", "建立領券活動"),
                Item("GAMING:TICKET_CLAIM_EVENT:UPDATE", "更新領券活動", "更新領券活動"),
                Item("GAMING:TICKET_CLAIM_EVENT:ACTIVATE", "啟用領券活動", "啟用領券活動", true),
                Item("GAMING:TICKET_CLAIM_EVENT:DISABLE", "停用領券活動", "停用領券活動", true),
                Item("GAMING:TICKET_CLAIM_EVENT:END", "結束領券活動", "結束領券活動", true),
                Item("GAMING:TICKET_CLAIM_EVENT:MANAGE", "管理領券活動", "高階管理領券活動", true),
                Item("GAMING:TICKET_CLAIM_EVENT_CLAIM:READ", "檢視領券紀錄", "檢視領券紀錄")
            });

        ModuleGroupDto gamingWinningsModule = new ModuleGroupDto(
            "WINNINGS",
            "中獎兌獎",
            "GAMING:WINNINGS:*",
            new List<PermissionItemDto>
            {
                Item("GAMING:WINNINGS:READ", "檢視中獎兌獎", "後台查詢中獎與兌獎清單"),
                Item("GAMING:WINNINGS:REDEEM", "執行兌獎", "後台執行中獎兌獎", true),
                Item("GAMING:WINNING_NUMBERS:READ", "檢視開獎號碼", "後台檢視開獎號碼"),
                Item("GAMING:WINNING_REDEEMED:READ", "檢視已兌獎", "依兌換時間區間查詢已兌獎資料")
            });

        ModuleGroupDto gamingSystemModule = new ModuleGroupDto(
            "GAMING_SYSTEM",
            "遊戲系統",
            "GAMING:*",
            new List<PermissionItemDto>
            {
                Item("GAMING:CATALOG:VIEW", "檢視遊戲目錄", "檢視平台遊戲與玩法清單"),
                Item("GAMING:ENTITLEMENT:MANAGE", "管理租戶啟用", "啟用或停用租戶遊戲/玩法", true),
                Item("GAMING:DRAW_TEMPLATE:MANAGE", "管理期數模板", "管理期數模板", true)
            });

        ModuleGroupDto legacyTicketsModule = new ModuleGroupDto(
            "TICKETS",
            "票券後台（相容）",
            "TICKETS:*",
            new List<PermissionItemDto>
            {
                Item("TICKETS:READ", "查詢票券", "舊版票券查詢權限"),
                Item("TICKETS:ISSUE", "發放票券", "舊版後台發放票券", true),
                Item("TICKETS:PLACE_BET", "代客下注", "舊版後台代客下注", true)
            });

        return new ScopeGroupDto(
            PermissionScope.Tenant,
            "租戶",
            new List<ModuleGroupDto>
            {
                usersModule,
                rolesModule,
                membersModule,
                memberAuditModule,
                memberPointsModule,
                memberAssetsModule,
                pointsModule,
                gamingSystemModule,
                gamingDrawModule,
                gamingDrawGroupModule,
                gamingTicketModule,
                gamingTicketClaimEventModule,
                gamingWinningsModule,
                legacyTicketsModule
            });
    }

    private static PermissionItemDto Item(
        string code,
        string displayName,
        string description,
        bool isDangerous = false,
        bool hidden = false)
    {
        return new PermissionItemDto(
            code,
            displayName,
            description,
            GetOrder(code),
            isDangerous,
            hidden);
    }

    private static int GetOrder(string code)
    {
        string action = NormalizeCode(code).Split(':').LastOrDefault() ?? string.Empty;

        return action switch
        {
            "READ" or "VIEW" => 10,
            "CREATE" or "PLACE" => 20,
            "UPDATE" => 30,
            "DELETE" or "CANCEL" => 40,
            "MANAGE" => 50,
            "EXECUTE" => 51,
            "SETTLE" => 52,
            _ => 60
        };
    }

    private static void ValidateCatalog(PermissionCatalogDto catalog)
    {
        HashSet<string> knownCodes = PermissionCatalog.AllPermissionCodes
            .Select(PermissionCatalog.NormalizeCode)
            .ToHashSet();

        HashSet<string> uiCodes = new HashSet<string>();

        foreach (ScopeGroupDto scopeGroup in catalog.Scopes)
        {
            foreach (ModuleGroupDto module in scopeGroup.Modules)
            {
                if (!string.IsNullOrWhiteSpace(module.MasterPermissionCode))
                {
                    string normalizedMasterCode = NormalizeCode(module.MasterPermissionCode);
                    ValidateCodeExists(knownCodes, normalizedMasterCode);
                    ValidateScope(scopeGroup.Scope, normalizedMasterCode);
                    uiCodes.Add(normalizedMasterCode);
                }

                foreach (PermissionItemDto item in module.Items)
                {
                    string normalizedCode = NormalizeCode(item.Code);
                    ValidateCodeExists(knownCodes, normalizedCode);
                    ValidateScope(scopeGroup.Scope, normalizedCode);
                    uiCodes.Add(normalizedCode);
                }
            }
        }

        if (!knownCodes.SetEquals(uiCodes))
        {
            IEnumerable<string> missingInUi = knownCodes.Except(uiCodes).OrderBy(code => code);
            IEnumerable<string> unknownInUi = uiCodes.Except(knownCodes).OrderBy(code => code);
            throw new InvalidOperationException(
                $"Permission catalog mismatch. MissingInUi=[{string.Join(',', missingInUi)}], UnknownInUi=[{string.Join(',', unknownInUi)}]");
        }
    }

    private static void ValidateCodeExists(HashSet<string> knownCodes, string normalizedCode)
    {
        if (!knownCodes.Contains(normalizedCode))
        {
            throw new InvalidOperationException(
                $"Permission catalog contains unknown code: {normalizedCode}");
        }
    }

    private static void ValidateScope(PermissionScope scopeGroupScope, string normalizedCode)
    {
        if (!PermissionCatalog.TryGetScope(normalizedCode, out PermissionScope scope))
        {
            throw new InvalidOperationException(
                $"Permission catalog contains unknown scope for code: {normalizedCode}");
        }

        if (scope != scopeGroupScope)
        {
            throw new InvalidOperationException(
                $"Permission catalog scope mismatch for code: {normalizedCode}");
        }
    }

    private static string NormalizeCode(string code)
    {
        return PermissionCatalog.NormalizeCode(code);
    }
}
