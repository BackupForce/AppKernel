# 後台查詢指定會員可下注票券（Dropdown 用）

## 何時呼叫
- 後台「代客下注」頁面載入時。
- Staff 選定會員後，帶入該會員的可下注票券清單。

## API
- **Endpoint:** `GET /api/v1/admin/members/{memberId}/tickets/available-for-bet`
- **Auth:** JWT（TenantUser）+ Permission `tickets.read`
- **Query params:**
  - `drawId` (optional): 只看指定期數。
  - `limit` (optional): 預設 200，最大 500。
- **時間規範:** API 回傳 UTC；前端依 Tenant 時區顯示。

## TypeScript 型別
```ts
export interface AvailableTicketItemDto {
  ticketId: string;
  displayText: string;
  gameCode?: string | null;
  playTypeCode?: string | null;
  drawId?: string | null;
  salesCloseAtUtc?: string | null;
  expiresAtUtc?: string | null;
}

export interface AvailableTicketsResponse {
  items: AvailableTicketItemDto[];
}
```

## 呼叫範例
```ts
export async function getMemberAvailableTicketsForBet(
  memberId: string,
  params?: { drawId?: string; limit?: number }
) {
  const qs = new URLSearchParams();
  if (params?.drawId) qs.set("drawId", params.drawId);
  if (params?.limit) qs.set("limit", String(params.limit));

  const url = `/api/v1/admin/members/${memberId}/tickets/available-for-bet${qs.toString() ? `?${qs}` : ""}`;
  const res = await http.get<AvailableTicketsResponse>(url);
  return res.data.items;
}
```

## Dropdown 對應
```ts
const items = await getMemberAvailableTicketsForBet(memberId, { drawId });
const options = items.map(x => ({ label: x.displayText, value: x.ticketId }));
```

## 錯誤處理建議
- **401 Unauthorized:** 請引導重新登入。
- **403 Forbidden:** Staff 無 `tickets.read` 權限，顯示「無權限」提示。
- **404 Not Found:** 會員不存在或不屬於目前 tenant，提示「查無會員」。
