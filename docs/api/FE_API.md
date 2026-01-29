# FE API 文件

## 1. 全域規範

### 1.1 Base URL / Environment
- 所有 Minimal API 皆掛在版本化路由：`/api/v{version}`。`version` 由 URL segment 決定（例如 `/api/v1/...`、`/api/v2/...`）。【F:src/Web.Api/Program.cs†L39-L50】
- 目前端點宣告的版本：
  - v1：Auth、Tenants、Permissions、Roles、Members、Reports、Gaming、Account 等群組（見各端點定義）。【F:src/Web.Api/Endpoints/Auth/Login.cs†L11-L29】【F:src/Web.Api/Endpoints/Tenants/TenantsEndpoints.cs†L12-L66】【F:src/Web.Api/Endpoints/Permissions/PermissionsEndpoints.cs†L9-L27】【F:src/Web.Api/Endpoints/Roles/RolesEndpoints.cs†L17-L153】【F:src/Web.Api/Endpoints/Members/MembersEndpoints.cs†L28-L223】【F:src/Web.Api/Endpoints/Reports/DailyReportsEndpoints.cs†L10-L30】【F:src/Web.Api/Endpoints/Gaming/GamingEndpoints.cs†L31-L494】【F:src/Web.Api/Endpoints/Account/Me.cs†L9-L19】
  - v2：Users 群組。`/api/v2/users/...`。【F:src/Web.Api/Endpoints/Users/UsersEndpoints.cs†L13-L92】
- 例外：`/health` 為健康檢查端點，不在版本化群組之下。【F:src/Web.Api/Program.cs†L66-L70】

### 1.2 認證（JWT）
**如何取得 token**
- 透過 `POST /api/v1/auth/login` 取得管理者（Tenant User / Platform）JWT。【F:src/Web.Api/Endpoints/Auth/Login.cs†L11-L29】
- 透過 `POST /api/v1/{tenantId}/auth/line-login` 取得 Member JWT（LINE 登入）。【F:src/Web.Api/Endpoints/Auth/LineLogin.cs†L10-L33】
- 透過 `POST /api/v1/tenants/{tenantId}/auth/line/liff-login` 取得 Member JWT（LIFF 登入即註冊）。【F:src/Web.Api/Endpoints/Auth/LineLiffLogin.cs†L1-L57】

**Authorization header 格式**
- `Authorization: Bearer <JWT>`（標準 JWT Bearer）。【F:src/Infrastructure/Extensions/JwtAuthenticationExtensions.cs†L13-L47】

**JWT claims（可從程式碼確認）**
- `nameidentifier`（ClaimTypes.NameIdentifier）= 使用者 Id（Guid）。【F:src/Infrastructure/Authentication/JwtService.cs†L32-L40】
- `name`（ClaimTypes.Name）= 使用者名稱。【F:src/Infrastructure/Authentication/JwtService.cs†L32-L40】
- `user_type`（JwtClaimNames.UserType）= `Platform | Tenant | Member`。【F:src/Infrastructure/Authentication/JwtService.cs†L32-L40】【F:src/Application/Abstractions/Authentication/JwtClaimNames.cs†L1-L9】【F:src/Domain/Users/UserType.cs†L1-L33】
- `tenant_id`（JwtClaimNames.TenantId）= 非 Platform 使用者必填（Guid）。【F:src/Infrastructure/Authentication/JwtService.cs†L41-L49】【F:src/Application/Abstractions/Authentication/JwtUserContext.cs†L36-L64】
- `role`（ClaimTypes.Role）= 逗號分隔的角色名稱字串。【F:src/Infrastructure/Authentication/JwtService.cs†L32-L40】
- `nodes` = 逗號分隔 node id 列表（Guid 字串）。【F:src/Infrastructure/Authentication/JwtService.cs†L32-L40】
- `permissions` = 逗號分隔 permission code 列表。【F:src/Infrastructure/Authentication/JwtService.cs†L32-L40】

**JWT 驗證規則**
- Issuer/Audience/Secret 由 `JwtSettings` 設定（在 `appsettings.*.json` 內）。【F:src/Infrastructure/Extensions/JwtAuthenticationExtensions.cs†L13-L42】【F:src/Infrastructure/Settings/JwtSettings.cs†L7-L12】【F:src/Web.Api/appsettings.Development.json†L31-L38】
- 透過 `JwtUserContext.TryFromClaims` 強制驗證 `user_type` / `tenant_id`（非 Platform 必須帶 tenant_id）。【F:src/Infrastructure/Extensions/JwtAuthenticationExtensions.cs†L33-L45】【F:src/Application/Abstractions/Authentication/JwtUserContext.cs†L27-L73】

**Unknown / Needs confirmation**
- 是否有 refresh token 機制？程式碼中未找到 refresh token 端點或邏輯，需確認需求端或其他專案模組。【F:src/Web.Api/Endpoints/Auth/Login.cs†L11-L29】【F:src/Web.Api/Endpoints/Auth/LineLogin.cs†L10-L33】
- `LoginResponse.Expiration` 欄位在 handler 中未賦值，回傳可能為預設值（`0001-01-01T00:00:00`）。需確認前端是否依賴此欄位。【F:src/Application/Auth/LoginResponse.cs†L1-L9】【F:src/Application/Auth/LoginCommandHandler.cs†L72-L85】

### 1.3 Tenant 規範
- Tenant 解析流程在 `TenantResolutionMiddleware`：
  - 若路由含 `tenantId`：
    - 若同時帶 `X-Tenant-Id`，必須一致，否則回 400。 
    - 若帶 `X-Tenant-Code`，必須為 3 碼英數字且對應同一租戶，否則回 400。
    - 若 `tenantId` 找不到對應租戶，回 404。
  - 若路由不含 `tenantId`：
    - 若 `TenantResolutionOptions.AllowTenantIdHeader` = true 且有 `X-Tenant-Id`，以 header 為 tenant context。
    - 若 header 有 `X-Tenant-Code`，格式正確時以 code 查找租戶並設定 context。
  - 解析到的 TenantId 會寫入 `HttpContext.Items["TenantId"]`，供 `ITenantContext` 使用。 
  【F:src/Web.Api/Middleware/TenantResolutionMiddleware.cs†L12-L121】【F:src/Web.Api/Settings/TenantResolutionOptions.cs†L1-L8】【F:src/Infrastructure/Authentication/TenantContext.cs†L14-L57】
- TenantCode 格式：固定 3 碼英數字（會先 Trim + Uppercase）。【F:src/Web.Api/Common/TenantCodeHelper.cs†L3-L28】
- 注意：`TenantResolutionOptions` 需由設定檔提供，repo 內 `appsettings.json` 未見該設定，是否開放 header 解析需由部署環境確認。【F:src/Web.Api/Extensions/DependencyInjection.cs†L31-L38】【F:src/Web.Api/appsettings.json†L1-L18】

### 1.4 時間與時區（非常重要）
**序列化格式（依目前實作推導）**
- 專案未對 `System.Text.Json` 做自訂設定，採預設序列化行為（ISO 8601）。【F:src/Web.Api/Extensions/DependencyInjection.cs†L12-L83】
- `DateTime`：預設序列化格式類似 `2024-01-01T12:30:00Z` 或 `2024-01-01T12:30:00`，實際是否帶 `Z/offset` 取決於 `DateTime.Kind`。目前程式碼未強制 `Kind`，因此可能出現無時區資訊格式。**建議前端一律送/收 ISO 8601 含 `Z` 或 offset。**【F:src/Application/Gaming/Dtos/DrawDetailDto.cs†L1-L16】【F:src/Application/Members/Dtos/MemberDetailDto.cs†L1-L9】
- `DateTimeOffset`：序列化為含 offset 的 ISO 8601（例如 `2024-01-01T00:00:00+00:00`）。【F:src/Application/Reports/Daily/DailyReportResponse.cs†L1-L10】
- `DateOnly`：Query 參數/JSON 格式為 `YYYY-MM-DD`（ASP.NET Core 預設）。【F:src/Web.Api/Endpoints/Reports/DailyReportsEndpoints.cs†L16-L28】

**時區規範（依程式碼可確認的行為）**
- 每租戶 TimeZoneId 預設為 `UTC`，可由 `PUT /tenants/{tenantId}/settings/timezone` 更新。【F:src/Infrastructure/Tenants/TenantTimeZoneProvider.cs†L12-L47】【F:src/Web.Api/Endpoints/Tenants/TenantsEndpoints.cs†L49-L64】
- 每日報表 `GET /tenants/{tenantId}/reports/daily` 會用租戶時區把 `DateOnly` 轉成 UTC 範圍，回傳 `StartUtc/EndUtc`（`DateTimeOffset`）。【F:src/Application/Reports/Daily/GetDailyReportQueryHandler.cs†L17-L74】

**Unknown / Needs confirmation**
- 其他含 `DateTime` 欄位（如 Draw / Prize / Member 等）是否以 UTC 儲存及回傳，程式碼未明確指出（無統一 converter）。需確認資料庫存放與 API 期望行為。【F:src/Application/Gaming/Dtos/DrawSummaryDto.cs†L1-L12】【F:src/Application/Gaming/Dtos/PrizeDto.cs†L1-L12】【F:src/Application/Members/Dtos/MemberDetailDto.cs†L1-L9】

### 1.5 通用錯誤格式
**ProblemDetails 統一格式**
- 使用 `Results.Problem(...)` 建立回應，並包含 `title / detail / type / status`，符合 RFC7231 格式。驗證錯誤時會額外帶 `errors` 擴充欄位。【F:src/Web.Api/Infrastructure/CustomResults.cs†L8-L70】【F:src/SharedKernel/ValidationError.cs†L1-L18】

**Error 對應**
- `Validation` → `400`，type=`https://tools.ietf.org/html/rfc7231#section-6.5.1`。【F:src/Web.Api/Infrastructure/CustomResults.cs†L24-L52】
- `NotFound` → `404`，type=`https://tools.ietf.org/html/rfc7231#section-6.5.4`。【F:src/Web.Api/Infrastructure/CustomResults.cs†L24-L52】
- `Conflict` → `409`，type=`https://tools.ietf.org/html/rfc7231#section-6.5.8`。【F:src/Web.Api/Infrastructure/CustomResults.cs†L24-L52】
- `Forbidden`（`ErrorType.Forbidden`）目前未在 `CustomResults` 對應，會落入預設 `500`；需前後端協調是否補 403。 【F:src/SharedKernel/ErrorType.cs†L1-L9】【F:src/Web.Api/Infrastructure/CustomResults.cs†L24-L52】
- 其他錯誤 → `500`（Server failure）。【F:src/Web.Api/Infrastructure/CustomResults.cs†L24-L52】【F:src/Web.Api/Infrastructure/GlobalExceptionHandler.cs†L6-L30】

