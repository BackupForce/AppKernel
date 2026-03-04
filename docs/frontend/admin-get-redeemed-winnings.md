# 後台查詢已兌獎資料（依兌換時間區間）

## 何時呼叫
- 後台「中獎兌獎管理」頁面，切到「已兌獎」分頁時。
- 後台需要依 `redeemedAtUtc` 時間區間做報表查詢時。

## API
- **Endpoint:** `GET /api/v1/tenants/{tenantId}/admin/gaming/winnings/redeemed`
- **Auth:** JWT（TenantUser）+ Permission `Gaming.Winning.Redeemed.Read`
- **Query params:**
  - `redeemedFromUtc` (optional, ISO-8601 UTC)
  - `redeemedToUtc` (optional, ISO-8601 UTC)
  - `page` (optional, 預設 1，最小 1)
  - `pageSize` (optional, 預設 50，範圍 1~200)
- **時間規範:**
  - 請用 UTC 傳入（例如 `2026-03-01T00:00:00Z`）。
  - `redeemedFromUtc <= redeemedToUtc`，否則會回 400。

## 回傳格式（PagedResult）
```ts
export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface RedeemedWinningListItemDto {
  winningId: string;
  ticketId: string;
  memberId: string;
  memberCode: string;
  memberDisplayName: string;
  memberPhoneNumber?: string | null;
  drawCode: string;
  prizeName?: string | null;
  prizeAmount: number;
  status: number; // TicketDrawParticipationStatus（目前 API 預設為數值 enum）
  redeemedAtUtc?: string | null;
  redeemedBy?: string | null;
}
```

## 呼叫範例
```ts
export interface GetRedeemedWinningsParams {
  redeemedFromUtc?: string;
  redeemedToUtc?: string;
  page?: number;
  pageSize?: number;
}

export async function getRedeemedWinnings(
  tenantId: string,
  params: GetRedeemedWinningsParams = {}
) {
  const qs = new URLSearchParams();

  if (params.redeemedFromUtc) {
    qs.set("redeemedFromUtc", params.redeemedFromUtc);
  }

  if (params.redeemedToUtc) {
    qs.set("redeemedToUtc", params.redeemedToUtc);
  }

  qs.set("page", String(params.page ?? 1));
  qs.set("pageSize", String(params.pageSize ?? 50));

  const url = `/api/v1/tenants/${tenantId}/admin/gaming/winnings/redeemed?${qs.toString()}`;
  const response = await http.get<PagedResult<RedeemedWinningListItemDto>>(url);
  return response.data;
}
```

## Request / Response 範例
### Request
```http
GET /api/v1/tenants/11111111-1111-1111-1111-111111111111/admin/gaming/winnings/redeemed?redeemedFromUtc=2026-03-01T00:00:00Z&redeemedToUtc=2026-03-31T23:59:59Z&page=1&pageSize=20
Authorization: Bearer <jwt>
```

### Response 200
```json
{
  "items": [
    {
      "winningId": "3a2b4c5d-0f0f-4f4f-8a8a-1b2c3d4e5f60",
      "ticketId": "f2d0b0e2-ffec-44a5-9c7b-b6cb3f8890af",
      "memberId": "14d8e1d7-11cd-4d74-a99f-bec1829d4b5c",
      "memberCode": "M000123",
      "memberDisplayName": "王小明",
      "memberPhoneNumber": "0912345678",
      "drawCode": "539-20260320-001",
      "prizeName": "二獎",
      "prizeAmount": 8000,
      "status": 2,
      "redeemedAtUtc": "2026-03-20T08:31:22Z",
      "redeemedBy": "admin.user"
    }
  ],
  "totalCount": 1,
  "page": 1,
  "pageSize": 20
}
```

## 錯誤處理建議
- **400 BadRequest**
  - `page < 1`
  - `pageSize` 不在 1~200
  - `redeemedFromUtc > redeemedToUtc`
- **401 Unauthorized**：JWT 過期或未登入。
- **403 Forbidden**：無 `Gaming.Winning.Redeemed.Read` 權限。

## 前端實作建議
- 日期選擇器輸出請直接轉 UTC ISO 字串再送出。
- 列表的「兌換時間」請在 UI 層轉租戶時區顯示；後端回傳固定 UTC。
- `memberPhoneNumber` 可能為 `null`（該會員尚未填寫或資料不存在），UI 顯示 `-`。
