# Game Module API (Gaming)

此文件整理 `Gaming` 模組對外 API，提供前端串接使用。所有路由都位於 `api/v1` 版本群組底下，並以租戶隔離。

## 基本資訊

- **Base URL**：`/api/v1/tenants/{tenantId}/gaming`
- **授權**：
  - 預設需要 `TenantUser` Policy。
  - 標示 `AllowAnonymous` 的端點可匿名呼叫。
  - 會員操作端點需 `Member` Policy（如「我的票券／得獎／兌獎／下注」）。
- **時間格式**：所有時間欄位皆為 **UTC**，格式為 ISO 8601（`2024-01-01T00:00:00Z`）。
- **錯誤格式**：`application/problem+json`（RFC 7807）。

## 權限與範圍 (Permission + Scope + Entitlement)

所有涉及 Game/Play 的操作需同時滿足：

1. **Entitlement**：租戶已啟用遊戲 / 玩法。
2. **Permission**：使用者具有對應動作權限。
3. **Scope**：權限範圍涵蓋到指定遊戲 / 玩法（資源節點樹支援父節點涵蓋子節點）。

### Scope 範例（資源節點）

- TenantRoot
  - `game:{gameCode}`
    - `play:{gameCode}:{playTypeCode}`

> 權限若綁定到 `game:{gameCode}` 節點，將涵蓋該遊戲下所有玩法。

### 主要權限代碼

- `GAMING:CATALOG:VIEW`：檢視遊戲目錄
- `GAMING:ENTITLEMENT:MANAGE`：管理租戶啟用
- `GAMING:DRAW:CREATE`：建立期數
- `GAMING:DRAW:EXECUTE`：執行開獎
- `GAMING:DRAW:SETTLE`：結算開獎
- `GAMING:DRAW:MANUAL-CLOSE`：手動封盤
- `GAMING:DRAW:REOPEN`：重新開盤
- `GAMING:DRAW:UPDATE-ALLOWED-TEMPLATES`：更新期數允許票種

### Entitlement 拒絕行為

- 若租戶未啟用指定遊戲 / 玩法，將回傳 **403 Forbidden**，錯誤碼：
  - `Gaming.GameNotEntitled`
  - `Gaming.PlayNotEntitled`

## 共用枚舉/狀態

### DrawStatus
`Scheduled` / `SalesOpen` / `SalesClosed` / `Settled` / `Cancelled`

### AwardStatus
`Awarded` / `Redeemed` / `Expired` / `Cancelled`

### TicketTemplateType
`Standard` / `Promo` / `Free` / `Vip` / `Event`

## Catalog

### 取得平台遊戲/玩法清單
`GET /catalog/games`

**Permission**：`GAMING:CATALOG:VIEW`

**Response**
```json
[
  {
    "gameCode": "LOTTERY539",
    "playTypeCodes": ["BASIC"]
  }
]
```

## Tenant Entitlement

### 查詢租戶已啟用遊戲/玩法
`GET /entitlements`

**Permission**：`GAMING:ENTITLEMENT:MANAGE`

**Response**
```json
{
  "enabledGameCodes": ["LOTTERY539"],
  "enabledPlayTypesByGame": {
    "LOTTERY539": ["BASIC"]
  }
}
```

---

### 啟用租戶遊戲
`PATCH /entitlements/games/{gameCode}/enable`

**Permission**：`GAMING:ENTITLEMENT:MANAGE`

**Response**
- `200 OK`

---

### 停用租戶遊戲
`PATCH /entitlements/games/{gameCode}/disable`

**Permission**：`GAMING:ENTITLEMENT:MANAGE`

**Response**
- `200 OK`

---

### 啟用租戶玩法
`PATCH /entitlements/games/{gameCode}/plays/{playTypeCode}/enable`

**Permission**：`GAMING:ENTITLEMENT:MANAGE`

**Response**
- `200 OK`

---

### 停用租戶玩法
`PATCH /entitlements/games/{gameCode}/plays/{playTypeCode}/disable`

**Permission**：`GAMING:ENTITLEMENT:MANAGE`

**Response**
- `200 OK`

## 期數 (Draws)

### 建立期數
`POST /games/{gameCode}/draws`

- `gameCode` 代表遊戲代碼（例如 `LOTTERY539`）。
- 若 Request Body 內有 `gameCode`，需與路徑一致。

**Permission**：`GAMING:DRAW:CREATE`

