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

    public static readonly Error GameCodeRequired = Error.Validation(
        "Gaming.GameCodeRequired",
        "遊戲代碼不可為空白。");

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
        "期數尚未開獎。");

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

    public static readonly Error PrizeAwardOptionNotFound = Error.Validation(
        "Gaming.PrizeAwardOptionNotFound",
        "兌獎選項不存在。");

    public static readonly Error ServerSeedMissing = Error.Validation(
        "Gaming.ServerSeedMissing",
        "缺少開獎用 ServerSeed。");
}
