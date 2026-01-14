# Game Module API (Gaming)

此文件整理 `Gaming` 模組對外 API，提供前端串接使用。所有路由都位於 `api/v1` 版本群組底下，並以租戶隔離。

## 基本資訊

- **Base URL**：`/api/v1/tenants/{tenantId}/gaming`
- **授權**：
  - 預設需要 `TenantUser` Policy。
  - 標示 `AllowAnonymous` 的端點可匿名呼叫。
  - 會員操作端點需 `Member` Policy（如「我的票券／得獎／兌獎／下注」）。
- **錯誤格式**：`application/problem+json`（RFC 7807）。

## 共用枚舉/狀態

### DrawStatus
`Scheduled` / `SalesOpen` / `SalesClosed` / `Settled` / `Cancelled`

### AwardStatus
`Awarded` / `Redeemed` / `Expired` / `Cancelled`

### TicketTemplateType
`Standard` / `Promo` / `Free` / `Vip` / `Event`

## 期數 (Draws)

### 建立期數
`POST /games/{gameCode}/draws`

- `gameCode` 代表遊戲代碼（例如 `LOTTERY539`）。
- 若 Request Body 內有 `gameCode`，需與路徑一致。

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

**Response**
```json
[
  {
    "id": "guid",
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

**Response**
```json
{
  "id": "guid",
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

**Response**
- `200 OK`

---

### 結算 (settle)
`POST /games/{gameCode}/draws/{drawId}/settle`

**Response**
- `200 OK`

---

### 手動封盤
`POST /games/{gameCode}/draws/{drawId}/manual-close`

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

**Response**
- `200 OK`

---

### 取得期數允許票種清單
`GET /games/{gameCode}/draws/{drawId}/allowed-ticket-templates`

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

**Request Body**
```json
{
  "templateIds": ["guid", "guid"]
}
```

**Response**
- `200 OK`

---

### 取得期數獎項對應
`GET /games/{gameCode}/draws/{drawId}/prize-mappings`

**Response**
```json
[
  {
    "matchCount": 5,
    "prizes": [
      {
        "prizeId": "guid",
        "prizeName": "頭獎",
        "prizeCost": 1000,
        "isActive": true
      }
    ]
  }
]
```

---

### 更新期數獎項對應
`PUT /games/{gameCode}/draws/{drawId}/prize-mappings`

**Request Body**
```json
{
  "mappings": [
    {
      "matchCount": 5,
      "prizeIds": ["guid", "guid"]
    }
  ]
}
```

**Response**
- `200 OK`

## 會員功能

### 取得我的票券
`GET /games/{gameCode}/members/me/tickets?from=2024-01-01T00:00:00Z&to=2024-12-31T23:59:59Z`

**Response**
```json
[
  {
    "ticketId": "guid",
    "drawId": "guid",
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

**Response**
```json
[
  {
    "awardId": "guid",
    "drawId": "guid",
    "ticketId": "guid",
    "lineIndex": 0,
    "matchedCount": 5,
    "prizeId": "guid",
    "prizeName": "頭獎",
    "status": "Redeemed",
    "awardedAt": "2024-01-02T01:00:00Z",
    "expiresAt": "2024-02-01T01:00:00Z",
    "redeemedAt": "2024-01-05T01:00:00Z",
    "costSnapshot": 1000,
    "options": [
      {
        "prizeId": "guid",
        "prizeName": "頭獎",
        "prizeCost": 1000
      }
    ]
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

## 中獎規則 (Prize Rules)

### 取得中獎規則清單
`GET /games/{gameCode}/prize-rules`

**Response**
```json
[
  {
    "id": "guid",
    "matchCount": 5,
    "prizeId": "guid",
    "prizeName": "頭獎",
    "isActive": true,
    "effectiveFrom": "2024-01-01T00:00:00Z",
    "effectiveTo": "2024-12-31T23:59:59Z",
    "redeemValidDays": 30
  }
]
```

---

### 建立中獎規則
`POST /games/{gameCode}/prize-rules`

**Request Body**
```json
{
  "matchCount": 5,
  "prizeId": "guid",
  "effectiveFrom": "2024-01-01T00:00:00Z",
  "effectiveTo": "2024-12-31T23:59:59Z",
  "redeemValidDays": 30
}
```

**Response**
```json
"guid"
```

---

### 更新中獎規則
`PUT /games/{gameCode}/prize-rules/{ruleId}`

**Request Body**
```json
{
  "matchCount": 5,
  "prizeId": "guid",
  "effectiveFrom": "2024-01-01T00:00:00Z",
  "effectiveTo": "2024-12-31T23:59:59Z",
  "redeemValidDays": 30
}
```

**Response**
- `200 OK`

---

### 啟用中獎規則
`PATCH /games/{gameCode}/prize-rules/{ruleId}/activate`

**Response**
- `200 OK`

---

### 停用中獎規則
`PATCH /games/{gameCode}/prize-rules/{ruleId}/deactivate`

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
