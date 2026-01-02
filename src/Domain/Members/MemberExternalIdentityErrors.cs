using SharedKernel;

namespace Domain.Members;

public static class MemberExternalIdentityErrors
{
    public static readonly Error ProviderRequired = Error.Validation(
        "MemberExternalIdentity.ProviderRequired",
        "外部提供者不可為空白。");

    public static readonly Error ExternalUserIdRequired = Error.Validation(
        "MemberExternalIdentity.ExternalUserIdRequired",
        "外部使用者編號不可為空白。");

    public static readonly Error ExternalUserNameRequired = Error.Validation(
        "MemberExternalIdentity.ExternalUserNameRequired",
        "外部使用者名稱不可為空白。");
}
