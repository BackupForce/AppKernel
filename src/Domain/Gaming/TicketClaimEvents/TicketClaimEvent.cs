using Domain.Gaming.Shared;
using SharedKernel;

namespace Domain.Gaming.TicketClaimEvents;

/// <summary>
/// 票券領取活動（Ticket Claim Event）
///
/// 用來描述「一段時間內，會員可領取某種票券」的活動規則與狀態。
/// 這個 Aggregate 主要負責：
/// - 活動基本資訊（名稱、時間窗）
/// - 狀態機（Draft / Active / Disabled / Ended / SoldOut）
/// - 配額（總配額、已領取、每人上限）
/// - 適用範圍（ScopeType + ScopeId）
/// - 綁定票券模板（TicketTemplateId）
///
/// 注意：
/// - 這個 Aggregate 自身不追蹤「每個會員領了多少」；那通常會在 Application 或另一個紀錄表處理。
/// - IncreaseClaimed 只管「總已領取數」增加與售罄判斷。
/// </summary>
public sealed class TicketClaimEvent : Entity
{
    /// <summary>
    /// 完整建構子（私有）：
    /// - 強制透過 Create 工廠方法建立，確保進入點都經過驗證規則。
    /// - 同時也方便 EF Core 用私有建構子 materialize（搭配設定）。
    /// </summary>
    private TicketClaimEvent(
        Guid id,
        Guid tenantId,
        string name,
        DateTime startsAtUtc,
        DateTime endsAtUtc,
        TicketClaimEventStatus status,
        int totalQuota,
        int totalClaimed,
        int perMemberQuota,
        TicketClaimEventScopeType scopeType,
        Guid scopeId,
        Guid? ticketTemplateId,
        DateTime createdAtUtc,
        DateTime updatedAtUtc) : base(id)
    {
        TenantId = tenantId;
        Name = name;
        StartsAtUtc = startsAtUtc;
        EndsAtUtc = endsAtUtc;
        Status = status;
        TotalQuota = totalQuota;
        TotalClaimed = totalClaimed;
        PerMemberQuota = perMemberQuota;
        ScopeType = scopeType;
        ScopeId = scopeId;
        TicketTemplateId = ticketTemplateId;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
    }

    /// <summary>
    /// EF Core 需要的 parameterless ctor。
    /// 讓 ORM 可以先 new，再逐一 set 私有 setter 的欄位（或透過反射）。
    /// </summary>
    private TicketClaimEvent()
    {
    }

    /// <summary>
    /// 租戶 Id（多租戶隔離）。
    /// </summary>
    public Guid TenantId { get; private set; }

    /// <summary>
    /// 活動名稱（後台顯示/管理用）。
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// 活動開始時間（UTC）。
    /// </summary>
    public DateTime StartsAtUtc { get; private set; }

    /// <summary>
    /// 活動結束時間（UTC）。
    /// </summary>
    public DateTime EndsAtUtc { get; private set; }

    /// <summary>
    /// 活動狀態（狀態機核心）。
    /// - Draft：草稿（尚未開始、不可領）
    /// - Active：啟用中（可領，仍需符合時間窗）
    /// - Disabled：停用（不可領，但不代表時間到）
    /// - Ended：結束（不可再變更/不可再領）
    /// - SoldOut：售罄（總配額已用完）
    /// </summary>
    public TicketClaimEventStatus Status { get; private set; }

    /// <summary>
    /// 總配額：活動最多可被領取的票數上限。
    /// </summary>
    public int TotalQuota { get; private set; }

    /// <summary>
    /// 已領取數：所有會員累計領走的票數。
    /// </summary>
    public int TotalClaimed { get; private set; }

    /// <summary>
    /// 每位會員可領取上限。
    /// 注意：這裡只是「規格」，實際每位會員已領多少需另行記錄/檢核。
    /// </summary>
    public int PerMemberQuota { get; private set; }

