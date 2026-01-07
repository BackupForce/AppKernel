using SharedKernel;

namespace Domain.Members;

public static class MemberErrors
{
    public static readonly Error TenantIdRequired = Error.Validation(
        "Member.TenantIdRequired",
        "TenantId 不可為空白。");

    public static readonly Error DisplayNameRequired = Error.Validation(
        "Member.DisplayNameRequired",
        "顯示名稱不可為空白。");

    public static readonly Error MemberNoRequired = Error.Validation(
        "Member.MemberNoRequired",
        "會員編號不可為空白。");

    public static readonly Error MemberNotFound = Error.NotFound(
        "Member.NotFound",
        "找不到會員。");

    public static readonly Error MemberNoNotUnique = Error.Validation(
        "Member.MemberNoNotUnique",
        "會員編號已存在。");

    public static readonly Error MemberUserNotUnique = Error.Validation(
        "Member.MemberUserNotUnique",
        "User 已經與其他會員綁定。");

    public static readonly Error MemberSuspended = Error.Validation(
        "Member.Suspended",
        "會員已被停權。");

    public static readonly Error InvalidStatusTransition = Error.Validation(
        "Member.InvalidStatusTransition",
        "無法切換到指定的會員狀態。");

    public static readonly Error NegativePointBalance = Error.Validation(
        "Member.NegativePointBalance",
        "點數餘額不可為負數。");

    public static readonly Error AssetCodeRequired = Error.Validation(
        "Member.AssetCodeRequired",
        "資產代碼不可為空白。");

    public static readonly Error NegativeAssetBalance = Error.Validation(
        "Member.NegativeAssetBalance",
        "資產餘額不可為負數。");
}
