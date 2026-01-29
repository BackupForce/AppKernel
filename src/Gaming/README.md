# Gaming（539）模組說明

## 模組目的
Gaming 模組負責 539 遊戲的期數管理、票券發放/領取、號碼提交、開獎、公平性驗證、結算與逐期兌換流程。模組遵循 Clean Architecture：
- **Domain**：定義期數、票券、獎品、規則、兌換等核心模型與規則。
- **Application**：協調命令/查詢流程，透過介面與外部服務互動。
- **Infrastructure**：提供 RNG、快取、資料存取、帳本串接等實作。
- **Web.Api**：只負責路由、授權與 request/response 對應。

## 主要流程
1. **發券/領券**：活動期間建立 Ticket 並展開多期 TicketDraw，未在可售視窗內的期數不會加入。
2. **提交號碼**：會員對票券提交一組號碼，並依 `Draw.IsWithinSalesWindow` 判斷各期 TicketDraw 是否 Active/Invalid。
3. **開獎**：到達 DrawAt 後，揭露 ServerSeed 並使用 deterministic RNG 推導中獎號碼。
4. **結算**：以 DrawId 查 Active 的 TicketDraw，依命中數建立 TicketLineResult，並將 TicketDraw 標記為 Settled；結算成功後 Draw 的有效狀態會顯示為 Settled。
5. **逐期兌換**：只能對 Settled 的 TicketDraw 兌換，兌換完成改為 Redeemed。

## 公平性驗證（Commit-Reveal）
- 販售開始時建立 **ServerSeedHash** 作為 commit，避免事後變更 RNG。
- 開獎時揭露 **ServerSeed**，搭配 **Algorithm** 與 **DerivedInput**（通常是 drawId）可重算中獎號碼。
- 外部驗證方式：以相同 seed 與 input 重算號碼並比對 WinningNumbers。

## Idempotency 保護點
- **SettleDraw**：以 TenantId + TicketId + DrawId + LineIndex 避免重複建立 TicketLineResult。
- **TicketDraw**：以 TicketId + DrawId 做唯一鍵避免重複參與期數。

## 待改進事項
- 若未來需支援多種 RNG 或更多獎項規則，應擴充 proof 與規則解析策略，並保留既有驗證格式以維持向後相容。
