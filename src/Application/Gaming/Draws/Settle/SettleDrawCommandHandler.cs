using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Gaming;
using Application.Abstractions.Messaging;
using Domain.Gaming.Catalog;
using Domain.Gaming.Draws;
using Domain.Gaming.Repositories;
using Domain.Gaming.Rules;
using Domain.Gaming.Shared;
using Domain.Gaming.Tickets;
using SharedKernel;

namespace Application.Gaming.Draws.Settle;

/// <summary>
/// 結算開獎結果：
/// - 讀取指定 Draw 的中獎號碼（winning numbers）
/// - 找出參與該期的有效 Ticket（實際上是透過 TicketDraw 參與關聯）
/// - 對每張有效票券的投注號碼套用玩法規則（PlayRule）
/// - 命中則產生 TicketLineResult（每一注/每一行的得獎結果）
/// - 最後把 TicketDraw 標記為已結算（Settled）
/// </summary>
/// <remarks>
/// 這個 Handler 位於 Application 層：
/// - 負責 orchestration：Repository + RuleRegistry + 時間/權限檢查
/// - 避免 Domain 直接依賴 DB 或外部資料來源
/// </remarks>
internal sealed class SettleDrawCommandHandler(
    IDrawRepository drawRepository,
    ITicketRepository ticketRepository,
    ITicketDrawRepository ticketDrawRepository,
    ITicketLineResultRepository ticketLineResultRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ITenantContext tenantContext,
    IEntitlementChecker entitlementChecker) : ICommandHandler<SettleDrawCommand>
{
    public async Task<Result> Handle(SettleDrawCommand request, CancellationToken cancellationToken)
    {
        // =========================================================
        // 1) 讀取 Draw（期數）並確認存在
        // =========================================================
        Draw? draw = await drawRepository.GetByIdAsync(
            tenantContext.TenantId,
            request.DrawId,
            cancellationToken);

        if (draw is null)
        {
            // 找不到期數 => 結算無從談起
            return Result.Failure(GamingErrors.DrawNotFound);
        }

        // =========================================================
        // 2) 權限/啟用檢查：租戶是否允許該遊戲 GameCode
        // =========================================================
        Result entitlementResult = await entitlementChecker.EnsureGameEnabledAsync(
            tenantContext.TenantId,
            draw.GameCode,
            cancellationToken);

        if (entitlementResult.IsFailure)
        {
            // 租戶未啟用該遊戲 => 不允許結算
            return Result.Failure(entitlementResult.Error);
        }

        // =========================================================
        // 3) 時間與狀態檢查：必須已「開獎」才允許結算
        // =========================================================
        DateTime now = dateTimeProvider.UtcNow;

        // GetEffectiveStatus(now) 代表狀態可能會受時間影響（例如封盤/可售等）
        DrawStatus status = draw.GetEffectiveStatus(now);

        if (status != DrawStatus.Drawn && status != DrawStatus.Settled)
        {
            // 尚未開獎/尚未到可開獎狀態 => 不允許結算
            // 注意：錯誤名稱 DrawNotSettled 其實語意較像 "DrawNotDrawnYet"
            return Result.Failure(GamingErrors.DrawNotSettled);
        }

        // =========================================================
        // 4) 取得開獎號碼（winning numbers）
        // =========================================================
        LotteryNumbers? winningNumbers = draw.ParseWinningNumbers();
        if (winningNumbers is null)
        {
            // Draw 狀態是 Drawn，但 winning numbers 解析不到
            // 通常代表資料不完整/被污染 => 視為無法結算
            return Result.Failure(GamingErrors.DrawNotSettled);
        }

        // =========================================================
        // 5) 建立玩法規則註冊表（Rule Registry）
        //    - 透過 GameCode + PlayTypeCode 取得對應規則 IPlayRule
        // =========================================================
        PlayRuleRegistry registry = PlayRuleRegistry.CreateDefault();

        // =========================================================
        // 6) 結算前檢查：獎池（Prize Pool）是否完整
        //    - 例如每個玩法/每個 tier 都有設定 payout 或獎項
        // =========================================================
        Result prizePoolResult = draw.EnsurePrizePoolCompleteForSettlement(registry);
        if (prizePoolResult.IsFailure)
        {
            // 獎池配置不完整 => 不允許結算（避免算出 tier 卻找不到 payout）
            return Result.Failure(prizePoolResult.Error);
        }

        // =========================================================
        // 7) 找出所有「參與該 Draw」的票券（TicketDraw）
        //    - TicketDraw 是票券參與期數的關聯/狀態表
        //    - 只取 Active（排除 Cancelled / Invalid / ...）
        // =========================================================
        IReadOnlyCollection<TicketDraw> ticketDraws = await ticketDrawRepository.GetByDrawIdAsync(
            tenantContext.TenantId,
            draw.Id,
            TicketDrawParticipationStatus.Active,
            cancellationToken);

        // 沒有人參與這期 => 直接成功返回
        if (ticketDraws.Count == 0)
        {
            draw.MarkSettled(now);
            drawRepository.Update(draw);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }

        // =========================================================
        // 8) 批次載入 Ticket 本體（避免 N+1）
        // =========================================================
        IReadOnlyCollection<Guid> ticketIds = ticketDraws
            .Select(item => item.TicketId)
            .Distinct()
            .ToList();

        IReadOnlyCollection<Ticket> tickets = await ticketRepository.GetByIdsAsync(
            tenantContext.TenantId,
            ticketIds,
            cancellationToken);

        // 用 Dictionary 快速查詢 Ticket 是否存在
        Dictionary<Guid, Ticket> ticketMap = tickets.ToDictionary(ticket => ticket.Id, ticket => ticket);

        // =========================================================
        // 9) 取得既有結算結果（避免重複寫入）
        //    - 用 (TicketId, LineIndex) 當去重 Key
        // =========================================================
        IReadOnlyCollection<TicketLineResult> existingResults =
            await ticketLineResultRepository.GetByDrawAndTicketsAsync(
                tenantContext.TenantId,
                draw.Id,
                ticketIds,
                cancellationToken);

        HashSet<(Guid TicketId, int LineIndex)> existingKeys = existingResults
            .Select(result => (result.TicketId, result.LineIndex))
            .ToHashSet();

        // =========================================================
        // 10) 逐筆處理每一張參與記錄 TicketDraw
        // =========================================================
        foreach (TicketDraw ticketDraw in ticketDraws)
        {
            // -----------------------------------------------------
            // 10.1) Ticket 不存在：標記該 TicketDraw 為 Invalid
            // -----------------------------------------------------
            if (!ticketMap.TryGetValue(ticketDraw.TicketId, out Ticket? ticket))
            {
                ticketDraw.MarkInvalid(now);
                ticketDrawRepository.Update(ticketDraw);
                continue;
            }

            // -----------------------------------------------------
            // 10.2) Ticket 尚未提交（Submitted）：
            //       代表沒有有效投注內容 => 直接 Invalid
            // -----------------------------------------------------
            if (ticket.SubmissionStatus != TicketSubmissionStatus.Submitted)
            {
                ticketDraw.MarkInvalid(now);
                ticketDrawRepository.Update(ticketDraw);
                continue;
            }

            // -----------------------------------------------------
            // 10.3) 限制：目前只支援單注（Lines.Count <= 1）
            //       若有多注，直接 Fail（不是把該票 invalid）
            //       => 這是強限制：一張異常票會讓整期結算失敗
            // -----------------------------------------------------
            if (ticket.Lines.Count > 1)
            {
                return Result.Failure(GamingErrors.TicketLinesExceedLimit);
            }

            // 取出唯一的一行投注（可能為 null）
            TicketLine? line = ticket.Lines.FirstOrDefault();
            if (line is null)
            {
                ticketDraw.MarkInvalid(now);
                ticketDrawRepository.Update(ticketDraw);
                continue;
            }

            // -----------------------------------------------------
            // 10.4) 解析該行投注號碼
            //       - 解析失敗 => Invalid
            // -----------------------------------------------------
            LotteryNumbers? lineNumbers = line.ParseNumbers();
            if (lineNumbers is null)
            {
                ticketDraw.MarkInvalid(now);
                ticketDrawRepository.Update(ticketDraw);
                continue;
            }

            // -----------------------------------------------------
            // 10.5) 決定玩法 PlayTypeCode
            //       - line.PlayTypeCode 優先
            //       - 沒有才 fallback 到 ticket.PlayTypeCode（舊設計相容）
            //       - 最終沒玩法 => Invalid
            // -----------------------------------------------------
            PlayTypeCode? playTypeCode = line.PlayTypeCode ?? ticket.PlayTypeCode;
            if (!playTypeCode.HasValue)
            {
                ticketDraw.MarkInvalid(now);
                ticketDrawRepository.Update(ticketDraw);
                continue;
            }

            // -----------------------------------------------------
            // 10.6) 取得規則並評估是否命中
            //       Evaluate 回傳 PrizeTier?（例如：頭獎/貳獎/參獎...）
            //       - null 代表未中獎
            // -----------------------------------------------------
            IPlayRule rule = registry.GetRule(draw.GameCode, playTypeCode.Value);
            PrizeTier? tier = rule.Evaluate(lineNumbers, winningNumbers);

            // -----------------------------------------------------
            // 10.7) 若中獎且尚未寫入過結果 => 建立 TicketLineResult
            //       去重用 existingKeys 避免重跑重複 insert
            // -----------------------------------------------------
            if (tier is not null && !existingKeys.Contains((ticket.Id, line.LineIndex)))
            {
                // 依玩法 + tier 從 Draw 的獎池配置中找 payout
                PrizeOption? option = draw.FindPrizeOption(playTypeCode.Value, tier.Value);
                if (option is null)
                {
                    // 理論上前面 EnsurePrizePoolCompleteForSettlement 已經擋掉
                    // 但仍做保護，避免寫出沒有 payout 的結果
                    return Result.Failure(GamingErrors.PrizePoolNotConfigured);
                }

                // 建立得獎結果（對應到某 Ticket 的某一行 LineIndex）
                TicketLineResult result = TicketLineResult.Create(
                    tenantContext.TenantId,
                    ticket.Id,
                    draw.Id,
                    line.LineIndex,
                    tier.Value,
                    option.PayoutAmount,
                    now);

                ticketLineResultRepository.Insert(result);

                // 更新去重 Key，確保同一輪迴圈後續不會重複建立
                existingKeys.Add((ticket.Id, line.LineIndex));
            }

            // -----------------------------------------------------
            // 10.8) 不管中獎與否，只要能走到這裡都視為已結算
            //       => MarkSettled
            // -----------------------------------------------------
            ticketDraw.MarkSettled(now);
            ticketDrawRepository.Update(ticketDraw);
        }

        // =========================================================
        // 11) 一次性提交交易（Unit of Work）
        // =========================================================
        draw.MarkSettled(now);
        drawRepository.Update(draw);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
