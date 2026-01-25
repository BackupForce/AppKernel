# AppKernel API 摘要

此文件彙總 `Web.Api` 專案目前可用的 Minimal API 端點，方便前端測試。所有版本化路由都掛在 `api/v{version}` 下；程式啟動時預設註冊了 v1 版本集合。每個端點是否需要授權、所需權限定義及請求/回應結構都列在下方。 【F:src/Web.Api/Program.cs†L14-L60】

## 認證與錯誤格式

- **JWT Bearer**：除標示 `AllowAnonymous` 以外的端點都需要在 `Authorization` 標頭帶入 `Bearer <token>`。
- **權限控制**：部分路由額外要求特定權限（例如 `members:read`）；權限常數定義於 `Domain.Security.Permission`。 【F:src/Domain/Security/Permission.cs†L9-L83】
- **節點授權**：授權處理器僅會以 `tenantId`/`TenantId` 解析 tenant node 作為 fallback，不會再自動從路由的 `id`/`externalKey` 解析節點。若 API 需要節點授權，請在 handler 內自行從 query/header/body 解析 `nodeId` 或 `externalKey` 並手動指定。
- **錯誤響應**：使用 RFC 7807 的 `application/problem+json`，`title`/`detail` 會依錯誤類型填入，`extensions.errorCode` 會帶回錯誤碼，驗證錯誤會在 `extensions.errors` 帶回欄位訊息。 【F:src/Web.Api/Infrastructure/CustomResults.cs†L6-L80】
- **時區**：所有時間皆為 UTC。
- **Refresh Token 傳遞規則**：若 `AuthTokenOptions.UseRefreshTokenCookie = true`，refresh token 會寫入 HttpOnly Secure Cookie（Path 預設 `/auth/refresh`）；若改為 `false`，則 refresh token 會回傳於 body 且 refresh 時需在 body 傳入。

## 基本端點

### POST `/api/v1/auth/login`
- **授權**：匿名可呼叫。
- **請求體**
  ```json
  {
    "email": "user@example.com",
    "password": "string",
    "tenantCode": "string",
    "deviceId": "string|null"
  }
  ```
- **成功回應**
  ```json
  {
    "accessToken": "jwt-token",
    "accessTokenExpiresAtUtc": "2024-12-31T12:00:00Z",
    "refreshToken": "refresh-token|null",
    "sessionId": "guid"
  }
  ```
- **描述**：驗證帳密並發出 JWT/Refresh Token；若啟用 cookie 模式，refresh token 只會寫入 HttpOnly Cookie。 【F:src/Web.Api/Endpoints/Auth/Login.cs†L10-L60】【F:src/Application/Auth/LoginRequest.cs†L6-L14】【F:src/Application/Auth/LoginResponse.cs†L5-L11】

### POST `/api/v1/auth/refresh`
- **授權**：匿名可呼叫。
- **Cookie 模式**：若 `AuthTokenOptions.UseRefreshTokenCookie = true`，可不送 body。
- **請求體**
  ```json
  {
    "refreshToken": "string|null"
  }
  ```
- **成功回應**
  ```json
  {
    "accessToken": "jwt-token",
    "accessTokenExpiresAtUtc": "2024-12-31T12:00:00Z",
    "refreshToken": "refresh-token|null",
    "sessionId": "guid"
  }
  ```
- **錯誤碼**：
  - `invalid_refresh_token`
  - `refresh_token_expired`
  - `refresh_token_reused`
  - `session_revoked`
- **描述**：使用 refresh token 進行 rotation，偵測重放會撤銷整個 session。 【F:src/Web.Api/Endpoints/Auth/Refresh.cs†L9-L73】【F:src/Application/Auth/RefreshTokenCommandHandler.cs†L18-L103】

### POST `/api/v1/auth/logout`
- **授權**：匿名可呼叫。
- **Cookie 模式**：若 `AuthTokenOptions.UseRefreshTokenCookie = true`，可不送 body。
- **請求體**
  ```json
  {
    "refreshToken": "string|null"
  }
  ```
- **描述**：撤銷目前 session 的 refresh token，並清除 refresh cookie（若啟用）。 【F:src/Web.Api/Endpoints/Auth/Logout.cs†L9-L48】【F:src/Application/Auth/LogoutCommandHandler.cs†L13-L44】

### POST `/api/v1/auth/logout-all`
- **授權**：需要登入。
- **描述**：撤銷該使用者在租戶下的所有 session。 【F:src/Web.Api/Endpoints/Auth/LogoutAll.cs†L8-L27】【F:src/Application/Auth/LogoutAllCommandHandler.cs†L12-L44】

