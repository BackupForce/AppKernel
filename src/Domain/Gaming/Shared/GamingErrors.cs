using SharedKernel;

namespace Domain.Gaming.Shared;

/// <summary>
/// Gaming 模組錯誤碼定義，供應用層回傳一致錯誤資訊。
/// </summary>
public static class GamingErrors
{
    public static readonly Error LotteryNumbersRequired = Error.Validation(
        "Gaming.LotteryNumbersRequired",
        "號碼不可為空白。");

    public static readonly Error LotteryNumbersCountInvalid = Error.Validation(
        "Gaming.LotteryNumbersCountInvalid",
        "號碼數量必須為 5 個。");

    public static readonly Error LotteryNumbersOutOfRange = Error.Validation(
        "Gaming.LotteryNumbersOutOfRange",
        "號碼必須介於 1 到 39。");

    public static readonly Error LotteryNumbersDuplicated = Error.Validation(
        "Gaming.LotteryNumbersDuplicated",
        "號碼不可重複。");

    public static readonly Error LotteryNumbersFormatInvalid = Error.Validation(
        "Gaming.LotteryNumbersFormatInvalid",
        "號碼格式不正確。");

    public static readonly Error DrawNotFound = Error.NotFound(
        "Gaming.DrawNotFound",
        "找不到期數。");

    public static readonly Error DrawTemplateNotFound = Error.NotFound(
        "Gaming.DrawTemplateNotFound",
        "找不到期數模板。");

    public static readonly Error GameCodeRequired = Error.Validation(
        "Gaming.GameCodeRequired",
        "遊戲代碼不可為空白。");

    public static readonly Error DrawTemplateNameRequired = Error.Validation(
        "Gaming.DrawTemplateNameRequired",
        "期數模板名稱不可為空白。");

    public static readonly Error DrawTemplateNameTooLong = Error.Validation(
        "Gaming.DrawTemplateNameTooLong",
        "期數模板名稱不可超過 64 字元。");

    public static readonly Error DrawTemplateNameDuplicated = Error.Validation(
        "Gaming.DrawTemplateNameDuplicated",
        "期數模板名稱已存在。");

    public static readonly Error DrawTemplateTenantRequired = Error.Validation(
        "Gaming.DrawTemplateTenantRequired",
        "期數模板租戶不可為空。");

    public static readonly Error DrawTemplateInactive = Error.Validation(
        "Gaming.DrawTemplateInactive",
        "期數模板已停用。");

    public static readonly Error DrawTemplateLocked = Error.Validation(
        "Gaming.DrawTemplateLocked",
        "期數模板已鎖定，無法刪除既有設定。");

    public static readonly Error DrawTemplatePlayTypeDuplicated = Error.Validation(
        "Gaming.DrawTemplatePlayTypeDuplicated",
        "期數模板玩法重複。");

    public static readonly Error DrawTemplatePlayTypeNotFound = Error.NotFound(
        "Gaming.DrawTemplatePlayTypeNotFound",
        "期數模板玩法不存在。");

    public static readonly Error DrawTemplatePrizeTierNotFound = Error.NotFound(
        "Gaming.DrawTemplatePrizeTierNotFound",
        "期數模板獎級不存在。");

    public static readonly Error DrawTemplateAllowedTicketTemplateDuplicated = Error.Validation(
        "Gaming.DrawTemplateAllowedTicketTemplateDuplicated",
        "期數模板允許票種重複。");

    public static readonly Error DrawTemplateAllowedTicketTemplateNotFound = Error.NotFound(
        "Gaming.DrawTemplateAllowedTicketTemplateNotFound",
        "期數模板允許票種不存在。");

    public static readonly Error DrawTemplateTicketTemplateRequired = Error.Validation(
        "Gaming.DrawTemplateTicketTemplateRequired",
        "期數模板允許票種不可為空。");

    public static readonly Error DrawTemplateGameCodeMismatch = Error.Validation(
        "Gaming.DrawTemplateGameCodeMismatch",
        "期數模板遊戲類型不一致。");

    public static readonly Error DrawCodeRequired = Error.Validation(
        code: "Draw.DrawCodeRequired",
        description: "Draw code is required.");

    public static readonly Error DrawSequenceExceeded = Error.Validation(
        "Draw.DrawSequenceExceeded",
        "期數序號已超過上限。");