    /// <summary>
    /// 活動範圍類型：用來描述此活動套用到哪個維度。
    /// 例如：Campaign / Draw / CampaignDraw / 全站...（依你 enum 定義）
    /// </summary>
    public TicketClaimEventScopeType ScopeType { get; private set; }

    /// <summary>
    /// 活動範圍 Id：對應 ScopeType 的實體 Id。
    /// 例：ScopeType=Draw => ScopeId=DrawId
    /// </summary>
    public Guid ScopeId { get; private set; }

    /// <summary>
    /// 綁定要發放的票券模板 Id。
    /// Nullable：允許某些活動先不綁模板（或未來擴充多模板）。
    /// </summary>
    public Guid? TicketTemplateId { get; private set; }

    /// <summary>
    /// 建立時間（UTC）。
    /// </summary>
    public DateTime CreatedAtUtc { get; private set; }

    /// <summary>
    /// 更新時間（UTC）。
    /// </summary>
    public DateTime UpdatedAtUtc { get; private set; }

    /// <summary>
    /// 工廠方法：建立活動（回傳 Result<T>，把 validation error 透過 domain error 傳出）。
    ///
    /// 主要規則：
    /// - tenant 必填
    /// - name 必填、長度 <= 128
    /// - 時間窗 startsAt < endsAt
    /// - 配額 totalQuota、perMemberQuota >= 1
    /// - scopeId 必填（不可為 Guid.Empty）
    ///
    /// 初始狀態：
    /// - Draft
    /// - TotalClaimed = 0
    /// - createdAt/updatedAt 都等於 utcNow
    /// </summary>
    public static Result<TicketClaimEvent> Create(
        Guid tenantId,
        string name,
        DateTime startsAtUtc,
        DateTime endsAtUtc,
        int totalQuota,
        int perMemberQuota,
        TicketClaimEventScopeType scopeType,
        Guid scopeId,
        Guid? ticketTemplateId,
        DateTime utcNow)
    {
        if (tenantId == Guid.Empty)
        {
            return Result.Failure<TicketClaimEvent>(GamingErrors.TicketClaimEventTenantRequired);
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<TicketClaimEvent>(GamingErrors.TicketClaimEventNameRequired);
        }

        if (name.Trim().Length > 128)
        {
            return Result.Failure<TicketClaimEvent>(GamingErrors.TicketClaimEventNameTooLong);
        }

        if (startsAtUtc >= endsAtUtc)
        {
            return Result.Failure<TicketClaimEvent>(GamingErrors.TicketClaimEventInvalidTimeWindow);
        }

        if (totalQuota < 1)
        {
            return Result.Failure<TicketClaimEvent>(GamingErrors.TicketClaimEventInvalidQuota);
        }

        if (perMemberQuota < 1)
        {
            return Result.Failure<TicketClaimEvent>(GamingErrors.TicketClaimEventInvalidQuota);
        }

        if (scopeId == Guid.Empty)
        {
            return Result.Failure<TicketClaimEvent>(GamingErrors.TicketClaimEventScopeRequired);
        }

        return new TicketClaimEvent(
            Guid.NewGuid(),
            tenantId,
            name.Trim(),
            startsAtUtc,
            endsAtUtc,
            TicketClaimEventStatus.Draft,
            totalQuota,
            0,
            perMemberQuota,
            scopeType,
            scopeId,
            ticketTemplateId,
            utcNow,
            utcNow);
    }

