using SharedKernel;

namespace Domain.Gaming;

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

    public static readonly Error MemberNotFound = Error.NotFound(
        "Gaming.MemberNotFound",
        "找不到會員。");

    public static readonly Error PrizeNotFound = Error.NotFound(
        "Gaming.PrizeNotFound",
        "找不到獎品。");

    public static readonly Error PrizeNameRequired = Error.Validation(
        "Gaming.PrizeNameRequired",
        "獎品名稱不可為空白。");

    public static readonly Error PrizeInactive = Error.Validation(
        "Gaming.PrizeInactive",
        "獎品已停用。");

    public static readonly Error PrizeRuleConflict = Error.Validation(
        "Gaming.PrizeRuleConflict",
        "相同命中顆數已存在啟用中的規則。");

    public static readonly Error PrizeRuleNotFound = Error.NotFound(
        "Gaming.PrizeRuleNotFound",
        "找不到獎項規則。");

    public static readonly Error PrizeAwardNotFound = Error.NotFound(
        "Gaming.PrizeAwardNotFound",
        "找不到獎品兌獎資格。");

    public static readonly Error PrizeAwardNotOwned = Error.Forbidden(
        "Gaming.PrizeAwardNotOwned",
        "無權兌換此獎品。");

    public static readonly Error PrizeAwardAlreadyRedeemed = Error.Validation(
        "Gaming.PrizeAwardAlreadyRedeemed",
        "此獎品已兌換。");

    public static readonly Error TicketLineInvalid = Error.Validation(
        "Gaming.TicketLineInvalid",
        "下注明細不正確。");

    public static readonly Error ServerSeedMissing = Error.Validation(
        "Gaming.ServerSeedMissing",
        "缺少開獎用 ServerSeed。");
}
