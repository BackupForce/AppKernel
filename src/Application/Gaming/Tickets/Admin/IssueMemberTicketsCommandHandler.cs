using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Time;
using Domain.Gaming.Catalog;
using Domain.Gaming.Draws;
using Domain.Gaming.Repositories;
using Domain.Gaming.Shared;
using Domain.Gaming.Tickets;
using Domain.Members;
using SharedKernel;

namespace Application.Gaming.Tickets.Admin;

// 後台：發放票券給指定會員的 Command Handler
internal sealed class IssueMemberTicketsCommandHandler(
    IDrawRepository drawRepository,                     // 讀取 Draw（期數/場次）的 Repository
    ITicketRepository ticketRepository,                 // Ticket 寫入 Repository
    ITicketDrawRepository ticketDrawRepository,         // Ticket <-> Draw 對應表 Repository
    ITicketIdempotencyRepository ticketIdempotencyRepository, // 冪等性記錄 Repository（避免重複發票）
    IMemberRepository memberRepository,                 // 讀取會員資料 Repository
    IUnitOfWork unitOfWork,                             // UoW：一次性提交交易
    IDateTimeProvider dateTimeProvider,                 // 時間提供者（統一使用 UTC Now）
    ITenantContext tenantContext,                       // 多租戶上下文（TenantId）
    IUserContext userContext)                           // 使用者上下文（後台操作人員 UserId）
    : ICommandHandler<IssueMemberTicketsCommand, IssueMemberTicketsResult>
{
    // 冪等性：用來標識此操作類型（寫入 TicketIdempotencyRecord.Operation）
    private const string Operation = "issue_ticket";

    // JSON 序列化設定：Web defaults（camelCase、常用 Web 行為）
    // 目的：把回傳結果序列化後存進冪等記錄，未來可直接回放
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<Result<IssueMemberTicketsResult>> Handle(
        IssueMemberTicketsCommand request,
        CancellationToken cancellationToken)
    {
        // 1) 處理冪等 Key：去掉空白，空白視為 null
        string? idempotencyKey = NormalizeKey(request.IdempotencyKey);

        // 2) 計算 request hash：用來偵測「同一個 idempotencyKey 但 request 內容不同」的衝突
        string requestHash = ComputeIssueHash(request);

        // 3) 若有提供冪等 key，先查是否已存在紀錄
        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            TicketIdempotencyRecord? existing = await ticketIdempotencyRepository.GetByKeyAsync(
                tenantContext.TenantId,
                idempotencyKey,
                Operation,
                cancellationToken);

            // 3-1) 若存在，代表這次操作可能已經成功過（或至少已經被寫入冪等表）
            if (existing is not null)
            {
                // 若同 key 但 request hash 不同 -> 視為衝突（避免錯用同一 key 重放不同請求）
                if (!string.Equals(existing.RequestHash, requestHash, StringComparison.Ordinal))
                {
                    return Result.Failure<IssueMemberTicketsResult>(GamingErrors.TicketIdempotencyKeyConflict);
                }

                // 3-2) 回放先前結果：直接反序列化既有 ResponsePayload
                IssueMemberTicketsResult? cached =
                    JsonSerializer.Deserialize<IssueMemberTicketsResult>(existing.ResponsePayload, JsonOptions);

                // payload 無法反序列化 -> 視為資料異常
                if (cached is null)
                {
                    return Result.Failure<IssueMemberTicketsResult>(GamingErrors.TicketIdempotencyPayloadInvalid);
                }

                // 直接回傳 cached 結果（達成冪等）
                return cached;
            }
        }

        // 4) 驗證並建立 GameCode Value Object
        Result<GameCode> gameCodeResult = GameCode.Create(request.GameCode);
        if (gameCodeResult.IsFailure)
        {
            return Result.Failure<IssueMemberTicketsResult>(gameCodeResult.Error);
        }

        // 5) 取 UTC Now（統一時間來源）
        DateTime now = dateTimeProvider.UtcNow;
        GameCode gameCode = gameCodeResult.Value;

        // 6) 讀取 Draw：用 request.DrawId
        Draw? draw = await drawRepository.GetByIdAsync(tenantContext.TenantId, request.DrawId, cancellationToken);
        if (draw is null)
        {
            return Result.Failure<IssueMemberTicketsResult>(GamingErrors.DrawNotFound);
        }

        // 7) 再次確認 Draw 的 GameCode 與 request.GameCode 一致
        //    （避免用錯 drawId / gameCode 的組合）
        if (draw.GameCode != gameCode)
        {
            return Result.Failure<IssueMemberTicketsResult>(GamingErrors.DrawNotFound);
        }

        // 8) 若 Draw 已「實質封盤/關閉」-> 不允許發票（依你的 domain 規則）
        if (draw.IsEffectivelyClosed(now))
        {
            return Result.Failure<IssueMemberTicketsResult>(GamingErrors.DrawNotOpen);
        }

        // 9) 讀取會員
        Member? member = await memberRepository.GetByIdAsync(tenantContext.TenantId, request.MemberId, cancellationToken);
        if (member is null)
        {
            return Result.Failure<IssueMemberTicketsResult>(GamingErrors.MemberNotFound);
        }

        // 10) 數量限制：1~100
        if (request.Quantity <= 0 || request.Quantity > 100)
        {
            return Result.Failure<IssueMemberTicketsResult>(GamingErrors.TicketIssueQuantityInvalid);
        }

        // 11) 建立 tickets 與 ticketDraws（先在記憶體組好，再統一 Insert）
        List<Ticket> tickets = new();
        List<TicketDraw> ticketDraws = new();

        for (int index = 0; index < request.Quantity; index++)
        {
            // 建立 Ticket：
            // - TenantId / GameCode / MemberId
            // - draw.Id 綁定該期
            // - 發放者類型 Backoffice，發放者 userContext.UserId
            // - reason / note 由 request 帶入（可作審計）
            // - now 作為 issued/created 等時間（依你的 Ticket.Create 定義）
            Ticket ticket = Ticket.Create(
                tenantContext.TenantId,
                draw.GameCode,
                member.Id,
                null,                       // 可能是 MemberNo / ExternalRef / ...（依你的 domain 參數）
                null,                       // 可能是 PlayType / Template / ...（依你的 domain 參數）
                draw.Id,                    // 綁定 Draw
                null,                       // 可能是 SubmittedAt / CancelledAt / ...（依你的 domain 參數）
                null,                       // 可能是 SubmittedBy / CancelledBy / ...（依你的 domain 參數）
                now,                        // issuedAtUtc
                IssuedByType.Backoffice,    // 後台發放
                userContext.UserId,         // 後台操作人員
                request.Reason,             // 發放原因
                request.Note,               // 補充備註
                now);                       // createdAt / updatedAt（依你的 domain 參數）

            tickets.Add(ticket);

            // 建立 TicketDraw 對應資料（多期對獎/查詢用）
            ticketDraws.Add(TicketDraw.Create(tenantContext.TenantId, ticket.Id, draw.Id, now));
        }

        // 12) 寫入 tickets
        foreach (Ticket ticket in tickets)
        {
            ticketRepository.Insert(ticket);
        }

        // 13) 寫入 ticketDraws
        foreach (TicketDraw ticketDraw in ticketDraws)
        {
            ticketDrawRepository.Insert(ticketDraw);
        }

        // 14) 組合回傳結果 DTO（後台用狀態字串 + 關鍵欄位）
        IssueMemberTicketsResult result = new IssueMemberTicketsResult(
            tickets.Select(ticket => new IssuedTicketDto(
                ticket.Id,
                TicketStatusMapper.ToAdminStatus(ticket.SubmissionStatus), // 將 domain enum 映射成後台顯示狀態
                ticket.IssuedAtUtc,
                ticket.DrawId ?? draw.Id,                                 // 理論上應該有 DrawId；保險用 fallback
                ticket.GameCode.Value,
                ticket.IssuedByUserId ?? Guid.Empty,                      // 理論上會有 userId；保險用 Empty
                ticket.IssuedReason,
                ticket.IssuedNote))
            .ToList());

        // 15) 若有冪等 key：將 result 序列化後，寫入 TicketIdempotencyRecord
        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            string payload = JsonSerializer.Serialize(result, JsonOptions);

            TicketIdempotencyRecord record = TicketIdempotencyRecord.Create(
                tenantContext.TenantId,
                idempotencyKey,
                Operation,
                requestHash,
                payload,
                now);

            ticketIdempotencyRepository.Insert(record);
        }

        // 16) 交易提交：tickets、ticketDraws、（可能的）idempotency record 一次保存
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // 17) 回傳結果
        return result;
    }

    // 冪等 key 正規化：空白視為 null，否則 Trim
    private static string? NormalizeKey(string? key)
    {
        return string.IsNullOrWhiteSpace(key) ? null : key.Trim();
    }

    // 計算發票請求的 hash（SHA256）
    // 用於：
    // - 同一 idempotencyKey 重放時，確保 request 內容一致
    // - 避免同 key 被誤用在不同 request
    private static string ComputeIssueHash(IssueMemberTicketsCommand request)
    {
        // 注意：這裡以字串拼接形成 raw，包含 MemberId/GameCode/DrawId/Quantity/Reason/Note
        // 若 Reason/Note 可能有 | 或格式差異，通常也沒問題（只要一致即可）；但要注意 null 表現一致性
        string raw = $"{request.MemberId:N}|{request.GameCode}|{request.DrawId:N}|{request.Quantity}|{request.Reason}|{request.Note}";

        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(raw));

        // 轉成 HEX 字串（大寫）
        return Convert.ToHexString(hash);
    }
}

// 後台顯示用：將 TicketSubmissionStatus 映射為字串
internal static class TicketStatusMapper
{
    public static string ToAdminStatus(TicketSubmissionStatus status)
    {
        return status switch
        {
            // 未提交：後台視為「已發放」
            TicketSubmissionStatus.NotSubmitted => "Issued",

            // 已提交：代表會員已填寫並送出
            TicketSubmissionStatus.Submitted => "Submitted",

            // 已取消：後台顯示為作廢
            TicketSubmissionStatus.Cancelled => "Voided",

            // 已過期：封盤未提交等情境
            TicketSubmissionStatus.Expired => "Expired",

            // 其他未知 enum 值：退回 ToString（避免爆炸）
            _ => status.ToString()
        };
    }
}