**Validation errors 範例**（`errors` 是 `Error[]`，`type` 為 enum number）
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation.General",
  "status": 400,
  "detail": "One or more validation errors occurred",
  "errors": [
    { "code": "Users.InvalidEmail", "description": "...", "type": 1 }
  ]
}
```
【F:src/Web.Api/Infrastructure/CustomResults.cs†L8-L70】【F:src/SharedKernel/Error.cs†L1-L33】【F:src/SharedKernel/ErrorType.cs†L1-L9】

### 1.6 分頁/排序/搜尋
- 專案共通分頁模型：`PagedResult<T>`，回傳 `Items/TotalCount/Page/PageSize`。【F:src/Application/Abstractions/Data/PagedResult.cs†L1-L29】
- 目前使用分頁的端點（以 `Page/PageSize` 查詢參數為主）：
  - Members 搜尋 / 點數/資產歷史 / 活動紀錄等（見 Members 模組）。【F:src/Web.Api/Endpoints/Members/MembersEndpoints.cs†L68-L222】
- 未見全域排序或 filter 物件，僅有各 endpoint 自帶 query params。

### 1.7 Enum / 常數表
**Domain enums**（會暴露給前端或透過 DTO/Query 使用）
- `UserType`: `Platform(0)`, `Tenant(1)`, `Member(2)`。【F:src/Domain/Users/UserType.cs†L1-L33】
- `MemberStatus`: `Active(0)`, `Suspended(1)`, `Deleted(2)`。【F:src/Domain/Members/MemberStatus.cs†L1-L7】
- `MemberAssetLedgerType`: `Credit(0)`, `Debit(1)`, `AdjustAdd(2)`, `AdjustSub(3)`。【F:src/Domain/Members/MemberAssetLedgerType.cs†L1-L9】
- `MemberPointLedgerType`: `Earn(0)`, `Spend(1)`, `AdjustAdd(2)`, `AdjustSub(3)`, `Refund(4)`。【F:src/Domain/Members/MemberPointLedgerType.cs†L1-L10】
- `GameType`: `Lottery539(1)`。【F:src/Domain/Gaming/GameType.cs†L1-L13】
- `TicketTemplateType`: `Standard(0)`, `Promo(1)`, `Free(2)`, `Vip(3)`, `Event(4)`。【F:src/Domain/Gaming/TicketTemplateType.cs†L1-L12】
- `DrawStatus`: `Scheduled(0)`, `SalesOpen(1)`, `SalesClosed(2)`, `Drawn(3)`, `Cancelled(4)`。【F:src/Domain/Gaming/DrawStatus.cs†L1-L26】
- `AwardStatus`: `Awarded(0)`, `Redeemed(1)`, `Expired(2)`, `Cancelled(3)`。【F:src/Domain/Gaming/AwardStatus.cs†L1-L22】
- `PermissionScope`: `Platform(0)`, `Tenant(1)`, `Self(2)`。【F:src/Domain/Security/PermissionScope.cs†L1-L12】
- `Decision`: `Allow(0)`, `Deny(1)`。【F:src/Domain/Security/Decision.cs†L1-L6】
- `SubjectType`: `User(0)`, `Role(1)`, `Group(2)`。【F:src/Domain/Security/SubjectType.cs†L1-L7】

**i18n 顯示層建議（前端）**
- 建議用 `enum.<EnumName>.<Value>` 做顯示 key，例如：`enum.UserType.Platform`、`enum.DrawStatus.SalesOpen`。對應文字由前端 i18n 資料庫處理。

---

## 2. API Index
> 表內 Path 皆已包含版本前綴 `/api/v{version}`。

| Tag/Module | Method | Path | Auth | Request DTO | Response DTO | Notes |
|---|---|---|---|---|---|---|
| Auth | POST | `/api/v1/auth/login` | None | `LoginRequest` | `LoginResponse` | 需 tenantCode；回傳 JWT。 |
| Auth | POST | `/api/v1/{tenantId}/auth/line-login` | None | `LineLoginRequest` | `LineLoginResponse` | 需 tenantId path；自動建會員。 |
| Auth | POST | `/api/v1/tenants/{tenantId}/auth/line/liff-login` | None | `LineLiffLoginRequest` | `LineLoginResponse` | 需 tenantId path；LIFF 登入即註冊會員。 |
| Tenants | GET | `/api/v1/tenants/by-code/{tenantCode}` | None | N/A | `TenantLookupResponse` | tenantCode=3 碼英數。 |
| Tenants | PUT | `/api/v1/tenants/{tenantId}/settings/timezone` | JWT + Policy(TenantUser) | `UpdateTenantTimeZoneRequest` | N/A | 更新租戶時區。 |
| Users | GET | `/api/v2/users/{id}` | JWT + Policy(TenantUser) | N/A | `UserResponse` | v2 endpoint。 |
| Users | POST | `/api/v2/users` | JWT + Policy(TenantUser) | `CreateUserRequest` | `Guid` | 建立使用者。 |
| Users | POST | `/api/v2/users/tenant` | JWT + Policy(TenantUser) + Permission `USERS:CREATE` | `CreateTenantUserRequest` | `Guid` | 建立租戶使用者。 |
| Users | POST | `/api/v2/users/{userId}/roles/{roleId}` | JWT + Policy(TenantUser) + Permission `USERS:UPDATE` | N/A | `AssignRoleToUserResultDto` | 指派角色。 |
| Roles | POST | `/api/v1/roles` | JWT + Policy(TenantUser) + Permission `ROLES:CREATE` | `CreateRoleRequest` | `int` | 建立角色。 |
| Roles | PUT | `/api/v1/roles/{id}` | JWT + Policy(TenantUser) + Permission `ROLES:UPDATE` | `UpdateRoleRequest` | N/A | 更新角色。 |
| Roles | DELETE | `/api/v1/roles/{id}` | JWT + Policy(TenantUser) + Permission `ROLES:DELETE` | N/A | N/A | 刪除角色。 |
| Roles | GET | `/api/v1/roles/{id}` | JWT + Policy(TenantUser) + Permission `ROLES:VIEW` | N/A | `RoleDetailDto` | 角色詳情。 |
| Roles | GET | `/api/v1/roles` | JWT + Policy(TenantUser) + Permission `ROLES:VIEW` | N/A | `RoleListItemDto[]` | 角色列表。 |
| Roles | GET | `/api/v1/roles/{id}/permissions` | JWT + Policy(TenantUser) + Permission `ROLES:VIEW` | N/A | `string[]` | 角色權限。 |
| Roles | POST | `/api/v1/roles/{id}/permissions` | JWT + Policy(TenantUser) + Permission `ROLES:UPDATE` | `UpdateRolePermissionsRequest` | N/A | 新增權限。 |
| Roles | POST | `/api/v1/roles/{id}/permissions/remove` | JWT + Policy(TenantUser) + Permission `ROLES:UPDATE` | `UpdateRolePermissionsRequest` | N/A | 移除權限。 |
| Permissions | GET | `/api/v1/permissions/catalog` | JWT + Policy(TenantUser) | N/A | `PermissionCatalogDto` | 供 UI 使用的權限目錄。 |
| Members | POST | `/api/v1/members` | JWT + Policy(TenantUser) + Permission `MEMBERS:CREATE` | `CreateMemberRequest` | `Guid` | 建立會員。 |
| Members | GET | `/api/v1/members/{id}` | JWT + Policy(TenantUser) + Permission `MEMBERS:READ` | N/A | `MemberDetailDto` | 會員詳情。 |
| Members | GET | `/api/v1/members` | JWT + Policy(TenantUser) + Permission `MEMBERS:READ` | `SearchMembersRequest`(query) | `PagedResult<MemberListItemDto>` | 分頁查詢。 |
| Members | PUT | `/api/v1/members/{id}` | JWT + Policy(TenantUser) + Permission `MEMBERS:UPDATE` | `UpdateMemberProfileRequest` | N/A | 更新會員。 |
| Members | POST | `/api/v1/members/{id}/suspend` | JWT + Policy(TenantUser) + Permission `MEMBERS:SUSPEND` | `MemberStatusChangeRequest` | N/A | 停權。 |
| Members | POST | `/api/v1/members/{id}/activate` | JWT + Policy(TenantUser) + Permission `MEMBERS:SUSPEND` | `MemberStatusChangeRequest` | N/A | 解除停權。 |
| Members | GET | `/api/v1/members/{id}/points/balance` | JWT + Policy(TenantUser) + Permission `MEMBER_POINTS:READ` | N/A | `MemberPointBalanceDto` | 查詢點數餘額。 |
| Members | GET | `/api/v1/members/{id}/points/history` | JWT + Policy(TenantUser) + Permission `MEMBER_POINTS:READ` | `MemberPointHistoryRequest`(query) | `PagedResult<MemberPointLedgerDto>` | 點數歷史。 |
| Members | POST | `/api/v1/members/{id}/points/adjust` | JWT + Policy(TenantUser) + Permission `MEMBER_POINTS:ADJUST` | `AdjustMemberPointsRequest` | `long` | 調整點數。 |
| Members | GET | `/api/v1/members/{id}/assets` | JWT + Policy(TenantUser) + Permission `MEMBER_ASSETS:READ` | N/A | `MemberAssetBalanceDto[]` | 會員資產。 |
| Members | GET | `/api/v1/members/{id}/assets/{assetCode}/history` | JWT + Policy(TenantUser) + Permission `MEMBER_ASSETS:READ` | `MemberAssetHistoryRequest`(query) | `PagedResult<MemberAssetLedgerDto>` | 資產歷史。 |
| Members | POST | `/api/v1/members/{id}/assets/adjust` | JWT + Policy(TenantUser) + Permission `MEMBER_ASSETS:ADJUST` | `AdjustMemberAssetRequest` | `decimal` | 調整資產。 |
| Members | GET | `/api/v1/members/{id}/activity` | JWT + Policy(TenantUser) + Permission `MEMBER_AUDIT:READ` | `MemberActivityRequest`(query) | `PagedResult<MemberActivityLogDto>` | 操作歷程。 |
| Reports | GET | `/api/v1/tenants/{tenantId}/reports/daily` | JWT + Policy(TenantUser) | Query: `date` | `DailyReportResponse` | 依租戶時區計算。 |
| Gaming | POST | `/api/v1/tenants/{tenantId}/gaming/lottery539/draws` | JWT + Policy(TenantUser) | `CreateDrawRequest` | `Guid` | 建立期數。 |
| Admin Gaming | POST | `/api/v1/tenants/{tenantId}/admin/gaming/draw-templates` | JWT + Policy(TenantUser) + Permission `GAMING:DRAW-TEMPLATE:MANAGE` | `CreateDrawTemplateRequest` | `Guid` | 建立期數模板。 |
| Admin Gaming | PUT | `/api/v1/tenants/{tenantId}/admin/gaming/draw-templates/{templateId}` | JWT + Policy(TenantUser) + Permission `GAMING:DRAW-TEMPLATE:MANAGE` | `UpdateDrawTemplateRequest` | N/A | 更新期數模板。 |
| Admin Gaming | POST | `/api/v1/tenants/{tenantId}/admin/gaming/draw-templates/{templateId}/activate` | JWT + Policy(TenantUser) + Permission `GAMING:DRAW-TEMPLATE:MANAGE` | N/A | N/A | 啟用期數模板。 |
| Admin Gaming | POST | `/api/v1/tenants/{tenantId}/admin/gaming/draw-templates/{templateId}/deactivate` | JWT + Policy(TenantUser) + Permission `GAMING:DRAW-TEMPLATE:MANAGE` | N/A | N/A | 停用期數模板。 |
| Admin Gaming | GET | `/api/v1/tenants/{tenantId}/admin/gaming/draw-templates` | JWT + Policy(TenantUser) + Permission `GAMING:DRAW-TEMPLATE:MANAGE` | `GetDrawTemplatesRequest`(query) | `DrawTemplateSummaryDto[]` | 期數模板列表。 |
| Admin Gaming | GET | `/api/v1/tenants/{tenantId}/admin/gaming/draw-templates/{templateId}` | JWT + Policy(TenantUser) + Permission `GAMING:DRAW-TEMPLATE:MANAGE` | N/A | `DrawTemplateDetailDto` | 期數模板詳情。 |
| Gaming | GET | `/api/v1/tenants/{tenantId}/gaming/lottery539/draws` | None | `GetDrawsRequest`(query) | `DrawSummaryDto[]` | 取得期數列表（允許匿名）。 |
| Gaming | GET | `/api/v1/tenants/{tenantId}/gaming/draws/selling/options` | None | `GetSellingDrawOptionsRequest`(query) | `DrawSellingOptionDto[]` | 可售票期數下拉選項。 |
| Gaming | GET | `/api/v1/tenants/{tenantId}/gaming/lottery539/draws/{drawId}` | None | N/A | `DrawDetailDto` | 取得期數詳情（允許匿名）。 |
| Gaming | POST | `/api/v1/tenants/{tenantId}/gaming/lottery539/draws/{drawId}/tickets` | JWT + Policy(Member) | `PlaceTicketRequest` | `Guid` | 會員下注。 |
| Gaming | POST | `/api/v1/tenants/{tenantId}/gaming/lottery539/draws/{drawId}/execute` | JWT + Policy(TenantUser) | N/A | N/A | 開獎執行。 |
| Gaming | POST | `/api/v1/tenants/{tenantId}/gaming/lottery539/draws/{drawId}/settle` | JWT + Policy(TenantUser) | N/A | N/A | 結算。 |
| Gaming | POST | `/api/v1/tenants/{tenantId}/gaming/lottery539/draws/{drawId}/manual-close` | JWT + Policy(TenantUser) | `CloseDrawManuallyRequest` | N/A | 手動封盤。 |
| Gaming | POST | `/api/v1/tenants/{tenantId}/gaming/lottery539/draws/{drawId}/reopen` | JWT + Policy(TenantUser) | N/A | N/A | 重新開盤。 |
| Gaming | GET | `/api/v1/tenants/{tenantId}/gaming/lottery539/draws/{drawId}/allowed-ticket-templates` | JWT + Policy(TenantUser) | N/A | `DrawAllowedTicketTemplateDto[]` | 允許票種。 |
| Gaming | PUT | `/api/v1/tenants/{tenantId}/gaming/lottery539/draws/{drawId}/allowed-ticket-templates` | JWT + Policy(TenantUser) | `UpdateDrawAllowedTicketTemplatesRequest` | N/A | 更新允許票種。 |
| Gaming | GET | `/api/v1/tenants/{tenantId}/gaming/lottery539/draws/{drawId}/prize-mappings` | JWT + Policy(TenantUser) | N/A | `DrawPrizeMappingDto[]` | 查詢獎項對應。 |
| Gaming | PUT | `/api/v1/tenants/{tenantId}/gaming/lottery539/draws/{drawId}/prize-mappings` | JWT + Policy(TenantUser) | `UpdateDrawPrizeMappingsRequest` | N/A | 更新獎項對應。 |
| Gaming | GET | `/api/v1/tenants/{tenantId}/gaming/lottery539/members/me/tickets` | JWT + Policy(Member) | `GetMyTicketsRequest`(query) | `TicketSummaryDto[]` | 會員票券查詢。 |
| Gaming | GET | `/api/v1/tenants/{tenantId}/gaming/members/me/tickets/available-for-bet` | JWT + Policy(Member) | `GetAvailableTicketsForBetRequest`(query) | `AvailableTicketsResponse` | 取得可下注票券清單（Dropdown 用）。 |
| Gaming | GET | `/api/v1/tenants/{tenantId}/gaming/lottery539/members/me/awards` | JWT + Policy(Member) | `GetMyAwardsRequest`(query) | `PrizeAwardDto[]` | 會員得獎查詢。 |
| Gaming | POST | `/api/v1/tenants/{tenantId}/gaming/prizes/awards/{awardId}/redeem` | JWT + Policy(Member) | `RedeemPrizeAwardRequest` | `Guid` | 兌獎。 |
| Gaming | GET | `/api/v1/tenants/{tenantId}/gaming/prizes` | JWT + Policy(TenantUser) | N/A | `PrizeDto[]` | 獎品列表。 |
| Gaming | POST | `/api/v1/tenants/{tenantId}/gaming/prizes` | JWT + Policy(TenantUser) | `CreatePrizeRequest` | `Guid` | 建立獎品。 |
| Gaming | PUT | `/api/v1/tenants/{tenantId}/gaming/prizes/{prizeId}` | JWT + Policy(TenantUser) | `UpdatePrizeRequest` | N/A | 更新獎品。 |
| Gaming | PATCH | `/api/v1/tenants/{tenantId}/gaming/prizes/{prizeId}/activate` | JWT + Policy(TenantUser) | N/A | N/A | 啟用獎品。 |
| Gaming | PATCH | `/api/v1/tenants/{tenantId}/gaming/prizes/{prizeId}/deactivate` | JWT + Policy(TenantUser) | N/A | N/A | 停用獎品。 |
| Gaming | GET | `/api/v1/tenants/{tenantId}/gaming/ticket-templates` | JWT + Policy(TenantUser) | Query: `activeOnly` | `TicketTemplateDto[]` | 票種模板列表。 |
| Gaming | POST | `/api/v1/tenants/{tenantId}/gaming/ticket-templates` | JWT + Policy(TenantUser) | `CreateTicketTemplateRequest` | `Guid` | 建立票種模板。 |
| Gaming | PUT | `/api/v1/tenants/{tenantId}/gaming/ticket-templates/{templateId}` | JWT + Policy(TenantUser) | `UpdateTicketTemplateRequest` | N/A | 更新票種模板。 |
| Gaming | PATCH | `/api/v1/tenants/{tenantId}/gaming/ticket-templates/{templateId}/activate` | JWT + Policy(TenantUser) | N/A | N/A | 啟用票種模板。 |
| Gaming | PATCH | `/api/v1/tenants/{tenantId}/gaming/ticket-templates/{templateId}/deactivate` | JWT + Policy(TenantUser) | N/A | N/A | 停用票種模板。 |
| Gaming | GET | `/api/v1/tenants/{tenantId}/gaming/lottery539/prize-rules` | JWT + Policy(TenantUser) | N/A | `PrizeRuleDto[]` | 中獎規則列表。 |
| Gaming | POST | `/api/v1/tenants/{tenantId}/gaming/lottery539/prize-rules` | JWT + Policy(TenantUser) | `CreatePrizeRuleRequest` | `Guid` | 建立中獎規則。 |
| Gaming | PUT | `/api/v1/tenants/{tenantId}/gaming/lottery539/prize-rules/{ruleId}` | JWT + Policy(TenantUser) | `UpdatePrizeRuleRequest` | N/A | 更新中獎規則。 |
| Gaming | PATCH | `/api/v1/tenants/{tenantId}/gaming/lottery539/prize-rules/{ruleId}/activate` | JWT + Policy(TenantUser) | N/A | N/A | 啟用中獎規則。 |
| Gaming | PATCH | `/api/v1/tenants/{tenantId}/gaming/lottery539/prize-rules/{ruleId}/deactivate` | JWT + Policy(TenantUser) | N/A | N/A | 停用中獎規則。 |
| Account | GET | `/api/v1/account/me` | JWT | N/A | `string` | 回傳 `Hi, userId = ...`。 |
| Health | GET | `/health` | None | N/A | N/A | 健康檢查端點。 |

---

## 3. 各模組詳述（逐一展開）

### Auth

#### [POST] /api/v1/auth/login - 租戶管理者登入
**Auth:** None (AllowAnonymous)【F:src/Web.Api/Endpoints/Auth/Login.cs†L11-L29】

**Request**
- Content-Type: `application/json`
- Body schema
  | name | type | required | constraints | description |
  |---|---|---|---|---|
  | email | string | ✅ | 必須包含 `@` | 登入帳號 Email（由 `Email.Create` 驗證）。【F:src/Domain/Users/Email.cs†L9-L26】 |
  | password | string | ✅ | 無明確 validator | 密碼。 |
  | tenantCode | string | ✅ | 3 碼英數，會 Trim + Uppercase | 租戶代碼。【F:src/Application/Auth/LoginRequest.cs†L1-L9】【F:src/Application/Auth/LoginCommandHandler.cs†L20-L41】 |
- Example request
```bash
curl -X POST \
  "$BASE_URL/api/v1/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@tenant.com","password":"123456","tenantCode":"ABC"}'