**Entitlement**：
- 租戶必須啟用該遊戲。
- `enabledPlayTypes` 必須為該遊戲的 Catalog 玩法，且租戶已啟用該玩法。

**Request Body**
```json
{
  "gameCode": "LOTTERY539",
  "enabledPlayTypes": ["BASIC"],
  "salesStartAt": "2024-01-01T00:00:00Z",
  "salesCloseAt": "2024-01-02T00:00:00Z",
  "drawAt": "2024-01-02T01:00:00Z",
  "redeemValidDays": 30
}
```

**Response**
```json
"guid"
```

---

### 取得期數清單 (開放中/指定狀態)
`GET /games/{gameCode}/draws?status=SalesOpen`

- `status` 可省略，預設為 `SalesOpen`。

**Entitlement**：租戶必須啟用該遊戲。

**Response**
```json
[
  {
    "id": "guid",
    "gameCode": "LOTTERY539",
    "salesStartAt": "2024-01-01T00:00:00Z",
    "salesCloseAt": "2024-01-02T00:00:00Z",
    "drawAt": "2024-01-02T01:00:00Z",
    "status": "SalesOpen"
  }
]
```

---

### 取得期數詳情
`GET /games/{gameCode}/draws/{drawId}`

**Entitlement**：租戶必須啟用該遊戲。

**Response**
```json
{
  "id": "guid",
  "gameCode": "LOTTERY539",
  "salesStartAt": "2024-01-01T00:00:00Z",
  "salesCloseAt": "2024-01-02T00:00:00Z",
  "drawAt": "2024-01-02T01:00:00Z",
  "status": "Settled",
  "isManuallyClosed": false,
  "manualCloseAt": null,
  "manualCloseReason": null,
  "redeemValidDays": 30,
  "winningNumbers": "01,05,09,17,39",
  "serverSeedHash": "hash",
  "serverSeed": "seed",
  "algorithm": "SHA256",
  "derivedInput": "input"
}
```

---

### 下注
`POST /games/{gameCode}/draws/{drawId}/tickets`

**Entitlement**：
- 租戶必須啟用該遊戲。
- 該玩法必須在租戶啟用範圍內，且在 Draw.EnabledPlayTypes 內。

**Request Body**
```json
{
  "playTypeCode": "BASIC",
  "templateId": "guid",
  "lines": [
    [1, 5, 9, 17, 39],
    [2, 3, 11, 27, 35]
  ]
}
```

**備註**
- 每注必須為 5 個號碼，範圍 `1-39` 且不可重複。
- `lines` 數量不可超過票種 `maxLinesPerTicket`。
- 成本 = 票種單價 `price` × 注數。

**Response**
```json
"guid"
```

---

### 開獎 (execute)
`POST /games/{gameCode}/draws/{drawId}/execute`

**Permission**：`GAMING:DRAW:EXECUTE`

**Entitlement**：租戶必須啟用該遊戲。

**Response**
- `200 OK`

---

### 結算 (settle)
`POST /games/{gameCode}/draws/{drawId}/settle`

**Permission**：`GAMING:DRAW:SETTLE`

**Entitlement**：租戶必須啟用該遊戲。

**Response**
- `200 OK`

---

### 手動封盤
`POST /games/{gameCode}/draws/{drawId}/manual-close`

**Permission**：`GAMING:DRAW:MANUAL-CLOSE`

**Entitlement**：租戶必須啟用該遊戲。

**Request Body**
```json
{
  "reason": "maintenance"
}
```

**Response**
- `200 OK`

---

### 重新開盤
`POST /games/{gameCode}/draws/{drawId}/reopen`

**Permission**：`GAMING:DRAW:REOPEN`

**Entitlement**：租戶必須啟用該遊戲。

**Response**
- `200 OK`

---

### 取得期數允許票種清單
`GET /games/{gameCode}/draws/{drawId}/allowed-ticket-templates`

**Entitlement**：租戶必須啟用該遊戲。

**Response**
```json
[
  {
    "ticketTemplateId": "guid",
    "code": "STD",
    "name": "標準票種",
    "type": "Standard",
    "price": 50,
    "isActive": true
  }
]
```

---

### 更新期數允許票種清單
`PUT /games/{gameCode}/draws/{drawId}/allowed-ticket-templates`

**Permission**：`GAMING:DRAW:UPDATE-ALLOWED-TEMPLATES`