    /// <summary>
    /// 更新活動資訊（後台編輯用）。
    ///
    /// 不可編輯狀態：
    /// - Ended：已結束
    /// - SoldOut：已售罄
    ///
    /// 驗證規則同 Create（名稱、時間窗、配額、scope）。
    ///
    /// 特殊規則：當狀態為 Active 時，只允許修改：
    /// - Name
    /// - EndsAtUtc
    /// （避免活動啟用後變更開始時間/配額/範圍/模板，造成公平性或系統一致性問題）
    ///
    /// 更新後額外檢核：
    /// - TotalClaimed 不能大於 TotalQuota（否則表示你把總配額改小到低於已領取數）
    /// </summary>
    public Result UpdateInfo(
        string name,
        DateTime startsAtUtc,
        DateTime endsAtUtc,
        int totalQuota,
        int perMemberQuota,
        TicketClaimEventScopeType scopeType,
        Guid scopeId,
        Guid? ticketTemplateId,
        DateTime utcNow)
    {
        if (Status is TicketClaimEventStatus.Ended or TicketClaimEventStatus.SoldOut)
        {
            return Result.Failure(GamingErrors.TicketClaimEventNotEditable);
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure(GamingErrors.TicketClaimEventNameRequired);
        }

        if (name.Trim().Length > 128)
        {
            return Result.Failure(GamingErrors.TicketClaimEventNameTooLong);
        }

        if (startsAtUtc >= endsAtUtc)
        {
            return Result.Failure(GamingErrors.TicketClaimEventInvalidTimeWindow);
        }

        if (totalQuota < 1 || perMemberQuota < 1)
        {
            return Result.Failure(GamingErrors.TicketClaimEventInvalidQuota);
        }

        if (scopeId == Guid.Empty)
        {
            return Result.Failure(GamingErrors.TicketClaimEventScopeRequired);
        }

        // Active 狀態下：只允許改名稱與結束時間
        if (Status == TicketClaimEventStatus.Active)
        {
            Name = name.Trim();
            EndsAtUtc = endsAtUtc;
        }
        else
        {
            // 非 Active：可完整修改活動設定
            Name = name.Trim();
            StartsAtUtc = startsAtUtc;
            EndsAtUtc = endsAtUtc;
            TotalQuota = totalQuota;
            PerMemberQuota = perMemberQuota;
            ScopeType = scopeType;
            ScopeId = scopeId;
            TicketTemplateId = ticketTemplateId;
        }

        // 確保「已領取數」不會超過你改完的總配額
        if (TotalClaimed > TotalQuota)
        {
            return Result.Failure(GamingErrors.TicketClaimEventInvalidQuota);
        }

        UpdatedAtUtc = utcNow;
        return Result.Success();
    }

    /// <summary>
    /// 啟用活動。
    ///
    /// 規則：
    /// - 若已 Active：回傳 AlreadyActive
    /// - 若已 Ended：回傳 AlreadyEnded（結束不可逆）
    /// - 若已領取數 >= 總配額：直接標 SoldOut，並回 SoldOut
    /// - 否則狀態變 Active
    ///
    /// 注意：Activate 不會檢查「現在是否已到 StartsAtUtc」；
    /// 也就是：可以先把狀態設為 Active，但實際能不能領還要走 EnsureCanClaim（含時間窗）。
    /// </summary>
    public Result Activate(DateTime utcNow)
    {
        if (Status is TicketClaimEventStatus.Active)
        {
            return Result.Failure(GamingErrors.TicketClaimEventAlreadyActive);
        }

        if (Status is TicketClaimEventStatus.Ended)
        {
            return Result.Failure(GamingErrors.TicketClaimEventAlreadyEnded);
        }

        if (TotalClaimed >= TotalQuota)
        {
            Status = TicketClaimEventStatus.SoldOut;
            UpdatedAtUtc = utcNow;
            return Result.Failure(GamingErrors.TicketClaimEventSoldOut);
        }

        Status = TicketClaimEventStatus.Active;
        UpdatedAtUtc = utcNow;
        return Result.Success();
    }

    /// <summary>
    /// 停用活動（通常用於後台暫停）。
    ///
    /// 規則：
    /// - Ended 不可再變更（結束不可逆）
    /// - 其餘狀態都可切到 Disabled
    /// </summary>
    public Result Disable(DateTime utcNow)
    {
        if (Status is TicketClaimEventStatus.Ended)
        {
            return Result.Failure(GamingErrors.TicketClaimEventAlreadyEnded);
        }

        Status = TicketClaimEventStatus.Disabled;
        UpdatedAtUtc = utcNow;
        return Result.Success();
    }

