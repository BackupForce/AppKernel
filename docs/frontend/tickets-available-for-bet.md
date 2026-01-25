# 可下注 Tickets（Member / Staff 共用）

## Member Self 使用
```ts
const items = await http.get<AvailableTicketsResponse>(`/api/members/me/tickets/available-for-bet`);
const ticketOptions = items.data.items.map(t => ({ label: t.displayText, value: t.ticketId }));

function onTicketChange(ticketId: string) {
  const ticket = items.data.items.find(x => x.ticketId === ticketId);
  const playTypeOptions = (ticket?.availablePlayTypes ?? []).map(p => ({
    label: p.displayName,
    value: p.playTypeCode,
  }));
}
```

## Staff Admin 使用
```ts
const res = await http.get<AvailableTicketsResponse>(
  `/api/admin/members/${memberId}/tickets/available-for-bet?drawId=${drawId}`
);
```

## 錯誤處理
- 401 未登入
- 403 無權限（Staff permission 不足）
- 404 member 不在 tenant / ticket 不可見
