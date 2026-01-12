# Gaming（539）模組說明

## 模組目的
Gaming 模組負責 539 遊戲的期數管理、下注、開獎、公平性驗證、結算與兌換流程。模組遵循 Clean Architecture：
- **Domain**：定義期數、票券、獎品、規則、兌換等核心模型與規則。
- **Application**：協調命令/查詢流程，透過介面與外部服務互動。
- **Infrastructure**：提供 RNG、快取、資料存取、帳本串接等實作。
- **Web.Api**：只負責路由、授權與 request/response 對應。

## 主要流程
1. **下注**：會員在 SalesOpen 時建立 Ticket 與 TicketLine，並透過帳本服務扣點。
2. **開獎**：到達 DrawAt 後，揭露 ServerSeed 並使用 deterministic RNG 推導中獎號碼。
3. **結算**：依命中數計算獎項，建立 PrizeAward。
4. **得獎**：會員查詢得獎紀錄，Award 狀態初始為 Awarded。
5. **兌換**：會員只能兌換自己的 Award，建立 RedeemRecord 並更新狀態。

## 公平性驗證（Commit-Reveal）
- 販售開始時建立 **ServerSeedHash** 作為 commit，避免事後變更 RNG。
- 開獎時揭露 **ServerSeed**，搭配 **Algorithm** 與 **DerivedInput**（通常是 drawId）可重算中獎號碼。
- 外部驗證方式：以相同 seed 與 input 重算號碼並比對 WinningNumbers。

## Idempotency 保護點
- **SettleDraw**：以 TenantId + DrawId + TicketId + LineIndex 避免重複建立 PrizeAward。
- **Redeem**：先查是否已有 RedeemRecord，避免重複兌換。
- **Ledger reference**：下注扣點使用 ticket.Id 作為 referenceId，避免重複扣點。

## 待改進事項
- 若未來需支援多種 RNG 或更多獎項規則，應擴充 proof 與規則解析策略，並保留既有驗證格式以維持向後相容。