    public static readonly Error GameNotFound = Error.NotFound(
        "Gaming.GameNotFound",
        "找不到遊戲。");

    public static readonly Error PlayTypeCodeRequired = Error.Validation(
        "Gaming.PlayTypeCodeRequired",
        "玩法代碼不可為空白。");

    public static readonly Error PrizeTierRequired = Error.Validation(
        "Gaming.PrizeTierRequired",
        "獎級代碼不可為空白。");

    public static readonly Error DrawNotOpen = Error.Validation(
        "Gaming.DrawNotOpen",
        "目前不在可販售期間。");

    public static readonly Error DrawAlreadySettled = Error.Validation(
        "Gaming.DrawAlreadySettled",
        "期數已開獎。");

    public static readonly Error DrawNotReadyToExecute = Error.Validation(
        "Gaming.DrawNotReadyToExecute",
        "期數尚未到開獎時間。");

    public static readonly Error DrawTimeInvalid = Error.Validation(
        "Gaming.DrawTimeInvalid",
        "期數時間設定不正確。");

    public static readonly Error DrawNotSettled = Error.Validation(
        "Gaming.DrawNotSettled",
        "Draw 尚未開獎，無法結算。");

    public static readonly Error DrawRedeemValidDaysInvalid = Error.Validation(
        "Gaming.DrawRedeemValidDaysInvalid",
        "期數兌獎有效天數不正確。");

    public static readonly Error MemberNotFound = Error.NotFound(
        "Gaming.MemberNotFound",
        "找不到會員。");

    public static readonly Error PrizeNotFound = Error.NotFound(
        "Gaming.PrizeNotFound",
        "找不到獎品。");

    public static readonly Error PrizeNameRequired = Error.Validation(
        "Gaming.PrizeNameRequired",
        "獎品名稱不可為空白。");

    public static readonly Error PrizeCostInvalid = Error.Validation(
        "Gaming.PrizeCostInvalid",
        "獎品成本不正確。");

    public static readonly Error PrizePayoutInvalid = Error.Validation(
        "Gaming.PrizePayoutInvalid",
        "獎項派彩金額不正確。");

    public static readonly Error PrizeRedeemValidDaysInvalid = Error.Validation(
        "Gaming.PrizeRedeemValidDaysInvalid",
        "獎品兌獎有效天數不正確。");

    public static readonly Error PrizeInactive = Error.Validation(
        "Gaming.PrizeInactive",
        "獎品已停用。");

    public static readonly Error PrizeRuleConflict = Error.Validation(
        "Gaming.PrizeRuleConflict",
        "相同命中顆數已存在啟用中的規則。");

    public static readonly Error PrizeRuleNotFound = Error.NotFound(
        "Gaming.PrizeRuleNotFound",
        "找不到獎項規則。");

    public static readonly Error PrizeRuleRedeemValidDaysInvalid = Error.Validation(
        "Gaming.PrizeRuleRedeemValidDaysInvalid",
        "獎項兌獎有效天數不正確。");

    public static readonly Error PrizeAwardNotFound = Error.NotFound(
        "Gaming.PrizeAwardNotFound",
        "找不到獎品兌獎資格。");

    public static readonly Error PrizeAwardNotOwned = Error.Forbidden(
        "Gaming.PrizeAwardNotOwned",
        "無權兌換此獎品。");

    public static readonly Error PrizeAwardAlreadyRedeemed = Error.Validation(
        "Gaming.PrizeAwardAlreadyRedeemed",
        "此獎品已兌換。");

    public static readonly Error PrizeAwardExpired = Error.Validation(
        "Gaming.PrizeAwardExpired",
        "此獎品已過期。");

    public static readonly Error TicketLineInvalid = Error.Validation(
        "Gaming.TicketLineInvalid",
        "下注明細不正確。");

    public static readonly Error TicketNotFound = Error.NotFound(
        "Gaming.TicketNotFound",
        "找不到票券。");

    public static readonly Error TicketAlreadySubmitted = Error.Validation(
        "Gaming.TicketAlreadySubmitted",
        "票券已提交。");

    public static readonly Error TicketAlreadySubmittedConflict = Error.Conflict(
        "Gaming.TicketAlreadySubmitted",
        "票券已提交。");