**Entitlement**：租戶必須啟用該遊戲。

**Request Body**
```json
{
  "templateIds": ["guid", "guid"]
}
```

**Response**
- `200 OK`

## 會員功能

### 取得我的票券
`GET /games/{gameCode}/members/me/tickets?from=2024-01-01T00:00:00Z&to=2024-12-31T23:59:59Z`

**Entitlement**：租戶必須啟用該遊戲。

**Response**
```json
[
  {
    "ticketId": "guid",
    "drawId": "guid",
    "gameCode": "LOTTERY539",
    "playTypeCode": "BASIC",
    "totalCost": 100,
    "createdAt": "2024-01-01T00:00:00Z",
    "lines": [
      {
        "lineIndex": 0,
        "numbers": "01,05,09,17,39",
        "matchedCount": 2
      }
    ]
  }
]
```

---

### 取得我的得獎
`GET /games/{gameCode}/members/me/awards?status=redeemed`

- `status` 可省略；可用值：`awarded` / `redeemed`。

**Entitlement**：租戶必須啟用該遊戲。

**Response**
```json
[
  {
    "awardId": "guid",
    "drawId": "guid",
    "ticketId": "guid",
    "lineIndex": 0,
    "matchedCount": 5,
    "gameCode": "LOTTERY539",
    "playTypeCode": "BASIC",
    "prizeTier": "Tier1",
    "prizeId": "guid",
    "prizeName": "頭獎",
    "prizeCost": 1000,
    "prizeRedeemValidDays": 30,
    "prizeDescription": "獎品描述",
    "status": "Redeemed",
    "awardedAt": "2024-01-02T01:00:00Z",
    "expiresAt": "2024-02-01T01:00:00Z",
    "redeemedAt": "2024-01-05T01:00:00Z"
  }
]
```

## 兌獎

### 兌換獎項
`POST /prizes/awards/{awardId}/redeem`

**Request Body**
```json
{
  "prizeId": "guid",
  "note": "兌換備註"
}
```

**Response**
```json
"guid"
```

## 獎品 (Prizes)

### 取得獎品清單
`GET /prizes`

**Response**
```json
[
  {
    "id": "guid",
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

### 建立獎品
`POST /prizes`

**Request Body**
```json
{
  "name": "頭獎",
  "description": "獎品描述",
  "cost": 1000
}
```

**Response**
```json
"guid"
```

---

### 更新獎品
`PUT /prizes/{prizeId}`

**Request Body**
```json
{
  "name": "頭獎",
  "description": "獎品描述",
  "cost": 1000
}
```

**Response**
- `200 OK`

---

### 啟用獎品
`PATCH /prizes/{prizeId}/activate`

**Response**
- `200 OK`

---

### 停用獎品
`PATCH /prizes/{prizeId}/deactivate`

**Response**
- `200 OK`

## 票種模板 (Ticket Templates)

### 取得票種模板
`GET /ticket-templates?activeOnly=true`

**Response**
```json
[
  {
    "id": "guid",
    "code": "STD",
    "name": "標準票種",
    "type": "Standard",
    "price": 50,
    "isActive": true,
    "validFrom": "2024-01-01T00:00:00Z",
    "validTo": "2024-12-31T23:59:59Z",
    "maxLinesPerTicket": 5,
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-01T00:00:00Z"
  }
]
```

---

### 建立票種模板
`POST /ticket-templates`

**Request Body**
```json
{
  "code": "STD",
  "name": "標準票種",
  "type": "Standard",
  "price": 50,
  "validFrom": "2024-01-01T00:00:00Z",
  "validTo": "2024-12-31T23:59:59Z",
  "maxLinesPerTicket": 5
}
```

**Response**
```json
"guid"
```

---

### 更新票種模板
`PUT /ticket-templates/{templateId}`

**Request Body**
```json
{
  "code": "STD",
  "name": "標準票種",
  "type": "Standard",
  "price": 50,
  "validFrom": "2024-01-01T00:00:00Z",
  "validTo": "2024-12-31T23:59:59Z",
  "maxLinesPerTicket": 5
}
```

**Response**
- `200 OK`

---

### 啟用票種模板
`PATCH /ticket-templates/{templateId}/activate`

**Response**
- `200 OK`

---

### 停用票種模板
`PATCH /ticket-templates/{templateId}/deactivate`

**Response**
- `200 OK`