### GET `/api/v1/auth/sessions`
- **授權**：需要登入。
- **成功回應**
  ```json
  [
    {
      "id": "guid",
      "createdAtUtc": "2024-12-31T12:00:00Z",
      "lastUsedAtUtc": "2024-12-31T12:05:00Z",
      "expiresAtUtc": "2025-01-31T12:00:00Z",
      "revokedAtUtc": "2024-12-31T12:30:00Z|null",
      "revokeReason": "logout|null",
      "userAgent": "string|null",
      "ip": "string|null",
      "deviceId": "string|null"
    }
  ]
  ```
- **描述**：列出尚未過期的登入 session。 【F:src/Web.Api/Endpoints/Auth/Sessions.cs†L9-L30】【F:src/Application/Auth/GetSessionsQueryHandler.cs†L14-L44】

### DELETE `/api/v1/auth/sessions/{sessionId}`
- **授權**：需要登入。
- **描述**：撤銷指定 session。 【F:src/Web.Api/Endpoints/Auth/Sessions.cs†L32-L49】【F:src/Application/Auth/RevokeSessionCommandHandler.cs†L13-L47】

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

---

## Admin Tickets

#### [POST] `/api/v1/admin/members/{memberId}/tickets` - 後台發放 Ticket
**Auth:** JWT + Policy `TenantUser` + Permission `tickets.issue`。【F:src/Web.Api/Endpoints/Admin/AdminTicketEndpoints.cs†L23-L66】【F:src/Domain/Security/Permission.cs†L205-L223】

**Headers**
- `Idempotency-Key` (optional): 同 key 重送會回傳相同結果，不會重複發券。【F:src/Application/Gaming/Tickets/Admin/IssueMemberTicketsCommandHandler.cs†L22-L128】

**Request**
```json
{
  "gameCode": "LOTTERY539",
  "drawId": "00000000-0000-0000-0000-000000000000",
  "quantity": 2,
  "reason": "客服補發",
  "note": "VIP 會員"
}
```

**Response**
- 200: `IssueMemberTicketsResult`
```json
{
  "tickets": [
    {
      "ticketId": "11111111-1111-1111-1111-111111111111",
      "status": "Issued",
      "issuedAtUtc": "2024-01-01T00:00:00Z",
      "drawId": "00000000-0000-0000-0000-000000000000",
      "gameCode": "LOTTERY539",
      "issuedByStaffUserId": "22222222-2222-2222-2222-222222222222",
      "reason": "客服補發",
      "note": "VIP 會員"
    }
  ]
}
```

**Errors**
- 400: `Gaming.TicketIssueQuantityInvalid` / `Gaming.DrawNotOpen`
- 404: `Gaming.MemberNotFound` / `Gaming.DrawNotFound`
- 409: `Gaming.TicketIdempotencyKeyConflict`

---

#### [POST] `/api/v1/admin/tickets/{ticketId}/bet` - 後台代下注（提交投注號碼）
**Auth:** JWT + Policy `TenantUser` + Permission `tickets.placeBet`。【F:src/Web.Api/Endpoints/Admin/AdminTicketEndpoints.cs†L68-L98】【F:src/Domain/Security/Permission.cs†L205-L223】

**Headers**
- `Idempotency-Key` (optional): 同 key 重送會回傳相同結果，不會重複提交。【F:src/Application/Gaming/Tickets/Admin/PlaceTicketBetCommandHandler.cs†L22-L160】

**Request**
```json
{
  "playTypeCode": "BASIC",
  "numbers": [1, 2, 3, 4, 5],
  "clientReference": "客服單號-123",
  "note": "人工代客下注"
}
```

**Response**
- 200: `PlaceTicketBetResult`
```json
{
  "ticketId": "11111111-1111-1111-1111-111111111111",
  "status": "Submitted",
  "submittedAtUtc": "2024-01-01T00:00:00Z",
  "submittedByStaffUserId": "22222222-2222-2222-2222-222222222222",
  "bet": {
    "playTypeCode": "BASIC",
    "numbers": [1, 2, 3, 4, 5],
    "clientReference": "客服單號-123",
    "note": "人工代客下注"
  }
}
```

**Errors**
- 400: `Gaming.TicketCancelled`
- 404: `Gaming.TicketNotFound` / `Gaming.DrawNotFound`
- 409: `Gaming.TicketAlreadySubmitted` / `Gaming.TicketSubmissionClosed` / `Gaming.TicketIdempotencyKeyConflict`
- 422: `Gaming.LotteryNumbersRequired` / `Gaming.LotteryNumbersCountInvalid` / `Gaming.LotteryNumbersOutOfRange` / `Gaming.LotteryNumbersDuplicated`

