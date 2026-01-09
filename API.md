# AppKernel API 摘要

此文件彙總 `Web.Api` 專案目前可用的 Minimal API 端點，方便前端測試。所有版本化路由都掛在 `api/v{version}` 下；程式啟動時預設註冊了 v1 版本集合。每個端點是否需要授權、所需權限定義及請求/回應結構都列在下方。 【F:src/Web.Api/Program.cs†L14-L60】

## 認證與錯誤格式

- **JWT Bearer**：除標示 `AllowAnonymous` 以外的端點都需要在 `Authorization` 標頭帶入 `Bearer <token>`。
- **權限控制**：部分路由額外要求特定權限（例如 `members:read`）；權限常數定義於 `Domain.Security.Permission`。 【F:src/Domain/Security/Permission.cs†L9-L83】
- **節點授權**：授權處理器僅會以 `tenantId`/`TenantId` 解析 tenant node 作為 fallback，不會再自動從路由的 `id`/`externalKey` 解析節點。若 API 需要節點授權，請在 handler 內自行從 query/header/body 解析 `nodeId` 或 `externalKey` 並手動指定。
- **錯誤響應**：使用 RFC 7807 的 `application/problem+json`，`title`/`detail` 會依錯誤類型填入，驗證錯誤會在 `extensions.errors` 帶回欄位訊息。 【F:src/Web.Api/Infrastructure/CustomResults.cs†L6-L73】

## 基本端點

### POST `/api/v1/auth/login`
- **授權**：匿名可呼叫。
- **請求體**
  ```json
  {
    "email": "user@example.com",
    "password": "string",
    "tenantCode": "string"
  }
  ```
- **成功回應**
  ```json
  {
    "token": "jwt-token",
    "expiration": "2024-12-31T12:00:00Z"
  }
  ```
- **描述**：驗證帳密並發出 JWT。 【F:src/Web.Api/Endpoints/Auth/Login.cs†L9-L26】【F:src/Application/Auth/LoginRequest.cs†L8-L16】【F:src/Application/Auth/LoginResponse.cs†L8-L16】

### GET `/api/v1/account/me`
- **授權**：需要登入（無額外權限）。
- **回應**：純文字 `"Hi, userId = <subject>"`，其中 `<subject>` 為 JWT 的 `sub` 宣告值。 【F:src/Web.Api/Endpoints/Account/Me.cs†L10-L21】

### GET `/health`
- **授權**：不需要。
- **描述**：ASP.NET Core Health Checks UI 格式的健康檢查結果。 【F:src/Web.Api/Program.cs†L62-L70】