```

**Response**
- 200 schema: `LoginResponse`
  | name | type | required | description |
  |---|---|---|---|
  | token | string | ✅ | JWT token。【F:src/Application/Auth/LoginResponse.cs†L1-L9】 |
  | expiration | datetime | ❓ | handler 未賦值，可能為預設值。請確認。 |
- Example response
```json
{ "token": "<jwt>", "expiration": "0001-01-01T00:00:00" }
```

**Domain notes**
- tenantCode 必須存在且格式正確；找不到租戶會回 404 NotFound error code `Auth.TenantNotFound`。【F:src/Application/Auth/LoginCommandHandler.cs†L20-L45】【F:src/Application/Auth/AuthErrors.cs†L13-L21】

**Frontend tips**
- tenantCode 請以大寫 3 碼輸入，可在 UI 直接格式化。
- 需處理 `Users.InvalidCredentials` / `Users.NotFoundByEmail` 等錯誤顯示。【F:src/Domain/Users/UserErrors.cs†L9-L20】

---

#### [POST] /api/v1/{tenantId}/auth/line-login - 會員 LINE 登入
**Auth:** None (AllowAnonymous)【F:src/Web.Api/Endpoints/Auth/LineLogin.cs†L10-L33】

**Request**
- Path params
  | name | type | required | description |
  |---|---|---|---|
  | tenantId | guid | ✅ | 租戶識別碼。 |
- Body schema
  | name | type | required | constraints | description |
  |---|---|---|---|---|
  | accessToken | string | ✅ | 不可空白 | LINE access token。【F:src/Application/Auth/LineLoginRequest.cs†L1-L7】【F:src/Application/Auth/LineLoginCommandHandler.cs†L25-L41】 |
- Example request
```bash
curl -X POST \
  "$BASE_URL/api/v1/{tenantId}/auth/line-login" \
  -H "Content-Type: application/json" \
  -d '{"accessToken":"<line-access-token>"}'
```

**Response**
- 200 schema: `LineLoginResponse`
  | name | type | required | description |
  |---|---|---|---|
  | token | string | ✅ | JWT token。【F:src/Application/Auth/LineLoginResponse.cs†L1-L15】 |
  | userId | guid | ✅ | 使用者 Id。 |
  | tenantId | guid | ✅ | 租戶 Id。 |
  | memberId | guid? | ❓ | 會員 Id，可能為 null。 |
  | memberNo | string? | ❓ | 會員編號。 |
- Example response
```json
{
  "token": "<jwt>",
  "userId": "11111111-1111-1111-1111-111111111111",
  "tenantId": "22222222-2222-2222-2222-222222222222",
  "memberId": "33333333-3333-3333-3333-333333333333",
  "memberNo": "MBR-20240101010101-ABC123"
}
```

**Domain notes**
- 若首次登入會建立 User/Member 資料（可產生併發唯一性錯誤，但已容錯處理）。【F:src/Application/Auth/LineLoginCommandHandler.cs†L55-L117】

**Frontend tips**
- tenantId 必須與路由一致；若需支援 tenantCode 導頁，請先呼叫 `/tenants/by-code` 取得 Id。

---

#### [POST] /api/v1/tenants/{tenantId}/auth/line/liff-login - LIFF 登入即註冊會員
**Auth:** None (AllowAnonymous)【F:src/Web.Api/Endpoints/Auth/LineLiffLogin.cs†L1-L57】

**Request**
- Path params
  | name | type | required | description |
  |---|---|---|---|
  | tenantId | guid | ✅ | 租戶識別碼。 |
- Body schema
  | name | type | required | constraints | description |
  |---|---|---|---|---|
  | accessToken | string | ✅ | 不可空白 | LINE access token。【F:src/Application/Auth/LineLiffLoginRequest.cs†L1-L8】【F:src/Application/Auth/LineLiffLoginCommandHandler.cs†L24-L51】 |
  | displayName | string | ❓ | 允許空白 | 顯示名稱（未填則使用預設值）。【F:src/Application/Auth/LineLiffLoginRequest.cs†L1-L8】【F:src/Application/Auth/LineLiffLoginCommandHandler.cs†L62-L66】 |
  | deviceId | string | ❓ | 無 | 裝置識別碼。【F:src/Application/Auth/LineLiffLoginRequest.cs†L1-L8】 |
- Example request
```bash
curl -X POST \
  "$BASE_URL/api/v1/tenants/{tenantId}/auth/line/liff-login" \
  -H "Content-Type: application/json" \
  -d '{"accessToken":"<line-access-token>","displayName":"LINE會員"}'
```

**Response**
- 200 schema: `LineLoginResponse`
  | name | type | required | description |
  |---|---|---|---|
  | accessToken | string | ✅ | JWT token。【F:src/Application/Auth/LineLoginResponse.cs†L5-L19】 |
  | userId | guid | ✅ | 使用者 Id。 |
  | tenantId | guid | ✅ | 租戶 Id。 |
  | memberId | guid? | ❓ | 會員 Id，可能為 null。 |
  | memberNo | string? | ❓ | 會員編號。 |
- Example response
```json
{
  "accessToken": "<jwt>",
  "userId": "11111111-1111-1111-1111-111111111111",
  "tenantId": "22222222-2222-2222-2222-222222222222",
  "memberId": "33333333-3333-3333-3333-333333333333",
  "memberNo": "MBR-20240101010101-ABC123"
}
```

**Domain notes**
- access token 會經由 LINE verify 流程驗證，首次登入會自動建立會員並留下 `member.auto_register.liff` 活動紀錄。【F:src/Application/Auth/LineLiffLoginCommandHandler.cs†L33-L120】


### Tenants

#### [GET] /api/v1/tenants/by-code/{tenantCode} - 以代碼換取租戶資訊
**Auth:** None (AllowAnonymous)【F:src/Web.Api/Endpoints/Tenants/TenantsEndpoints.cs†L28-L47】

**Request**
- Path params
  | name | type | required | constraints | description |
  |---|---|---|---|---|
  | tenantCode | string | ✅ | 3 碼英數 | 租戶代碼。 |
- Example request
```bash
curl "$BASE_URL/api/v1/tenants/by-code/ABC"
```

**Response**
- 200 schema: `TenantLookupResponse`
  | name | type | required | description |
  |---|---|---|---|
  | tenantId | guid | ✅ | 租戶 Id。 |
  | tenantCode | string | ✅ | 租戶代碼（大寫）。 |
  | name | string | ✅ | 租戶名稱。 |
- Example response
```json
{ "tenantId": "22222222-2222-2222-2222-222222222222", "tenantCode": "ABC", "name": "Default Tenant" }
```

**Domain notes**
- tenantCode 格式錯誤 → 400；找不到 → 404（ProblemDetails）。【F:src/Web.Api/Endpoints/Tenants/TenantsEndpoints.cs†L32-L46】

---

#### [PUT] /api/v1/tenants/{tenantId}/settings/timezone - 更新租戶時區
**Auth:** JWT + Policy `TenantUser`【F:src/Web.Api/Endpoints/Tenants/TenantsEndpoints.cs†L20-L64】

**Request**
- Path params
  | name | type | required | description |
  |---|---|---|---|
  | tenantId | guid | ✅ | 租戶 Id。 |
- Body schema
  | name | type | required | constraints | description |
  |---|---|---|---|---|
  | timeZoneId | string | ✅ | 需為有效 IANA/Windows 時區 | 例如 `Asia/Taipei`、`UTC`。【F:src/Web.Api/Endpoints/Tenants/Requests/UpdateTenantTimeZoneRequest.cs†L1-L4】 |
- Example request
```bash
curl -X PUT \
  "$BASE_URL/api/v1/tenants/{tenantId}/settings/timezone" \
  -H "Authorization: Bearer <jwt>" \
  -H "Content-Type: application/json" \
  -d '{"timeZoneId":"Asia/Taipei"}'