---

#### [GET] `/api/v1/admin/members/{memberId}/tickets/available-for-bet` - 後台查詢會員可下注票券
**Auth:** JWT + Policy `TenantUser` + Permission `tickets.read`。【F:src/Web.Api/Endpoints/Admin/AdminTicketEndpoints.cs†L100-L125】【F:src/Domain/Security/Permission.cs†L205-L225】

**Query**
- `drawId` (optional): 只查某一期數可下注票券。
- `limit` (optional): 預設 200，最大 500。

**Response**
- 200: `AvailableTicketsResponse`
```json
{
  "items": [
    {
      "ticketId": "11111111-1111-1111-1111-111111111111",
      "displayText": "Ticket 11111111111111111111111111111111 | LOTTERY539 | BASIC | Close 2024-01-01T00:00:00.0000000Z",
      "gameCode": "LOTTERY539",
      "playTypeCode": "BASIC",
      "drawId": "77777777-7777-7777-7777-777777777777",
      "salesCloseAtUtc": "2024-01-01T00:00:00Z",
      "expiresAtUtc": null
    }
  ]
}
```

**Errors**
- 404: `Gaming.MemberNotFound`（member 不存在或不屬於 tenant）。

**Notes**
- 時間一律使用 UTC 回傳，前端依租戶時區顯示。

## Gaming - 票券

- **路由前綴**：`/api/v1/tenants/{tenantId}/gaming`
- **授權**：所有端點需登入；部分端點要求 `member` 或 `tenant user` 角色。

### POST `/api/v1/tenants/{tenantId}/gaming/tickets/issue`
- **授權**：TenantUser
- **請求體**
  ```json
  {
    "memberId": "guid",
    "campaignId": "guid",
    "ticketTemplateId": "guid|null",
    "issuedReason": "string|null"
  }
  ```
- **成功回應**
  ```json
  {
    "ticketId": "guid",
    "drawIds": ["guid"]
  }
  ```
- **描述**：客服/後台發券並回傳可參與期數。 【F:src/Web.Api/Endpoints/Gaming/Tickets/GamingTicketEndpoints.cs†L14-L49】【F:src/Application/Gaming/Tickets/Issue/IssueTicketCommandHandler.cs†L19-L168】

### POST `/api/v1/tenants/{tenantId}/gaming/tickets/campaigns/{campaignId}/claim`
- **授權**：Member
- **成功回應**
  ```json
  {
    "ticketId": "guid",
    "drawIds": ["guid"]
  }
  ```
- **描述**：會員領取活動票券，同活動僅可領取一次。 【F:src/Web.Api/Endpoints/Gaming/Tickets/GamingTicketEndpoints.cs†L51-L72】【F:src/Application/Gaming/Tickets/Claim/ClaimCampaignTicketCommandHandler.cs†L18-L154】

### POST `/api/v1/tenants/{tenantId}/gaming/tickets/{ticketId}/submit`
- **授權**：Member
- **請求體**
```json
{
  "playTypeCode": "BASIC",
  "numbers": [1, 2, 3, 4, 5]
}
```
- **描述**：提交票券號碼，僅允許一次。 【F:src/Web.Api/Endpoints/Gaming/Tickets/GamingTicketEndpoints.cs†L59-L74】【F:src/Application/Gaming/Tickets/Submit/SubmitTicketNumbersCommandHandler.cs†L26-L105】

### POST `/api/v1/tenants/{tenantId}/gaming/tickets/{ticketId}/draws/{drawId}/redeem`
- **授權**：Member
- **描述**：逐期兌獎，只能對已結算的 TicketDraw。 【F:src/Web.Api/Endpoints/Gaming/Tickets/GamingTicketEndpoints.cs†L95-L114】【F:src/Application/Gaming/Tickets/Redeem/RedeemTicketDrawCommandHandler.cs†L11-L52】

### POST `/api/v1/tenants/{tenantId}/gaming/tickets/{ticketId}/cancel`
- **授權**：TenantUser
- **請求體**
  ```json
  {
    "reason": "string|null"
  }
  ```
- **描述**：整張票券作廢，若任一期已開獎/結算則拒絕。 【F:src/Web.Api/Endpoints/Gaming/Tickets/GamingTicketEndpoints.cs†L116-L136】【F:src/Application/Gaming/Tickets/Cancel/CancelTicketCommandHandler.cs†L14-L63】

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