    public static readonly Error TicketNumbersAlreadySubmitted = Error.Validation(
        "Gaming.TicketNumbersAlreadySubmitted",
        "票券號碼已提交。");

    public static readonly Error TicketSubmissionClosed = Error.Conflict(
        "Gaming.TicketSubmissionClosed",
        "已封盤，無法提交。");

    public static readonly Error TicketNotSubmitted = Error.Validation(
        "Gaming.TicketNotSubmitted",
        "票券尚未提交。");

    public static readonly Error TicketCancelled = Error.Validation(
        "Gaming.TicketCancelled",
        "票券已作廢。");

    public static readonly Error TicketDrawNotAvailable = Error.Validation(
        "Gaming.TicketDrawNotAvailable",
        "活動尚無可參與的期數。");

    public static readonly Error TicketDrawNotFound = Error.NotFound(
        "Gaming.TicketDrawNotFound",
        "找不到票券期數。");

    public static readonly Error TicketDrawNotSettled = Error.Validation(
        "Gaming.TicketDrawNotSettled",
        "該期尚未結算。");

    public static readonly Error TicketDrawAlreadyRedeemed = Error.Validation(
        "Gaming.TicketDrawAlreadyRedeemed",
        "該期已兌獎。");

    public static readonly Error TicketCannotCancelAfterDraw = Error.Validation(
        "Gaming.TicketCannotCancelAfterDraw",
        "已開獎或結算的票券無法作廢。");

    public static readonly Error TicketTemplateNotFound = Error.NotFound(
        "Gaming.TicketTemplateNotFound",
        "找不到票種模板。");

    public static readonly Error TicketTemplateInactive = Error.Validation(
        "Gaming.TicketTemplateInactive",
        "票種已停用。");

    public static readonly Error TicketTemplateNotAvailable = Error.Validation(
        "Gaming.TicketTemplateNotAvailable",
        "票種不在可用期間。");

    public static readonly Error TicketTemplateCodeRequired = Error.Validation(
        "Gaming.TicketTemplateCodeRequired",
        "票種代碼不可為空白。");

    public static readonly Error TicketTemplateNameRequired = Error.Validation(
        "Gaming.TicketTemplateNameRequired",
        "票種名稱不可為空白。");

    public static readonly Error TicketTemplatePriceInvalid = Error.Validation(
        "Gaming.TicketTemplatePriceInvalid",
        "票種價格不正確。");

    public static readonly Error TicketTemplateMaxLinesInvalid = Error.Validation(
        "Gaming.TicketTemplateMaxLinesInvalid",
        "每張票的注數限制不正確。");

    public static readonly Error TicketTemplateValidityInvalid = Error.Validation(
        "Gaming.TicketTemplateValidityInvalid",
        "票種有效期間設定不正確。");

    public static readonly Error TicketTemplateCodeDuplicated = Error.Validation(
        "Gaming.TicketTemplateCodeDuplicated",
        "票種代碼已存在。");

    public static readonly Error TicketTemplateNotAllowed = Error.Validation(
        "Gaming.TicketTemplateNotAllowed",
        "該期不允許使用此票種。");

    public static readonly Error TicketLinesExceedLimit = Error.Validation(
        "Gaming.TicketLinesExceedLimit",
        "下注注數超過票種限制。");

    public static readonly Error TicketIssueQuantityInvalid = Error.Validation(
        "Gaming.TicketIssueQuantityInvalid",
        "發放數量不正確。");

    public static readonly Error TicketIdempotencyKeyConflict = Error.Conflict(
        "Gaming.TicketIdempotencyKeyConflict",
        "Idempotency-Key 已被不同請求使用。");

    public static readonly Error TicketIdempotencyPayloadInvalid = Error.Problem(
        "Gaming.TicketIdempotencyPayloadInvalid",
        "Idempotency 資料解析失敗。");

    public static readonly Error TicketPlayTypeNotEnabled = Error.Validation(
        "Gaming.TicketPlayTypeNotEnabled",
        "該期未啟用此玩法。");

    public static readonly Error PlayTypeNotAllowed = Error.Validation(
        "Gaming.PlayTypeNotAllowed",
        "玩法不屬於該遊戲。");

    public static readonly Error PlayTypeAlreadyEnabled = Error.Validation(
        "Gaming.PlayTypeAlreadyEnabled",
        "玩法已啟用。");