### GET `/api/v1/permissions/catalog`
- **授權**：需要登入（無額外權限）。
- **成功回應**
  ```json
  {
    "version": "1.0",
    "scopes": [
      {
        "scope": 0,
        "displayName": "平台",
        "modules": [
          {
            "moduleKey": "TENANTS",
            "displayName": "租戶管理",
            "masterPermissionCode": "TENANTS:*",
            "items": [
              {
                "code": "TENANTS:CREATE",
                "displayName": "建立租戶",
                "description": "建立新的租戶",
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
- **描述**：回傳前端可用的 UI 友善權限目錄。 【F:src/Web.Api/Endpoints/Permissions/PermissionsEndpoints.cs†L6-L25】【F:src/Application/Authorization/PermissionCatalogDto.cs†L7-L28】【F:src/Application/Authorization/PermissionUiCatalogProvider.cs†L15-L156】

## 使用者 (Users) – 管理後台

- **路由前綴**：`/api/v{version}/users`
- **API 版本**：端點標記為 `v2.0`，但目前全域版本集合僅註冊 v1；若要呼叫請將 `{version}` 設為 `2`. 【F:src/Web.Api/Program.cs†L34-L44】【F:src/Web.Api/Endpoints/Users/UsersEndpoints.cs†L14-L35】
- **授權**：需登入；指派角色端點需 `users:update` 權限，其餘端點未額外標註權限（若有後續授權策略請依部署設定）。【F:src/Domain/Security/Permission.cs†L13-L33】【F:src/Web.Api/Endpoints/Users/UsersEndpoints.cs†L14-L63】

### GET `/api/v2/users/{id}`
- **路徑參數**：`id` (GUID)
- **成功回應**
  ```json
  {
    "id": "guid",
    "email": "user@example.com",
    "name": "User Name",
    "hasPublicProfile": true
  }
  ```
- **描述**：依 ID 取得使用者摘要，找不到時回傳 404。 【F:src/Web.Api/Endpoints/Users/UsersEndpoints.cs†L19-L29】【F:src/Application/Users/GetById/UserResponse.cs†L5-L16】

### POST `/api/v2/users`
- **請求體**
  ```json
  {
    "email": "user@example.com",
    "name": "User Name",
    "password": "string",
    "hasPublicProfile": false,
    "userType": "string|null",
    "tenantId": "guid|null"
  }
  ```
- **成功回應**：新使用者的 GUID。驗證失敗時回 400。 【F:src/Web.Api/Endpoints/Users/UsersEndpoints.cs†L31-L43】【F:src/Application/Users/Create/CreateUserRequest.cs†L3-L8】

### POST `/api/v2/users/tenant`
- **權限**：`users:create`
- **請求體**
  ```json
  {
    "email": "user@example.com",
    "name": "User Name",
    "password": "string",
    "hasPublicProfile": false
  }
  ```
- **成功回應**：新租戶使用者的 GUID。驗證失敗時回 400。 【F:src/Web.Api/Endpoints/Users/UsersEndpoints.cs†L63-L79】【F:src/Application/Users/Create/CreateTenantUserRequest.cs†L3-L7】

### POST `/api/v2/users/{userId}/roles/{roleId}`
- **權限**：`users:update`
- **路徑參數**：`userId` (GUID)、`roleId` (int)
- **成功回應**
  ```json
  {
    "userId": "guid",
    "roleIds": [1, 2, 3]
  }
  ```
- **描述**：替使用者指派角色，若使用者或角色不存在回 404，已存在角色回 409。 【F:src/Web.Api/Endpoints/Users/UsersEndpoints.cs†L45-L63】【F:src/Application/Users/AssignRole/AssignRoleToUserResultDto.cs†L1-L3】

## 角色 (Roles) – 管理後台

- **路由前綴**：`/api/v1/roles`
- **API 版本**：v1。 【F:src/Web.Api/Endpoints/Roles/RolesEndpoints.cs†L17-L24】
- **授權**：需要登入且需具備各端點指定的權限（見下方）。【F:src/Web.Api/Endpoints/Roles/RolesEndpoints.cs†L25-L112】

### POST `/api/v1/roles`
- **權限**：`roles:create`
- **請求體**
  ```json
  { "name": "Admin" }
  ```
- **成功回應**：新角色 ID（int）。 【F:src/Web.Api/Endpoints/Roles/RolesEndpoints.cs†L25-L41】【F:src/Web.Api/Endpoints/Roles/Requests/CreateRoleRequest.cs†L1-L3】

### PUT `/api/v1/roles/{id}`
- **權限**：`roles:update`
- **路徑參數**：`id` (int)
- **請求體**
  ```json
  { "name": "Administrator" }
  ```
- **描述**：更新角色名稱，成功回 200 無內容。 【F:src/Web.Api/Endpoints/Roles/RolesEndpoints.cs†L43-L57】【F:src/Web.Api/Endpoints/Roles/Requests/UpdateRoleRequest.cs†L1-L3】

### DELETE `/api/v1/roles/{id}`
- **權限**：`roles:delete`
- **路徑參數**：`id` (int)
- **描述**：刪除角色，找不到回 404。 【F:src/Web.Api/Endpoints/Roles/RolesEndpoints.cs†L59-L71】

### GET `/api/v1/roles/{id}`
- **權限**：`roles:view`
- **路徑參數**：`id` (int)
- **成功回應**
  ```json
  {
    "id": 1,
    "name": "Admin",
    "permissionCodes": ["roles:view"]
  }
  ```
- **描述**：取得角色詳情。 【F:src/Web.Api/Endpoints/Roles/RolesEndpoints.cs†L73-L87】【F:src/Application/Roles/Dtos/RoleDetailDto.cs†L1-L5】

### GET `/api/v1/roles`
- **權限**：`roles:view`
- **成功回應**
  ```json
  [
    {
      "id": 1,
      "name": "Admin",
      "permissionCount": 3
    }
  ]
  ```
- **描述**：列出角色清單。 【F:src/Web.Api/Endpoints/Roles/RolesEndpoints.cs†L89-L102】【F:src/Application/Roles/Dtos/RoleListItemDto.cs†L1-L5】

### GET `/api/v1/roles/{id}/permissions`
- **權限**：`roles:view`
- **路徑參數**：`id` (int)
- **成功回應**：角色的權限代碼清單。 【F:src/Web.Api/Endpoints/Roles/RolesEndpoints.cs†L104-L118】

### POST `/api/v1/roles/{id}/permissions`
- **權限**：`roles:update`
- **路徑參數**：`id` (int)
- **請求體**
  ```json
  {
    "permissionCodes": ["roles:view", "roles:create"]
  }
  ```
- **描述**：新增角色權限。 【F:src/Web.Api/Endpoints/Roles/RolesEndpoints.cs†L120-L135】【F:src/Web.Api/Endpoints/Roles/Requests/UpdateRolePermissionsRequest.cs†L1-L3】

### POST `/api/v1/roles/{id}/permissions/remove`
- **權限**：`roles:update`
- **路徑參數**：`id` (int)
- **請求體** 同上。
- **描述**：移除角色權限。 【F:src/Web.Api/Endpoints/Roles/RolesEndpoints.cs†L137-L152】【F:src/Web.Api/Endpoints/Roles/Requests/UpdateRolePermissionsRequest.cs†L1-L3】

## 會員 (Members) – 管理後台

- **路由前綴**：`/api/v1/members`
- **API 版本**：v1。
- **授權**：需要登入且需具備各端點指定的權限（見下方）。

### POST `/api/v1/members`
- **權限**：`members:create`
- **請求體**
  ```json
  {
    "userId": "guid|null",
    "displayName": "string",
    "memberNo": "string|null"
  }
  ```
- **成功回應**：建立的會員 GUID。 【F:src/Web.Api/Endpoints/Members/MembersEndpoints.cs†L23-L41】【F:src/Web.Api/Endpoints/Members/Requests/CreateMemberRequest.cs†L3-L3】

### GET `/api/v1/members/{id}`
- **權限**：`members:read`
- **路徑參數**：`id` (GUID)
- **成功回應**：`MemberDetailDto`（包含 `userId`、`memberNo`、`displayName`、`status`、建立/更新時間）。 【F:src/Web.Api/Endpoints/Members/MembersEndpoints.cs†L43-L56】【F:src/Application/Members/Dtos/MemberDetailDto.cs†L3-L10】

### GET `/api/v1/members`
- **權限**：`members:read`
- **查詢參數**：`memberNo`、`displayName`、`status`、`userId`、`page` (預設 1)、`pageSize` (預設 20)。
- **成功回應**：`PagedResult<MemberListItemDto>`（`items`、`totalCount`、`page`、`pageSize`）。 【F:src/Web.Api/Endpoints/Members/MembersEndpoints.cs†L58-L76】【F:src/Application/Abstractions/Data/PagedResult.cs†L3-L23】【F:src/Application/Members/Dtos/MemberListItemDto.cs†L3-L9】

### PUT `/api/v1/members/{id}`
- **權限**：`members:update`
- **請求體**
  ```json
  { "displayName": "string" }
  ```
- **描述**：更新會員暱稱，成功回 200 無內容。 【F:src/Web.Api/Endpoints/Members/MembersEndpoints.cs†L78-L89】

### POST `/api/v1/members/{id}/suspend`
- **權限**：`members:suspend`
- **請求體**
  ```json
  { "reason": "string|null" }
  ```
- **描述**：停權會員。 【F:src/Web.Api/Endpoints/Members/MembersEndpoints.cs†L91-L103】【F:src/Web.Api/Endpoints/Members/Requests/MemberStatusChangeRequest.cs†L3-L3】

### POST `/api/v1/members/{id}/activate`
- **權限**：`members:suspend`
- **請求體** 同上。
- **描述**：解除停權。 【F:src/Web.Api/Endpoints/Members/MembersEndpoints.cs†L105-L117】

### GET `/api/v1/members/{id}/points/balance`
- **權限**：`member_points:read`
- **成功回應**：`MemberPointBalanceDto`（`memberId`、`balance`、`updatedAt`）。 【F:src/Web.Api/Endpoints/Members/MembersEndpoints.cs†L119-L131】【F:src/Application/Members/Dtos/MemberPointBalanceDto.cs†L3-L7】

### GET `/api/v1/members/{id}/points/history`
- **權限**：`member_points:read`
- **查詢參數**：`startDate`、`endDate`、`type`、`referenceType`、`referenceId`、`page`、`pageSize`。
- **成功回應**：`PagedResult<MemberPointLedgerDto>`（含點數異動明細）。 【F:src/Web.Api/Endpoints/Members/MembersEndpoints.cs†L133-L152】【F:src/Application/Members/Dtos/MemberPointLedgerDto.cs†L3-L12】

### POST `/api/v1/members/{id}/points/adjust`
- **權限**：`member_points:adjust`
- **請求體**
  ```json
  {
    "delta": 100,
    "remark": "string",
    "referenceType": "admin_adjust",
    "referenceId": "string|null",
    "allowNegative": false
  }
  ```
- **成功回應**：異動後餘額（long）。 【F:src/Web.Api/Endpoints/Members/MembersEndpoints.cs†L154-L173】【F:src/Web.Api/Endpoints/Members/Requests/AdjustMemberPointsRequest.cs†L3-L7】

### GET `/api/v1/members/{id}/assets`
- **權限**：`member_assets:read`
- **成功回應**：會員各資產餘額清單（`assetCode`、`balance`、`updatedAt`）。 【F:src/Web.Api/Endpoints/Members/MembersEndpoints.cs†L175-L186】【F:src/Application/Members/Dtos/MemberAssetBalanceDto.cs†L3-L7】

### GET `/api/v1/members/{id}/assets/{assetCode}/history`
- **權限**：`member_assets:read`
- **查詢參數**：`startDate`、`endDate`、`type`、`referenceType`、`referenceId`、`page`、`pageSize`。
- **成功回應**：`PagedResult<MemberAssetLedgerDto>`（資產異動明細）。 【F:src/Web.Api/Endpoints/Members/MembersEndpoints.cs†L188-L207】【F:src/Application/Members/Dtos/MemberAssetLedgerDto.cs†L3-L12】

### POST `/api/v1/members/{id}/assets/adjust`
- **權限**：`member_assets:adjust`
- **請求體**
  ```json
  {
    "assetCode": "string",
    "delta": 10.5,
    "remark": "string",
    "referenceType": "admin_adjust",
    "referenceId": "string|null",
    "allowNegative": false
  }
  ```
- **成功回應**：異動後餘額（decimal）。 【F:src/Web.Api/Endpoints/Members/MembersEndpoints.cs†L209-L229】【F:src/Web.Api/Endpoints/Members/Requests/AdjustMemberAssetRequest.cs†L3-L7】

### GET `/api/v1/members/{id}/activity`
- **權限**：`member_audit:read`
- **查詢參數**：`startDate`、`endDate`、`action`、`page`、`pageSize`。
- **成功回應**：`PagedResult<MemberActivityLogDto>`（操作記錄，含 IP/User-Agent/Payload 等）。 【F:src/Web.Api/Endpoints/Members/MembersEndpoints.cs†L231-L244】【F:src/Application/Members/Dtos/MemberActivityLogDto.cs†L3-L10】