```

**Response**
- 200 OK 無 body。
- 400 若 TimeZoneId 空白或無效（`TimeZone.Required/TimeZone.Invalid`）。【F:src/Application/Tenants/UpdateTimeZone/UpdateTenantTimeZoneCommandHandler.cs†L20-L46】【F:src/Application/Time/TimeZoneErrors.cs†L5-L13】
- Example response
```
HTTP/1.1 200 OK
```

**Frontend tips**
- 建議提供可選的 IANA 時區清單，避免輸入錯誤。

---

### Users (v2)

#### [GET] /api/v2/users/{id} - 取得使用者資料
**Auth:** JWT + Policy `TenantUser`【F:src/Web.Api/Endpoints/Users/UsersEndpoints.cs†L16-L36】

**Request**
- Path params: `id` (guid)
- Example
```bash
curl "$BASE_URL/api/v2/users/{id}" -H "Authorization: Bearer <jwt>"
```

**Response**
- 200 schema: `UserResponse`
  | name | type | required | description |
  |---|---|---|---|
  | id | guid | ✅ | 使用者 Id。 |
  | email | string | ✅ | Email。 |
  | name | string | ✅ | 名稱。 |
  | hasPublicProfile | bool | ✅ | 是否公開檔案。 |
- 404 NotFound 若找不到使用者。 【F:src/Application/Users/GetById/UserResponse.cs†L1-L13】【F:src/Domain/Users/UserErrors.cs†L5-L11】
- Example response
```json
{
  "id": "11111111-1111-1111-1111-111111111111",
  "email": "user@tenant.com",
  "name": "User",
  "hasPublicProfile": true
}
```

---

#### [POST] /api/v2/users - 建立使用者
**Auth:** JWT + Policy `TenantUser`【F:src/Web.Api/Endpoints/Users/UsersEndpoints.cs†L38-L58】

**Request**
- Body schema `CreateUserRequest`
  | name | type | required | constraints | description |
  |---|---|---|---|---|
  | email | string | ✅ | 必填 + Email 格式 | Email。【F:src/Application/Users/Create/CreateUserRequest.cs†L1-L8】【F:src/Application/Users/Create/CreateUserCommandValidator.cs†L5-L16】 |
  | name | string | ✅ | 必填 | 顯示名稱。 |
  | password | string | ✅ | 未定義 | 密碼。 |
  | hasPublicProfile | bool | ✅ | - | 公開檔案。 |
  | userType | string? | ❓ | `Platform/Tenant/Member` | 使用者類型。未填時預設 Tenant。【F:src/Application/Users/Create/CreateUserRequest.cs†L1-L8】【F:src/Application/Users/Create/CreateUserCommandHandler.cs†L36-L44】 |
  | tenantId | guid? | ❓ | Platform 使用者不得帶 | 指定 tenantId。 |
- Example
```bash
curl -X POST "$BASE_URL/api/v2/users" \
  -H "Authorization: Bearer <jwt>" \
  -H "Content-Type: application/json" \
  -d '{"email":"user@tenant.com","name":"User","password":"123456","hasPublicProfile":true,"userType":"Tenant"}'
```

**Response**
- 200: `Guid`（新使用者 Id）。
- 400: Email 不合法 / 非唯一 / userType 不合法 / tenantId 規則不符。 【F:src/Application/Users/Create/CreateUserCommandHandler.cs†L17-L72】【F:src/Domain/Users/UserErrors.cs†L13-L38】
- Example response
```json
"11111111-1111-1111-1111-111111111111"
```

---

#### [POST] /api/v2/users/tenant - 建立租戶使用者
**Auth:** JWT + Policy `TenantUser` + Permission `USERS:CREATE`【F:src/Web.Api/Endpoints/Users/UsersEndpoints.cs†L60-L75】【F:src/Domain/Security/Permission.cs†L28-L37】

**Request**
- Body schema `CreateTenantUserRequest`
  | name | type | required | constraints | description |
  |---|---|---|---|---|
  | email | string | ✅ | Email 格式 | Email。 |
  | name | string | ✅ | 必填 | 名稱。 |
  | password | string | ✅ | - | 密碼。 |
  | hasPublicProfile | bool | ✅ | - | 公開檔案。 |

- Example request
```bash
curl -X POST "$BASE_URL/api/v2/users/tenant" \
  -H "Authorization: Bearer <jwt>" \
  -H "Content-Type: application/json" \
  -d '{"email":"tenant@tenant.com","name":"TenantUser","password":"123456","hasPublicProfile":false}'
```

**Response**
- 200: `Guid`（新使用者 Id）。
- Example response
```json
"11111111-1111-1111-1111-111111111111"
```

---

#### [POST] /api/v2/users/{userId}/roles/{roleId} - 指派角色
**Auth:** JWT + Policy `TenantUser` + Permission `USERS:UPDATE`【F:src/Web.Api/Endpoints/Users/UsersEndpoints.cs†L77-L92】【F:src/Domain/Security/Permission.cs†L30-L35】

**Request**
- Path params: `userId` (guid), `roleId` (int > 0)
- Example
```bash
curl -X POST "$BASE_URL/api/v2/users/{userId}/roles/{roleId}" \
  -H "Authorization: Bearer <jwt>"
```

**Response**
- 200 schema: `AssignRoleToUserResultDto`
  | name | type | required | description |
  |---|---|---|---|
  | userId | guid | ✅ | 使用者 Id。 |
  | roleIds | int[] | ✅ | 已擁有角色列表。 |
- 400/404/409：使用者不存在、角色已存在、權限規則不符。 【F:src/Application/Users/AssignRole/AssignRoleToUserResultDto.cs†L1-L3】【F:src/Domain/Users/UserErrors.cs†L17-L31】
- Example response
```json
{ "userId": "11111111-1111-1111-1111-111111111111", "roleIds": [1, 2] }
```

---

### Roles

#### [POST] /api/v1/roles - 建立角色
**Auth:** JWT + Policy `TenantUser` + Permission `ROLES:CREATE`【F:src/Web.Api/Endpoints/Roles/RolesEndpoints.cs†L17-L40】【F:src/Domain/Security/Permission.cs†L115-L121】

**Request**
- Body schema `CreateRoleRequest`
  | name | type | required | constraints | description |
  |---|---|---|---|---|
  | name | string | ✅ | 不可空白 | 角色名稱。【F:src/Application/Roles/Create/CreateRoleCommandValidator.cs†L5-L13】 |

- Example request
```bash
curl -X POST "$BASE_URL/api/v1/roles" \
  -H "Authorization: Bearer <jwt>" \
  -H "Content-Type: application/json" \
  -d '{"name":"客服"}'
```

**Response**
- 200: `int`（角色 Id）
- Example response
```json
1
```

---

#### [PUT] /api/v1/roles/{id} - 更新角色
**Auth:** JWT + Policy `TenantUser` + Permission `ROLES:UPDATE`【F:src/Web.Api/Endpoints/Roles/RolesEndpoints.cs†L42-L60】

**Request**
- Path params: `id` (int > 0)
- Body schema `UpdateRoleRequest` (`name` 不可空白)。【F:src/Application/Roles/Update/UpdateRoleCommandValidator.cs†L5-L16】

- Example request
```bash
curl -X PUT "$BASE_URL/api/v1/roles/1" \
  -H "Authorization: Bearer <jwt>" \
  -H "Content-Type: application/json" \
  -d '{"name":"客服主管"}'
```

**Response**
- 200 OK
- Example response
```
HTTP/1.1 200 OK
```

---

#### [DELETE] /api/v1/roles/{id} - 刪除角色
**Auth:** JWT + Policy `TenantUser` + Permission `ROLES:DELETE`【F:src/Web.Api/Endpoints/Roles/RolesEndpoints.cs†L62-L75】

**Example request**
```bash
curl -X DELETE "$BASE_URL/api/v1/roles/1" -H "Authorization: Bearer <jwt>"
```

**Response**
- 200 OK
- 404 NotFound（角色不存在）。
- Example response
```
HTTP/1.1 200 OK
```

---

#### [GET] /api/v1/roles/{id} - 角色詳情
**Auth:** JWT + Policy `TenantUser` + Permission `ROLES:VIEW`【F:src/Web.Api/Endpoints/Roles/RolesEndpoints.cs†L77-L95】

**Example request**
```bash
curl "$BASE_URL/api/v1/roles/1" -H "Authorization: Bearer <jwt>"
```

**Response**
- 200 schema: `RoleDetailDto`（含 permission codes）。【F:src/Application/Roles/Dtos/RoleDetailDto.cs†L1-L6】
- Example response
```json
{
  "id": 1,
  "name": "客服",
  "permissionCodes": ["USERS:VIEW", "MEMBERS:READ"]
}
```

---

#### [GET] /api/v1/roles - 角色列表
**Auth:** JWT + Policy `TenantUser` + Permission `ROLES:VIEW`【F:src/Web.Api/Endpoints/Roles/RolesEndpoints.cs†L97-L112】

**Example request**
```bash
curl "$BASE_URL/api/v1/roles" -H "Authorization: Bearer <jwt>"
```

**Response**
- 200 schema: `RoleListItemDto[]`。【F:src/Application/Roles/Dtos/RoleListItemDto.cs†L1-L6】
- Example response
```json
[
  { "id": 1, "name": "客服", "permissionCount": 2 }
]
```

---

#### [GET] /api/v1/roles/{id}/permissions - 角色權限清單
**Auth:** JWT + Policy `TenantUser` + Permission `ROLES:VIEW`【F:src/Web.Api/Endpoints/Roles/RolesEndpoints.cs†L114-L126】

**Example request**
```bash
curl "$BASE_URL/api/v1/roles/1/permissions" -H "Authorization: Bearer <jwt>"
```

**Response**
- 200: `string[]`（permission codes）
- Example response
```json
["USERS:VIEW", "MEMBERS:READ"]
```

---

#### [POST] /api/v1/roles/{id}/permissions - 新增角色權限
**Auth:** JWT + Policy `TenantUser` + Permission `ROLES:UPDATE`【F:src/Web.Api/Endpoints/Roles/RolesEndpoints.cs†L128-L141】

**Request**
- Body schema `UpdateRolePermissionsRequest`
  | name | type | required | constraints |
  |---|---|---|---|
  | permissionCodes | string[] | ✅ | 至少一個非空白代碼。【F:src/Application/Roles/Permissions/AddRolePermissionsCommandValidator.cs†L5-L25】 |

- Example request
```bash
curl -X POST "$BASE_URL/api/v1/roles/1/permissions" \
  -H "Authorization: Bearer <jwt>" \
  -H "Content-Type: application/json" \
  -d '{"permissionCodes":["USERS:VIEW","MEMBERS:READ"]}'
```

**Response**
- 200 OK
- Example response
```
HTTP/1.1 200 OK
```

---

#### [POST] /api/v1/roles/{id}/permissions/remove - 移除角色權限
**Auth:** JWT + Policy `TenantUser` + Permission `ROLES:UPDATE`【F:src/Web.Api/Endpoints/Roles/RolesEndpoints.cs†L143-L153】

**Request**
- Body schema `UpdateRolePermissionsRequest`（同上）。【F:src/Application/Roles/Permissions/RemoveRolePermissionsCommandValidator.cs†L5-L25】

- Example request
```bash
curl -X POST "$BASE_URL/api/v1/roles/1/permissions/remove" \
  -H "Authorization: Bearer <jwt>" \
  -H "Content-Type: application/json" \
  -d '{"permissionCodes":["USERS:VIEW"]}'