    public static readonly Error GameNotEntitled = Error.Forbidden(
        "Gaming.GameNotEntitled",
        "租戶未啟用該遊戲。");

    public static readonly Error PlayNotEntitled = Error.Forbidden(
        "Gaming.PlayNotEntitled",
        "租戶未啟用該玩法。");

    public static readonly Error PrizeTierNotAllowed = Error.Validation(
        "Gaming.PrizeTierNotAllowed",
        "獎級不屬於該玩法規則。");

    public static readonly Error PrizePoolIncomplete = Error.Validation(
        "Gaming.PrizePoolIncomplete",
        "期數獎項配置不完整。");

    public static readonly Error PrizePoolSlotMissing = Error.Validation(
        "Gaming.PrizePoolSlotMissing",
        "期數獎項配置缺少獎項槽位。");

    public static readonly Error PrizePoolNotConfigured = Error.Validation(
        "Gaming.PrizePoolNotConfigured",
        "期數獎項槽位尚未設定。");

    public static readonly Error DrawManuallyClosed = Error.Validation(
        "Gaming.DrawManuallyClosed",
        "期數已手動封盤。");

    public static readonly Error DrawAlreadyExecuted = Error.Validation(
        "Gaming.DrawAlreadyExecuted",
        "期數已開獎，無法操作。");

    public static readonly Error DrawReopenWindowInvalid = Error.Validation(
        "Gaming.DrawReopenWindowInvalid",
        "解封時已不在可下注時間窗內。");

    public static readonly Error DrawGroupNotFound = Error.NotFound(
        "Gaming.DrawGroupNotFound",
        "找不到活動。");

    public static readonly Error DrawGroupNameRequired = Error.Validation(
        "Gaming.DrawGroupNameRequired",
        "活動名稱不可為空白。");

    public static readonly Error DrawGroupGrantWindowInvalid = Error.Validation(
        "Gaming.DrawGroupGrantWindowInvalid",
        "活動發放時間窗設定不正確。");

    public static readonly Error DrawGroupInactive = Error.Validation(
        "Gaming.DrawGroupInactive",
        "活動尚未開放或已結束。");

    public static readonly Error DrawGroupNotDraft = Error.Validation(
        "Gaming.DrawGroupNotDraft",
        "活動狀態必須為草稿。");

    public static readonly Error DrawGroupNotActive = Error.Validation(
        "Gaming.DrawGroupNotActive",
        "活動狀態必須為啟用中。");

    public static readonly Error DrawGroupDrawRequired = Error.Validation(
        "Gaming.DrawGroupDrawRequired",
        "活動至少需要綁定一個期數。");

    public static readonly Error DrawGroupDrawDuplicated = Error.Validation(
        "Gaming.DrawGroupDrawDuplicated",
        "活動期數重複。");

    public static readonly Error DrawGroupDrawNotFound = Error.Validation(
        "Gaming.DrawGroupDrawNotFound",
        "活動期數不存在。");

    public static readonly Error DrawGroupTenantMismatch = Error.Forbidden(
        "Gaming.DrawGroupTenantMismatch",
        "租戶資訊不一致。");

    public static readonly Error DrawGroupDrawGameCodeMismatch = Error.Validation(
        "Gaming.DrawGroupDrawGameCodeMismatch",
        "活動遊戲與期數遊戲不一致。");

    public static readonly Error DrawGroupStatusInvalid = Error.Validation(
        "Gaming.DrawGroupStatusInvalid",
        "活動狀態不正確。");

    public static readonly Error DrawGroupAlreadyClaimed = Error.Validation(
        "Gaming.DrawGroupAlreadyClaimed",
        "活動已領取。");

    public static readonly Error DrawGroupDrawPlayTypeNotEnabled = Error.Validation(
        "Gaming.DrawGroupDrawPlayTypeNotEnabled",
        "期數尚未啟用指定玩法。");

    public static readonly Error PrizeAwardOptionNotFound = Error.Validation(
        "Gaming.PrizeAwardOptionNotFound",
        "兌獎選項不存在。");

    public static readonly Error ServerSeedMissing = Error.Validation(
        "Gaming.ServerSeedMissing",
        "缺少開獎用 ServerSeed。");
}