    /// <summary>
    /// 結束活動（人工結束）。
    ///
    /// 規則：
    /// - Ended 不可重複結束
    /// - 結束後：
    ///   - Status = Ended
    ///   - EndsAtUtc = utcNow（強制將時間窗結束時間改成現在）
    ///   - UpdatedAtUtc = utcNow
    /// </summary>
    public Result End(DateTime utcNow)
    {
        if (Status is TicketClaimEventStatus.Ended)
        {
            return Result.Failure(GamingErrors.TicketClaimEventAlreadyEnded);
        }

        Status = TicketClaimEventStatus.Ended;
        EndsAtUtc = utcNow;
        UpdatedAtUtc = utcNow;
        return Result.Success();
    }

    /// <summary>
    /// 檢查目前是否允許「進行領取」。
    ///
    /// 這個方法的目的通常是：在 Application 執行 Claim 流程前先做 Gatekeeping。
    ///
    /// 規則檢核順序（依序回傳對應錯誤）：
    /// - Disabled：停用中
    /// - Draft：未啟用
    /// - Ended：已結束
    /// - SoldOut：已售罄
    /// - 未到開始時間：NotStarted
    /// - 已超過結束時間：Ended
    /// - 已領取數 >= 總配額：會「順便」把狀態標成 SoldOut，並回 SoldOut
    ///
    /// 注意：這裡會有「帶狀態副作用」：當發現已達配額會改 Status/UpdatedAt。
    /// 這是合理的：把售罄狀態固化在 Aggregate 上，避免之後每次都重新判斷。
    /// </summary>
    public Result EnsureCanClaim(DateTime utcNow)
    {
        if (Status == TicketClaimEventStatus.Disabled)
        {
            return Result.Failure(GamingErrors.TicketClaimEventDisabled);
        }

        if (Status == TicketClaimEventStatus.Draft)
        {
            return Result.Failure(GamingErrors.TicketClaimEventNotActive);
        }

        if (Status == TicketClaimEventStatus.Ended)
        {
            return Result.Failure(GamingErrors.TicketClaimEventEnded);
        }

        if (Status == TicketClaimEventStatus.SoldOut)
        {
            return Result.Failure(GamingErrors.TicketClaimEventSoldOut);
        }

        if (utcNow < StartsAtUtc)
        {
            return Result.Failure(GamingErrors.TicketClaimEventNotStarted);
        }

        if (utcNow >= EndsAtUtc)
        {
            return Result.Failure(GamingErrors.TicketClaimEventEnded);
        }

        if (TotalClaimed >= TotalQuota)
        {
            Status = TicketClaimEventStatus.SoldOut;
            UpdatedAtUtc = utcNow;
            return Result.Failure(GamingErrors.TicketClaimEventSoldOut);
        }

        return Result.Success();
    }

    /// <summary>
    /// 增加已領取數（通常在「成功發票/成功占用配額」之後呼叫）。
    ///
    /// 注意：
    /// - 這裡只檢查「總配額」；不檢查「單會員上限」。
    ///   單會員上限需靠外部（Application）讀取該會員領取紀錄來判斷。
    ///
    /// 規則：
    /// - quantity 必須 > 0
    /// - 若加總後超過總配額：標 SoldOut，回 SoldOut
    /// - 若加總後達到總配額：標 SoldOut
    /// - 成功則更新 UpdatedAtUtc
    /// </summary>
    public Result IncreaseClaimed(int quantity, DateTime utcNow)
    {
        if (quantity <= 0)
        {
            return Result.Failure(GamingErrors.TicketClaimEventInvalidQuota);
        }

        if (TotalClaimed + quantity > TotalQuota)
        {
            Status = TicketClaimEventStatus.SoldOut;
            return Result.Failure(GamingErrors.TicketClaimEventSoldOut);
        }

        TotalClaimed += quantity;
        if (TotalClaimed >= TotalQuota)
        {
            Status = TicketClaimEventStatus.SoldOut;
        }

        UpdatedAtUtc = utcNow;
        return Result.Success();
    }
}