```

**Response**
- 200 OK
- Example response
```
HTTP/1.1 200 OK
```

---

### Permissions

#### [GET] /api/v1/permissions/catalog - 取得權限目錄
**Auth:** JWT + Policy `TenantUser`【F:src/Web.Api/Endpoints/Permissions/PermissionsEndpoints.cs†L9-L27】

**Example request**
```bash
curl "$BASE_URL/api/v1/permissions/catalog" -H "Authorization: Bearer <jwt>"
```

**Response**
- 200 schema: `PermissionCatalogDto`，包含 scope/module/permissions。 【F:src/Application/Authorization/PermissionCatalogDto.cs†L1-L22】
- Example response
```json
{
  "version": "1.0",
  "scopes": [
    {
      "scope": 1,
      "displayName": "租戶",
      "modules": [
        {
          "moduleKey": "USERS",
          "displayName": "使用者管理",
          "masterPermissionCode": "USERS:*",
          "items": [
            {
              "code": "USERS:VIEW",
              "displayName": "檢視使用者",
              "description": "檢視使用者資料",
              "sortOrder": 10,
              "isDangerous": false,
              "hidden": false
            }
          ]
        }
      ]
    }
  ]
}
```

**Frontend tips**
- 以 `code` 作為權限識別，前端顯示可用 `permission.<code>` 作 i18n key。

---

### Members

#### [POST] /api/v1/members - 建立會員
**Auth:** JWT + Policy `TenantUser` + Permission `MEMBERS:CREATE`【F:src/Web.Api/Endpoints/Members/MembersEndpoints.cs†L31-L53】【F:src/Domain/Security/Permission.cs†L45-L53】

**Request**
- Body schema `CreateMemberRequest`
  | name | type | required | description |
  |---|---|---|---|
  | userId | guid? | ❓ | 綁定使用者 Id（可為 null）。 |
  | displayName | string | ✅ | 會員顯示名稱。 |
  | memberNo | string? | ❓ | 會員編號，可由後端產生。 |

- Example request
```bash
curl -X POST "$BASE_URL/api/v1/members" \
  -H "Authorization: Bearer <jwt>" \
  -H "Content-Type: application/json" \
  -d '{"userId":null,"displayName":"王小明","memberNo":null}'
```

**Response**
- 200: `Guid`（會員 Id）。
- Example response
```json
"33333333-3333-3333-3333-333333333333"
```

---

#### [GET] /api/v1/members/{id} - 會員詳情
**Auth:** JWT + Policy `TenantUser` + Permission `MEMBERS:READ`【F:src/Web.Api/Endpoints/Members/MembersEndpoints.cs†L55-L71】

**Example request**
```bash
curl "$BASE_URL/api/v1/members/33333333-3333-3333-3333-333333333333" \
  -H "Authorization: Bearer <jwt>"
```

**Response**
- 200 schema: `MemberDetailDto`。【F:src/Application/Members/Dtos/MemberDetailDto.cs†L1-L9】
- Example response
```json
{
  "id": "33333333-3333-3333-3333-333333333333",
  "userId": null,
  "memberNo": "MBR-20240101010101-ABC123",
  "displayName": "王小明",
  "status": 0,
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": "2024-01-01T00:00:00Z"
}
```

---

#### [GET] /api/v1/members - 會員查詢
**Auth:** JWT + Policy `TenantUser` + Permission `MEMBERS:READ`【F:src/Web.Api/Endpoints/Members/MembersEndpoints.cs†L73-L93】

**Request (Query)**
- `memberNo` (string?)
- `displayName` (string?)
- `status` (short?) → 對應 `MemberStatus` enum
- `userId` (guid?)
- `page` (int, default 1)
- `pageSize` (int, default 20)

- Example request
```bash
curl "$BASE_URL/api/v1/members?displayName=%E7%8E%8B&page=1&pageSize=20" \
  -H "Authorization: Bearer <jwt>"
```

**Response**
- 200 schema: `PagedResult<MemberListItemDto>`。【F:src/Application/Abstractions/Data/PagedResult.cs†L1-L29】【F:src/Application/Members/Dtos/MemberListItemDto.cs†L1-L9】
- Example response
```json
{
  "items": [
    {
      "id": "33333333-3333-3333-3333-333333333333",
      "userId": null,
      "memberNo": "MBR-20240101010101-ABC123",
      "displayName": "王小明",
      "status": 0,
      "createdAt": "2024-01-01T00:00:00Z",
      "updatedAt": "2024-01-01T00:00:00Z"
    }
  ],
  "totalCount": 1,
  "page": 1,
  "pageSize": 20
}
```

---

#### [PUT] /api/v1/members/{id} - 更新會員資料
**Auth:** JWT + Policy `TenantUser` + Permission `MEMBERS:UPDATE`【F:src/Web.Api/Endpoints/Members/MembersEndpoints.cs†L95-L106】

**Request**
- Body: `UpdateMemberProfileRequest` (`displayName`)。

- Example request
```bash
curl -X PUT "$BASE_URL/api/v1/members/33333333-3333-3333-3333-333333333333" \
  -H "Authorization: Bearer <jwt>" \
  -H "Content-Type: application/json" \
  -d '{"displayName":"王小明"}'
```

**Response**
- 200 OK
- Example response
```
HTTP/1.1 200 OK
```

---

#### [POST] /api/v1/members/{id}/suspend - 停權會員
**Auth:** JWT + Policy `TenantUser` + Permission `MEMBERS:SUSPEND`【F:src/Web.Api/Endpoints/Members/MembersEndpoints.cs†L108-L119】

**Request**
- Body: `MemberStatusChangeRequest` (`reason` optional)。

- Example request
```bash
curl -X POST "$BASE_URL/api/v1/members/33333333-3333-3333-3333-333333333333/suspend" \
  -H "Authorization: Bearer <jwt>" \
  -H "Content-Type: application/json" \
  -d '{"reason":"違規操作"}'
```

**Response**
- 200 OK
- Example response
```
HTTP/1.1 200 OK
```

---

#### [POST] /api/v1/members/{id}/activate - 解除停權
**Auth:** JWT + Policy `TenantUser` + Permission `MEMBERS:SUSPEND`【F:src/Web.Api/Endpoints/Members/MembersEndpoints.cs†L121-L132】

**Request**
- Body: `MemberStatusChangeRequest`。

- Example request
```bash
curl -X POST "$BASE_URL/api/v1/members/33333333-3333-3333-3333-333333333333/activate" \
  -H "Authorization: Bearer <jwt>" \
  -H "Content-Type: application/json" \
  -d '{"reason":"解除停權"}'
```

**Response**
- 200 OK
- Example response
```
HTTP/1.1 200 OK
```

---

#### [GET] /api/v1/members/{id}/points/balance - 查詢點數餘額
**Auth:** JWT + Policy `TenantUser` + Permission `MEMBER_POINTS:READ`【F:src/Web.Api/Endpoints/Members/MembersEndpoints.cs†L134-L148】

**Example request**
```bash
curl "$BASE_URL/api/v1/members/33333333-3333-3333-3333-333333333333/points/balance" \
  -H "Authorization: Bearer <jwt>"
```

**Response**
- 200 schema: `MemberPointBalanceDto`。【F:src/Application/Members/Dtos/MemberPointBalanceDto.cs†L1-L6】
- Example response
```json
{ "memberId": "33333333-3333-3333-3333-333333333333", "balance": 1200, "updatedAt": "2024-01-01T00:00:00Z" }
```

---

#### [GET] /api/v1/members/{id}/points/history - 點數歷史
**Auth:** JWT + Policy `TenantUser` + Permission `MEMBER_POINTS:READ`【F:src/Web.Api/Endpoints/Members/MembersEndpoints.cs†L150-L173】

**Request (Query)**
- `startDate` / `endDate`: DateTime?
- `type`: short?（`MemberPointLedgerType`）
- `referenceType` / `referenceId`: string?
- `page` / `pageSize`

- Example request
```bash
curl "$BASE_URL/api/v1/members/33333333-3333-3333-3333-333333333333/points/history?page=1&pageSize=20" \
  -H "Authorization: Bearer <jwt>"
```

**Response**
- 200 schema: `PagedResult<MemberPointLedgerDto>`。【F:src/Application/Members/Dtos/MemberPointLedgerDto.cs†L1-L12】
- Example response
```json
{
  "items": [
    {
      "id": "44444444-4444-4444-4444-444444444444",
      "memberId": "33333333-3333-3333-3333-333333333333",
      "type": 2,
      "amount": 100,
      "beforeBalance": 1100,
      "afterBalance": 1200,
      "referenceType": "admin_adjust",
      "referenceId": null,
      "operatorUserId": null,
      "remark": "手動調整",
      "createdAt": "2024-01-01T00:00:00Z"
    }
  ],
  "totalCount": 1,
  "page": 1,
  "pageSize": 20
}
```

---

#### [POST] /api/v1/members/{id}/points/adjust - 調整點數
**Auth:** JWT + Policy `TenantUser` + Permission `MEMBER_POINTS:ADJUST`【F:src/Web.Api/Endpoints/Members/MembersEndpoints.cs†L175-L196】

**Request**
- Body `AdjustMemberPointsRequest`
  | name | type | required | description |
  |---|---|---|---|
  | delta | long | ✅ | 調整數量（可正可負）。 |
  | remark | string | ✅ | 備註。 |
  | referenceType | string | ✅ | 預設 `admin_adjust`。 |
  | referenceId | string? | ❓ | 參考 Id。 |
  | allowNegative | bool | ❓ | 是否允許負數餘額（預設 false）。 |

- Example request
```bash
curl -X POST "$BASE_URL/api/v1/members/33333333-3333-3333-3333-333333333333/points/adjust" \
  -H "Authorization: Bearer <jwt>" \
  -H "Content-Type: application/json" \
  -d '{"delta":100,"remark":"手動調整","referenceType":"admin_adjust","referenceId":null,"allowNegative":false}'
```

**Response**
- 200: `long`（調整後餘額）。
- Example response
```json
1200
```

---

#### [GET] /api/v1/members/{id}/assets - 會員資產
**Auth:** JWT + Policy `TenantUser` + Permission `MEMBER_ASSETS:READ`【F:src/Web.Api/Endpoints/Members/MembersEndpoints.cs†L198-L212】

**Example request**
```bash
curl "$BASE_URL/api/v1/members/33333333-3333-3333-3333-333333333333/assets" \
  -H "Authorization: Bearer <jwt>"
```

**Response**
- 200 schema: `MemberAssetBalanceDto[]`。【F:src/Application/Members/Dtos/MemberAssetBalanceDto.cs†L1-L7】
- Example response
```json
[
  {
    "memberId": "33333333-3333-3333-3333-333333333333",
    "assetCode": "COIN",
    "balance": 99.5,
    "updatedAt": "2024-01-01T00:00:00Z"
  }
]
```

---

#### [GET] /api/v1/members/{id}/assets/{assetCode}/history - 資產歷史
**Auth:** JWT + Policy `TenantUser` + Permission `MEMBER_ASSETS:READ`【F:src/Web.Api/Endpoints/Members/MembersEndpoints.cs†L214-L238】

**Request (Query)**
- `startDate` / `endDate` (DateTime?)
- `type` (short?) → `MemberAssetLedgerType`
- `referenceType` / `referenceId`
- `page` / `pageSize`

- Example request
```bash
curl "$BASE_URL/api/v1/members/33333333-3333-3333-3333-333333333333/assets/COIN/history?page=1&pageSize=20" \
  -H "Authorization: Bearer <jwt>"
