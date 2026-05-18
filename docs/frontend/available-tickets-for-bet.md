# 前端 Codex 使用方式：取得可下注 Tickets

## 何時呼叫
- 進入下注頁時先行載入可用票券列表。
- 開啟 Dropdown/Select 前刷新一次，避免票券被其他流程使用後仍顯示。

## API 基本資訊
- **Endpoint:** `GET /api/v1/tenants/{tenantId}/gaming/members/me/tickets/available-for-bet`
- **Auth:** Member JWT (`Authorization: Bearer <token>`)
- **Query params:**
  - `drawId?: string`（可選，若下注流程已鎖定期數/Draw）
  - `limit?: number`（可選，預設 200）

> 後端時間一律以 UTC 儲存與回傳，前端需轉換成 Tenant timezone 顯示。

## TypeScript 型別
```ts
// types
export interface AvailableTicketItemDto {
  ticketId: string;
  displayText: string;
  gameCode: string;
  drawId?: string | null;
  salesCloseAtUtc?: string | null; // ISO string
  expiresAtUtc?: string | null;    // ISO string
  availablePlayTypes: TicketPlayTypeDto[];
}

export interface AvailableTicketsResponse {
  items: AvailableTicketItemDto[];
}

export interface TicketPlayTypeDto {
  playTypeCode: string;
  displayName: string;
}
```

## API 呼叫範例
```ts
// api
export async function getAvailableTicketsForBet(params?: { drawId?: string; limit?: number }) {
  const qs = new URLSearchParams();
  if (params?.drawId) qs.set("drawId", params.drawId);
  if (params?.limit) qs.set("limit", String(params.limit));

  const res = await http.get<AvailableTicketsResponse>(
    `/api/v1/tenants/${tenantId}/gaming/members/me/tickets/available-for-bet${qs.toString() ? `?${qs}` : ""}`
  );
  return res.data.items;
}
```

## Dropdown 使用範例
```ts
// usage (Dropdown)
const items = await getAvailableTicketsForBet({ drawId });
const options = items.map(x => ({ label: x.displayText, value: x.ticketId }));
```

## PlayType Dropdown 使用範例
```ts
function onTicketChange(ticketId: string) {
  const ticket = items.find(x => x.ticketId === ticketId);
  const playTypeOptions = (ticket?.availablePlayTypes ?? []).map(p => ({
    label: p.displayName,
    value: p.playTypeCode,
  }));
}
```

## 時區提醒（重要）
- `salesCloseAtUtc` / `expiresAtUtc` 一律為 UTC。
- 顯示時請使用租戶時區格式化（例如 `tenantTimeZone` + `formatInTimeZone`）。
- 若需要比較封盤時間，請先轉為 UTC 再比較，避免跨時區誤判。

## 錯誤處理建議
- `401 Unauthorized`: token 過期或未登入，導向重新登入。
- `403 Forbidden`: 非 Member 身分或權限不足。
- `500 Internal Server Error`: 顯示通用錯誤訊息並記錄 log。