```

**Response**
- 200 schema: `PagedResult<MemberAssetLedgerDto>`。【F:src/Application/Members/Dtos/MemberAssetLedgerDto.cs†L1-L14】
- Example response
```json
{
  "items": [
    {
      "id": "55555555-5555-5555-5555-555555555555",
      "memberId": "33333333-3333-3333-3333-333333333333",
      "assetCode": "COIN",
      "type": 0,
      "amount": 10.5,
      "beforeBalance": 89.0,
      "afterBalance": 99.5,
      "referenceType": "admin_adjust",
      "referenceId": null,
      "operatorUserId": null,
      "remark": "資產調整",
      "createdAt": "2024-01-01T00:00:00Z"
    }
  ],
  "totalCount": 1,
  "page": 1,
  "pageSize": 20
}
```

---

#### [POST] /api/v1/members/{id}/assets/adjust - 調整資產
**Auth:** JWT + Policy `TenantUser` + Permission `MEMBER_ASSETS:ADJUST`【F:src/Web.Api/Endpoints/Members/MembersEndpoints.cs†L240-L262】

**Request**
- Body `AdjustMemberAssetRequest`
  | name | type | required | description |
  |---|---|---|---|
  | assetCode | string | ✅ | 資產代碼。 |
  | delta | decimal | ✅ | 調整金額。 |
  | remark | string | ✅ | 備註。 |
  | referenceType | string | ✅ | 參考類型。 |
  | referenceId | string? | ❓ | 參考 Id。 |
  | allowNegative | bool | ❓ | 是否允許負數。 |

- Example request
```bash
curl -X POST "$BASE_URL/api/v1/members/33333333-3333-3333-3333-333333333333/assets/adjust" \
  -H "Authorization: Bearer <jwt>" \
  -H "Content-Type: application/json" \
  -d '{"assetCode":"COIN","delta":10.5,"remark":"資產調整","referenceType":"admin_adjust","referenceId":null,"allowNegative":false}'
```

**Response**
- 200: `decimal`（調整後餘額）。
- Example response
```json
99.5
```

---

#### [GET] /api/v1/members/{id}/activity - 會員操作歷程
**Auth:** JWT + Policy `TenantUser` + Permission `MEMBER_AUDIT:READ`【F:src/Web.Api/Endpoints/Members/MembersEndpoints.cs†L264-L283】

**Request (Query)**
- `startDate` / `endDate` (DateTime?)
- `action` (string?)
- `page` / `pageSize`

- Example request
```bash
curl "$BASE_URL/api/v1/members/33333333-3333-3333-3333-333333333333/activity?page=1&pageSize=20" \
  -H "Authorization: Bearer <jwt>"
```

**Response**
- 200 schema: `PagedResult<MemberActivityLogDto>`。【F:src/Application/Members/Dtos/MemberActivityLogDto.cs†L1-L11】
- Example response
```json
{
  "items": [
    {
      "id": "66666666-6666-6666-6666-666666666666",
      "memberId": "33333333-3333-3333-3333-333333333333",
      "action": "LOGIN",
      "ip": "127.0.0.1",
      "userAgent": "Mozilla/5.0",
      "operatorUserId": null,
      "payload": null,
      "createdAt": "2024-01-01T00:00:00Z"
    }
  ],
  "totalCount": 1,
  "page": 1,
  "pageSize": 20
}
```

---

### Reports

#### [GET] /api/v1/tenants/{tenantId}/reports/daily - 每日報表
**Auth:** JWT + Policy `TenantUser`【F:src/Web.Api/Endpoints/Reports/DailyReportsEndpoints.cs†L10-L30】

**Request**
- Path: `tenantId` (guid)
- Query: `date` (DateOnly, 格式 `YYYY-MM-DD`)

- Example request
```bash
curl "$BASE_URL/api/v1/tenants/22222222-2222-2222-2222-222222222222/reports/daily?date=2024-01-01" \
  -H "Authorization: Bearer <jwt>"
```

**Response**
- 200 schema: `DailyReportResponse`
  | name | type | required | description |
  |---|---|---|---|
  | tenantId | guid | ✅ | 租戶 Id。 |
  | localDate | date | ✅ | 查詢日期（租戶時區）。 |
  | timeZoneId | string | ✅ | 租戶時區。 |
  | startUtc | datetimeoffset | ✅ | UTC 起始時間。 |
  | endUtc | datetimeoffset | ✅ | UTC 結束時間。 |
  | newMembers | int | ✅ | 新增會員數。 |
  | ticketsCreated | int | ✅ | 新增票券數。 |

- Example response
```json
{
  "tenantId": "22222222-2222-2222-2222-222222222222",
  "localDate": "2024-01-01",
  "timeZoneId": "UTC",
  "startUtc": "2024-01-01T00:00:00+00:00",
  "endUtc": "2024-01-02T00:00:00+00:00",
  "newMembers": 10,
  "ticketsCreated": 15
}
```

---

### Gaming

> 路由皆位於 `/api/v1/tenants/{tenantId}/gaming/...`，除特別標註外需 JWT + Policy `TenantUser`。【F:src/Web.Api/Endpoints/Gaming/GamingEndpoints.cs†L31-L50】

#### [POST] /api/v1/tenants/{tenantId}/gaming/lottery539/draws - 建立期數
**Auth:** JWT + Policy `TenantUser`

**Request**
- Body `CreateDrawRequest`
  | name | type | required | description |
  |---|---|---|---|
  | templateId | guid | ✅ | 期數模板識別。 |
  | salesStartAt | datetime | ✅ | 開賣時間。 |
  | salesCloseAt | datetime | ✅ | 截止時間。 |
  | drawAt | datetime | ✅ | 開獎時間。 |
  | redeemValidDays | int? | ❓ | 兌獎天數。 |

- Example request
```bash
curl -X POST "$BASE_URL/api/v1/tenants/22222222-2222-2222-2222-222222222222/gaming/lottery539/draws" \
  -H "Authorization: Bearer <jwt>" \
  -H "Content-Type: application/json" \
  -d '{"templateId":"99999999-9999-9999-9999-999999999999","salesStartAt":"2024-01-01T00:00:00Z","salesCloseAt":"2024-01-01T12:00:00Z","drawAt":"2024-01-01T13:00:00Z","redeemValidDays":30}'
```

**Response**
- 200: `Guid`（drawId）。
- Example response
```json
"77777777-7777-7777-7777-777777777777"
```

**Notes**
- `draw_code` 由後端自動產生，格式為 `yyyyMMdd-000000`，以 tenant + game 為序號遞增基準。
- 若既有資料存在 `draw_code = ''`，請先執行一次性修正：
```sql
UPDATE gaming.draws
SET draw_code = NULL
WHERE draw_code = '';
```

---

### Admin Gaming - Draw Templates

> 路由皆位於 `/api/v1/tenants/{tenantId}/admin/gaming/...`，需 JWT + Policy `TenantUser` + Permission `GAMING:DRAW-TEMPLATE:MANAGE`。

#### [POST] /api/v1/tenants/{tenantId}/admin/gaming/draw-templates - 建立期數模板
**Auth:** JWT + Policy `TenantUser` + Permission `GAMING:DRAW-TEMPLATE:MANAGE`

**Request**
- Body `CreateDrawTemplateRequest`
  | name | type | required | description |
  |---|---|---|---|
  | gameCode | string | ✅ | 遊戲代碼。 |
  | name | string | ✅ | 模板名稱。 |
  | isActive | bool | ✅ | 是否啟用。 |
  | playTypes | array | ✅ | 啟用玩法與獎項。 |
  | allowedTicketTemplateIds | Guid[] | ✅ | 允許票種模板列表。 |
  - `prizeTiers.option` 需包含派彩金額 `payoutAmount`（非成本）。【F:src/Web.Api/Endpoints/Admin/Requests/DrawTemplateRequests.cs†L1-L31】

- Example request
```bash
curl -X POST "$BASE_URL/api/v1/tenants/22222222-2222-2222-2222-222222222222/admin/gaming/draw-templates" \
  -H "Authorization: Bearer <jwt>" \
  -H "Content-Type: application/json" \
  -d '{
    "gameCode": "LOTTERY539",
    "name": "標準模板",
    "isActive": true,
    "playTypes": [
      {
        "playTypeCode": "BASIC",
        "prizeTiers": [
          {
            "tier": "T1",
            "option": {
              "prizeId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
              "name": "頭獎",
              "cost": 1000,
              "payoutAmount": 5000,
              "redeemValidDays": 30,
              "description": "頭獎獎項"
            }
          }
        ]
      }
    ],
    "allowedTicketTemplateIds": [
      "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"
    ]
  }'
```

**Response**
- 200: `Guid`（templateId）。

---

#### [PUT] /api/v1/tenants/{tenantId}/admin/gaming/draw-templates/{templateId} - 更新期數模板
**Auth:** JWT + Policy `TenantUser` + Permission `GAMING:DRAW-TEMPLATE:MANAGE`

**Request**
- Body `UpdateDrawTemplateRequest`
  | name | type | required | description |
  |---|---|---|---|
  | name | string | ✅ | 模板名稱。 |
  | playTypes | array | ✅ | 啟用玩法與獎項。 |
  | allowedTicketTemplateIds | Guid[] | ✅ | 允許票種模板列表。 |
  - `prizeTiers.option` 需包含派彩金額 `payoutAmount`（非成本）。【F:src/Web.Api/Endpoints/Admin/Requests/DrawTemplateRequests.cs†L1-L31】

**Response**
- 200: `OK`

---

#### [POST] /api/v1/tenants/{tenantId}/admin/gaming/draw-templates/{templateId}/activate - 啟用期數模板
**Auth:** JWT + Policy `TenantUser` + Permission `GAMING:DRAW-TEMPLATE:MANAGE`

**Response**
- 200: `OK`

---

#### [POST] /api/v1/tenants/{tenantId}/admin/gaming/draw-templates/{templateId}/deactivate - 停用期數模板
**Auth:** JWT + Policy `TenantUser` + Permission `GAMING:DRAW-TEMPLATE:MANAGE`

**Response**
- 200: `OK`

---

#### [GET] /api/v1/tenants/{tenantId}/admin/gaming/draw-templates - 期數模板列表
**Auth:** JWT + Policy `TenantUser` + Permission `GAMING:DRAW-TEMPLATE:MANAGE`

**Request**
- Query: `gameCode` (string?), `isActive` (bool?)

**Response**
- 200: `DrawTemplateSummaryDto[]`

---

#### [GET] /api/v1/tenants/{tenantId}/admin/gaming/draw-templates/{templateId} - 期數模板詳情
**Auth:** JWT + Policy `TenantUser` + Permission `GAMING:DRAW-TEMPLATE:MANAGE`

**Response**
- 200: `DrawTemplateDetailDto`

---

#### [GET] /api/v1/tenants/{tenantId}/gaming/lottery539/draws - 期數列表
**Auth:** None (AllowAnonymous)【F:src/Web.Api/Endpoints/Gaming/GamingEndpoints.cs†L87-L101】

**Request**
- Query: `status` (string?)

- Example request
```bash
curl "$BASE_URL/api/v1/tenants/22222222-2222-2222-2222-222222222222/gaming/lottery539/draws?status=SalesOpen"
```

**Response**
- 200: `DrawSummaryDto[]`。
- Example response
```json
[
  {
    "id": "77777777-7777-7777-7777-777777777777",
    "salesStartAt": "2024-01-01T00:00:00Z",
    "salesCloseAt": "2024-01-01T12:00:00Z",
    "drawAt": "2024-01-01T13:00:00Z",
    "status": "SalesOpen"
  }
]
```

---

#### [GET] /api/v1/tenants/{tenantId}/gaming/draws/selling/options - 可售票期數下拉選項
**Auth:** None (AllowAnonymous)

**用途**
- 提供前端 DropdownList 使用的可售票期數選項清單，時間皆為 UTC。

**Request**
- Query: `gameCode` (string?)，不填則回傳所有可售票期數。
- Query: `playTypeCode` (string?)，只回傳包含該玩法已啟用的期數。
- Query: `take` (int?，預設 50，上限 200)

- Example request
```bash
curl "$BASE_URL/api/v1/tenants/22222222-2222-2222-2222-222222222222/gaming/draws/selling/options?gameCode=lottery539&playTypeCode=STRAIGHT&take=50"
```

**Response**
- 200: `DrawSellingOptionDto[]`
- Example response
```json
[
  {
    "value": "77777777-7777-7777-7777-777777777777",
    "label": "lottery539 | 售票至 2024-01-01 12:00 (UTC) | 開獎 2024-01-01 13:00 (UTC)",
    "salesCloseAtUtc": "2024-01-01T12:00:00Z",
    "drawAtUtc": "2024-01-01T13:00:00Z"
  }
]
```

**排序規則**
- `SalesCloseAtUtc` ASC，次排序 `DrawAtUtc` ASC。

**備註**
- 回傳時間為 UTC，前端可自行依 tenant timezone 顯示。

---

#### [GET] /api/v1/tenants/{tenantId}/gaming/lottery539/draws/{drawId} - 期數詳情
**Auth:** None (AllowAnonymous)

**Example request**
```bash
curl "$BASE_URL/api/v1/tenants/22222222-2222-2222-2222-222222222222/gaming/lottery539/draws/77777777-7777-7777-7777-777777777777"
```

**Response**
- 200: `DrawDetailDto`。
- 404: draw 不存在。 
- Example response
```json
{
  "id": "77777777-7777-7777-7777-777777777777",
  "salesStartAt": "2024-01-01T00:00:00Z",
  "salesCloseAt": "2024-01-01T12:00:00Z",
  "drawAt": "2024-01-01T13:00:00Z",
  "status": "SalesOpen",
  "isManuallyClosed": false,
  "manualCloseAt": null,
  "manualCloseReason": null,
  "redeemValidDays": 30,
  "winningNumbers": null,
  "serverSeedHash": null,
  "serverSeed": null,
  "algorithm": null,
  "derivedInput": null
}
```

---

#### [POST] /api/v1/tenants/{tenantId}/gaming/lottery539/draws/{drawId}/tickets - 會員下注
**Auth:** JWT + Policy `Member`【F:src/Web.Api/Endpoints/Gaming/GamingEndpoints.cs†L118-L131】

**Request**
- Body `PlaceTicketRequest`
  | name | type | required | description |
  |---|---|---|---|
  | templateId | guid | ✅ | 票種模板 Id。 |
  | lines | int[][] | ✅ | 每注號碼列表。 |

- Example request
```bash
curl -X POST "$BASE_URL/api/v1/tenants/22222222-2222-2222-2222-222222222222/gaming/lottery539/draws/77777777-7777-7777-7777-777777777777/tickets" \
  -H "Authorization: Bearer <jwt>" \
  -H "Content-Type: application/json" \
  -d '{"templateId":"88888888-8888-8888-8888-888888888888","lines":[[1,2,3,4,5],[6,7,8,9,10]]}'
```

**Response**
- 200: `Guid`（ticketId）
- Example response
```json
"99999999-9999-9999-9999-999999999999"
```

---

#### [POST] /api/v1/tenants/{tenantId}/gaming/lottery539/draws/{drawId}/execute - 開獎
**Auth:** JWT + Policy `TenantUser`

**Example request**
```bash
curl -X POST "$BASE_URL/api/v1/tenants/22222222-2222-2222-2222-222222222222/gaming/lottery539/draws/77777777-7777-7777-7777-777777777777/execute" \
  -H "Authorization: Bearer <jwt>"
```

**Response**
- 200 OK
- Example response
```
HTTP/1.1 200 OK
```

---

#### [POST] /api/v1/tenants/{tenantId}/gaming/lottery539/draws/{drawId}/settle - 結算
**Auth:** JWT + Policy `TenantUser`

**Example request**
```bash
curl -X POST "$BASE_URL/api/v1/tenants/22222222-2222-2222-2222-222222222222/gaming/lottery539/draws/77777777-7777-7777-7777-777777777777/settle" \
  -H "Authorization: Bearer <jwt>"
```

**Response**
- 200 OK
- Example response
```
HTTP/1.1 200 OK
```

---

#### [POST] /api/v1/tenants/{tenantId}/gaming/lottery539/draws/{drawId}/manual-close - 手動封盤
**Auth:** JWT + Policy `TenantUser`

**Request**
- Body `CloseDrawManuallyRequest` (`reason` optional)

- Example request
```bash
curl -X POST "$BASE_URL/api/v1/tenants/22222222-2222-2222-2222-222222222222/gaming/lottery539/draws/77777777-7777-7777-7777-777777777777/manual-close" \
  -H "Authorization: Bearer <jwt>" \
  -H "Content-Type: application/json" \
  -d '{"reason":"臨時封盤"}'
```

**Response**
- 200 OK
- Example response
```
HTTP/1.1 200 OK
```

---

#### [POST] /api/v1/tenants/{tenantId}/gaming/lottery539/draws/{drawId}/reopen - 重新開盤
**Auth:** JWT + Policy `TenantUser`

**Example request**
```bash
curl -X POST "$BASE_URL/api/v1/tenants/22222222-2222-2222-2222-222222222222/gaming/lottery539/draws/77777777-7777-7777-7777-777777777777/reopen" \
  -H "Authorization: Bearer <jwt>"
```

**Response**
- 200 OK
- Example response
```
HTTP/1.1 200 OK
```

---

#### [GET] /api/v1/tenants/{tenantId}/gaming/lottery539/draws/{drawId}/allowed-ticket-templates - 允許票種
**Auth:** JWT + Policy `TenantUser`

**Example request**
```bash
curl "$BASE_URL/api/v1/tenants/22222222-2222-2222-2222-222222222222/gaming/lottery539/draws/77777777-7777-7777-7777-777777777777/allowed-ticket-templates" \
  -H "Authorization: Bearer <jwt>"
```

**Response**
- 200: `DrawAllowedTicketTemplateDto[]`
- Example response
```json
[
  {
    "ticketTemplateId": "88888888-8888-8888-8888-888888888888",
    "code": "STD",
    "name": "標準票",
    "type": "Standard",
    "price": 50,
    "isActive": true
  }
]
```

---

#### [PUT] /api/v1/tenants/{tenantId}/gaming/lottery539/draws/{drawId}/allowed-ticket-templates - 更新允許票種
**Auth:** JWT + Policy `TenantUser`

**Request**
- Body `UpdateDrawAllowedTicketTemplatesRequest` (`templateIds: Guid[]`)

- Example request
```bash
curl -X PUT "$BASE_URL/api/v1/tenants/22222222-2222-2222-2222-222222222222/gaming/lottery539/draws/77777777-7777-7777-7777-777777777777/allowed-ticket-templates" \
  -H "Authorization: Bearer <jwt>" \
  -H "Content-Type: application/json" \
  -d '{"templateIds":["88888888-8888-8888-8888-888888888888"]}'
```

**Response**
- 200 OK
- Example response
```
HTTP/1.1 200 OK
```

---

#### [GET] /api/v1/tenants/{tenantId}/gaming/lottery539/draws/{drawId}/prize-mappings - 獎項對應
**Auth:** JWT + Policy `TenantUser`

**Example request**
```bash
curl "$BASE_URL/api/v1/tenants/22222222-2222-2222-2222-222222222222/gaming/lottery539/draws/77777777-7777-7777-7777-777777777777/prize-mappings" \
  -H "Authorization: Bearer <jwt>"
```

**Response**
- 200: `DrawPrizeMappingDto[]`
- Example response
```json
[
  {
    "matchCount": 5,
    "prizes": [
      {
        "prizeId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
        "prizeName": "頭獎",
        "prizeCost": 1000,
        "isActive": true
      }
    ]
  }
]
```

---

#### [PUT] /api/v1/tenants/{tenantId}/gaming/lottery539/draws/{drawId}/prize-mappings - 更新獎項對應
**Auth:** JWT + Policy `TenantUser`

**Request**
- Body `UpdateDrawPrizeMappingsRequest`
  - `mappings`: `[{ matchCount: int, prizeIds: Guid[] }]`

- Example request
```bash
curl -X PUT "$BASE_URL/api/v1/tenants/22222222-2222-2222-2222-222222222222/gaming/lottery539/draws/77777777-7777-7777-7777-777777777777/prize-mappings" \
  -H "Authorization: Bearer <jwt>" \
  -H "Content-Type: application/json" \
  -d '{"mappings":[{"matchCount":5,"prizeIds":["aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"]}]}'
```

**Response**
- 200 OK
- Example response
```
HTTP/1.1 200 OK
```

---

#### [GET] /api/v1/tenants/{tenantId}/gaming/lottery539/members/me/tickets - 會員票券查詢
**Auth:** JWT + Policy `Member`

**Request**
- Query: `from` / `to` (DateTime?)

- Example request
```bash
curl "$BASE_URL/api/v1/tenants/22222222-2222-2222-2222-222222222222/gaming/lottery539/members/me/tickets?from=2024-01-01T00:00:00Z&to=2024-01-31T23:59:59Z" \
  -H "Authorization: Bearer <jwt>"
```

**Response**
- 200: `TicketSummaryDto[]`
- Example response
```json
[
  {
    "ticketId": "99999999-9999-9999-9999-999999999999",
    "drawId": "77777777-7777-7777-7777-777777777777",
    "totalCost": 100,
    "createdAt": "2024-01-01T10:00:00Z",
    "lines": [
      { "lineIndex": 0, "numbers": "1,2,3,4,5", "matchedCount": 2 }
    ]
  }
]
```

---

#### [GET] /api/v1/tenants/{tenantId}/gaming/members/me/tickets/available-for-bet - 取得可下注票券
**Auth:** JWT + Policy `Member`

**Staff Endpoint:** `/api/v1/tenants/{tenantId}/admin/members/{memberId}/tickets/available-for-bet`（JWT + Policy `TenantUser` + Permission `tickets.read`）

**Request**
- Query: `drawId` (Guid? optional), `limit` (int? optional, default 200)

- Example request
```bash
curl "$BASE_URL/api/v1/tenants/22222222-2222-2222-2222-222222222222/gaming/members/me/tickets/available-for-bet?drawId=77777777-7777-7777-7777-777777777777&limit=200" \
  -H "Authorization: Bearer <jwt>"
```

**Response**
- 200: `AvailableTicketsResponse`
- Example response
```json
{
  "items": [
    {
      "ticketId": "11111111-1111-1111-1111-111111111111",
      "displayText": "Ticket 11111111111111111111111111111111 | LOTTERY539 | Close 2026-01-25T10:00:00.0000000Z",
      "gameCode": "LOTTERY539",
      "drawId": "77777777-7777-7777-7777-777777777777",
      "salesCloseAtUtc": "2026-01-25T10:00:00Z",
      "expiresAtUtc": null,
      "availablePlayTypes": [
        {
          "playTypeCode": "BASIC",
          "displayName": "BASIC"
        }
      ]
    }
  ]
}
```

**Notes**
- `salesCloseAtUtc` / `expiresAtUtc` 為 UTC，前端顯示需依租戶時區轉換。

---

#### [GET] /api/v1/tenants/{tenantId}/gaming/lottery539/members/me/awards - 會員得獎查詢
**Auth:** JWT + Policy `Member`

**Request**
- Query: `status` (string?)

- Example request
```bash
curl "$BASE_URL/api/v1/tenants/22222222-2222-2222-2222-222222222222/gaming/lottery539/members/me/awards?status=Awarded" \
  -H "Authorization: Bearer <jwt>"
```

**Response**
- 200: `PrizeAwardDto[]`
- Example response
```json
[
  {
    "awardId": "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
    "drawId": "77777777-7777-7777-7777-777777777777",
    "ticketId": "99999999-9999-9999-9999-999999999999",
    "lineIndex": 0,
    "matchedCount": 5,
    "prizeId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
    "prizeName": "頭獎",
    "status": "Awarded",
    "awardedAt": "2024-01-01T13:00:00Z",
    "expiresAt": "2024-01-31T00:00:00Z",
    "redeemedAt": null,
    "costSnapshot": 1000,
    "options": [
      { "prizeId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa", "prizeName": "頭獎", "prizeCost": 1000 }
    ]
  }
]
```

---

#### [POST] /api/v1/tenants/{tenantId}/gaming/prizes/awards/{awardId}/redeem - 兌獎
**Auth:** JWT + Policy `Member`

**Request**
- Body `RedeemPrizeAwardRequest` (`prizeId`, `note`?)

- Example request
```bash
curl -X POST "$BASE_URL/api/v1/tenants/22222222-2222-2222-2222-222222222222/gaming/prizes/awards/bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb/redeem" \
  -H "Authorization: Bearer <jwt>" \
  -H "Content-Type: application/json" \
  -d '{"prizeId":"aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa","note":"現場領取"}'
```

**Response**
- 200: `Guid`（redeem record 或 award id）
- Example response
```json
"bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"
```

---

#### [GET] /api/v1/tenants/{tenantId}/gaming/prizes - 獎品列表
**Auth:** JWT + Policy `TenantUser`

**Example request**
```bash
curl "$BASE_URL/api/v1/tenants/22222222-2222-2222-2222-222222222222/gaming/prizes" \
  -H "Authorization: Bearer <jwt>"
```

**Response**
- 200: `PrizeDto[]`
- Example response
```json
[
  {
    "id": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
    "name": "頭獎",
    "description": "獎品描述",
    "cost": 1000,
    "isActive": true,
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-01T00:00:00Z"
  }
]
```

---

#### [POST] /api/v1/tenants/{tenantId}/gaming/prizes - 建立獎品
**Auth:** JWT + Policy `TenantUser`

**Request**
- Body `CreatePrizeRequest` (`name`, `description?`, `cost`)

- Example request
```bash
curl -X POST "$BASE_URL/api/v1/tenants/22222222-2222-2222-2222-222222222222/gaming/prizes" \
  -H "Authorization: Bearer <jwt>" \
  -H "Content-Type: application/json" \
  -d '{"name":"頭獎","description":"獎品描述","cost":1000}'
```

**Response**
- 200: `Guid`（prizeId）
- Example response
```json
"aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"
```

---

#### [PUT] /api/v1/tenants/{tenantId}/gaming/prizes/{prizeId} - 更新獎品
**Auth:** JWT + Policy `TenantUser`

**Request**
- Body `UpdatePrizeRequest`

- Example request
```bash
curl -X PUT "$BASE_URL/api/v1/tenants/22222222-2222-2222-2222-222222222222/gaming/prizes/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa" \
  -H "Authorization: Bearer <jwt>" \
  -H "Content-Type: application/json" \
  -d '{"name":"頭獎","description":"更新描述","cost":1200}'
```

**Response**
- 200 OK
- Example response
```
HTTP/1.1 200 OK
```

---

#### [PATCH] /api/v1/tenants/{tenantId}/gaming/prizes/{prizeId}/activate - 啟用獎品
**Auth:** JWT + Policy `TenantUser`

**Example request**
```bash
curl -X PATCH "$BASE_URL/api/v1/tenants/22222222-2222-2222-2222-222222222222/gaming/prizes/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa/activate" \
  -H "Authorization: Bearer <jwt>"
```

**Response**
- 200 OK
- Example response
```
HTTP/1.1 200 OK
```

---

#### [PATCH] /api/v1/tenants/{tenantId}/gaming/prizes/{prizeId}/deactivate - 停用獎品
**Auth:** JWT + Policy `TenantUser`

**Example request**
```bash
curl -X PATCH "$BASE_URL/api/v1/tenants/22222222-2222-2222-2222-222222222222/gaming/prizes/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa/deactivate" \
  -H "Authorization: Bearer <jwt>"
```

**Response**
- 200 OK
- Example response
```
HTTP/1.1 200 OK
```

---

#### [GET] /api/v1/tenants/{tenantId}/gaming/ticket-templates - 票種模板列表
**Auth:** JWT + Policy `TenantUser`

**Request**
- Query: `activeOnly` (bool)

- Example request
```bash
curl "$BASE_URL/api/v1/tenants/22222222-2222-2222-2222-222222222222/gaming/ticket-templates?activeOnly=true" \
  -H "Authorization: Bearer <jwt>"
```

**Response**
- 200: `TicketTemplateDto[]`
- Example response
```json
[
  {
    "id": "88888888-8888-8888-8888-888888888888",
    "code": "STD",
    "name": "標準票",
    "type": "Standard",
    "price": 50,
    "isActive": true,
    "validFrom": null,
    "validTo": null,
    "maxLinesPerTicket": 5,
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-01T00:00:00Z"
  }
]
```

---

#### [POST] /api/v1/tenants/{tenantId}/gaming/ticket-templates - 建立票種模板
**Auth:** JWT + Policy `TenantUser`

**Request**
- Body `CreateTicketTemplateRequest`

- Example request
```bash
curl -X POST "$BASE_URL/api/v1/tenants/22222222-2222-2222-2222-222222222222/gaming/ticket-templates" \
  -H "Authorization: Bearer <jwt>" \
  -H "Content-Type: application/json" \
  -d '{"code":"STD","name":"標準票","type":0,"price":50,"validFrom":null,"validTo":null,"maxLinesPerTicket":5}'
```

**Response**
- 200: `Guid`（templateId）
- Example response
```json
"88888888-8888-8888-8888-888888888888"
```

---

#### [PUT] /api/v1/tenants/{tenantId}/gaming/ticket-templates/{templateId} - 更新票種模板
**Auth:** JWT + Policy `TenantUser`

**Request**
- Body `UpdateTicketTemplateRequest`

- Example request
```bash
curl -X PUT "$BASE_URL/api/v1/tenants/22222222-2222-2222-2222-222222222222/gaming/ticket-templates/88888888-8888-8888-8888-888888888888" \
  -H "Authorization: Bearer <jwt>" \
  -H "Content-Type: application/json" \
  -d '{"code":"STD","name":"標準票","type":0,"price":60,"validFrom":null,"validTo":null,"maxLinesPerTicket":5}'
```

**Response**
- 200 OK
- Example response
```
HTTP/1.1 200 OK
```

---

#### [PATCH] /api/v1/tenants/{tenantId}/gaming/ticket-templates/{templateId}/activate - 啟用票種模板
**Auth:** JWT + Policy `TenantUser`

**Example request**
```bash
curl -X PATCH "$BASE_URL/api/v1/tenants/22222222-2222-2222-2222-222222222222/gaming/ticket-templates/88888888-8888-8888-8888-888888888888/activate" \
  -H "Authorization: Bearer <jwt>"
```

**Response**
- 200 OK
- Example response
```
HTTP/1.1 200 OK
```

---

#### [PATCH] /api/v1/tenants/{tenantId}/gaming/ticket-templates/{templateId}/deactivate - 停用票種模板
**Auth:** JWT + Policy `TenantUser`

**Example request**
```bash
curl -X PATCH "$BASE_URL/api/v1/tenants/22222222-2222-2222-2222-222222222222/gaming/ticket-templates/88888888-8888-8888-8888-888888888888/deactivate" \
  -H "Authorization: Bearer <jwt>"
```

**Response**
- 200 OK
- Example response
```
HTTP/1.1 200 OK
```

---

#### [GET] /api/v1/tenants/{tenantId}/gaming/lottery539/prize-rules - 中獎規則列表
**Auth:** JWT + Policy `TenantUser`

**Example request**
```bash
curl "$BASE_URL/api/v1/tenants/22222222-2222-2222-2222-222222222222/gaming/lottery539/prize-rules" \
  -H "Authorization: Bearer <jwt>"
```

**Response**
- 200: `PrizeRuleDto[]`
- Example response
```json
[
  {
    "id": "cccccccc-cccc-cccc-cccc-cccccccccccc",
    "matchCount": 5,
    "prizeId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
    "prizeName": "頭獎",
    "isActive": true,
    "effectiveFrom": null,
    "effectiveTo": null,
    "redeemValidDays": 30
  }
]
```

---

#### [POST] /api/v1/tenants/{tenantId}/gaming/lottery539/prize-rules - 建立中獎規則
**Auth:** JWT + Policy `TenantUser`

**Request**
- Body `CreatePrizeRuleRequest`

- Example request
```bash
curl -X POST "$BASE_URL/api/v1/tenants/22222222-2222-2222-2222-222222222222/gaming/lottery539/prize-rules" \
  -H "Authorization: Bearer <jwt>" \
  -H "Content-Type: application/json" \
  -d '{"matchCount":5,"prizeId":"aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa","effectiveFrom":null,"effectiveTo":null,"redeemValidDays":30}'
```

**Response**
- 200: `Guid`（ruleId）
- Example response
```json
"cccccccc-cccc-cccc-cccc-cccccccccccc"
```

---

#### [PUT] /api/v1/tenants/{tenantId}/gaming/lottery539/prize-rules/{ruleId} - 更新中獎規則
**Auth:** JWT + Policy `TenantUser`

**Request**
- Body `UpdatePrizeRuleRequest`

- Example request
```bash
curl -X PUT "$BASE_URL/api/v1/tenants/22222222-2222-2222-2222-222222222222/gaming/lottery539/prize-rules/cccccccc-cccc-cccc-cccc-cccccccccccc" \
  -H "Authorization: Bearer <jwt>" \
  -H "Content-Type: application/json" \
  -d '{"matchCount":5,"prizeId":"aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa","effectiveFrom":null,"effectiveTo":null,"redeemValidDays":45}'
```

**Response**
- 200 OK
- Example response
```
HTTP/1.1 200 OK
```

---

#### [PATCH] /api/v1/tenants/{tenantId}/gaming/lottery539/prize-rules/{ruleId}/activate - 啟用中獎規則
**Auth:** JWT + Policy `TenantUser`

**Example request**
```bash
curl -X PATCH "$BASE_URL/api/v1/tenants/22222222-2222-2222-2222-222222222222/gaming/lottery539/prize-rules/cccccccc-cccc-cccc-cccc-cccccccccccc/activate" \
  -H "Authorization: Bearer <jwt>"
```

**Response**
- 200 OK
- Example response
```
HTTP/1.1 200 OK
```

---

#### [PATCH] /api/v1/tenants/{tenantId}/gaming/lottery539/prize-rules/{ruleId}/deactivate - 停用中獎規則
**Auth:** JWT + Policy `TenantUser`

**Example request**
```bash
curl -X PATCH "$BASE_URL/api/v1/tenants/22222222-2222-2222-2222-222222222222/gaming/lottery539/prize-rules/cccccccc-cccc-cccc-cccc-cccccccccccc/deactivate" \
  -H "Authorization: Bearer <jwt>"
```

**Response**
- 200 OK
- Example response
```
HTTP/1.1 200 OK
```

---

### Account

#### [GET] /api/v1/account/me - 取得當前登入資訊
**Auth:** JWT (只需登入)【F:src/Web.Api/Endpoints/Account/Me.cs†L9-L19】

**Example request**
```bash
curl "$BASE_URL/api/v1/account/me" -H "Authorization: Bearer <jwt>"
```

**Response**
- 200: `string`，格式 `Hi, userId = {sub}`（從 claim `sub` 讀取）。
- Example response
```json
"Hi, userId = 11111111-1111-1111-1111-111111111111"
```

**Unknown / Needs confirmation**
- JWT 產生的 userId claim 使用 `ClaimTypes.NameIdentifier`，但此端點取 `sub`，可能為空。需確認是否有 claim mapping 或要求統一 claim。 【F:src/Web.Api/Endpoints/Account/Me.cs†L12-L15】【F:src/Infrastructure/Authentication/JwtService.cs†L32-L40】

---

### Health

#### [GET] /health - 健康檢查
**Auth:** None【F:src/Web.Api/Program.cs†L66-L70】

**Example request**
```bash
curl "$BASE_URL/health"
```

**Response**
- 200（由 HealthChecks UI 回應）。
- Example response
```json
{ "status": "Healthy" }
```

**Unknown / Needs confirmation**
- 實際輸出格式由 `HealthChecks.UI.Client.UIResponseWriter` 決定，可能包含更多欄位（entries / duration 等）。需依部署版本確認。 【F:src/Web.Api/Program.cs†L66-L70】

---

## 4. 產出方式與可維護性

### 4.1 產出方式（本次）
- 以靜態掃描方式整理：
  - Minimal API routes：`src/Web.Api/Endpoints/*` 及 `Program.cs` 路由群組。【F:src/Web.Api/Program.cs†L39-L50】【F:src/Web.Api/Endpoints/Gaming/GamingEndpoints.cs†L31-L494】
  - DTO / Request / Response：`src/Application/*` 與 `src/Web.Api/Endpoints/*/Requests`。【F:src/Application/Members/Dtos/MemberDetailDto.cs†L1-L9】【F:src/Web.Api/Endpoints/Members/Requests/CreateMemberRequest.cs†L1-L3】
  - 驗證規則：FluentValidation validators（Users/Roles 等）。【F:src/Application/Users/Create/CreateUserCommandValidator.cs†L5-L16】【F:src/Application/Roles/Update/UpdateRoleCommandValidator.cs†L5-L16】
  - 授權與錯誤：`Permission` / `AuthorizationPolicyNames` / `CustomResults` / `GlobalExceptionHandler`。【F:src/Domain/Security/Permission.cs†L1-L171】【F:src/Application/Abstractions/Authorization/AuthorizationPolicyNames.cs†L1-L12】【F:src/Web.Api/Infrastructure/CustomResults.cs†L8-L70】
  - Tenant / 時區：`TenantResolutionMiddleware`、`TenantTimeZoneProvider`、`UtcRangeCalculator`。【F:src/Web.Api/Middleware/TenantResolutionMiddleware.cs†L12-L171】【F:src/Infrastructure/Tenants/TenantTimeZoneProvider.cs†L12-L47】【F:src/Application/Time/UtcRangeCalculator.cs†L6-L27】

### 4.2 建議維護方式（未實作）
- 逐步導入 OpenAPI/NSwag 或在 endpoint 上加上完整的 `WithOpenApi` / `Produces` metadata，以自動輸出前端文件。
- 若要保持輕量，可建立 Roslyn 掃描工具（/tools/api-doc-gen）定期解析 `MapGet/MapPost` + DTO，輸出 Markdown。
- 本次未新增 tool，避免改動執行環境；若後續要自動化，可從上述文件路徑作為掃描入口。
