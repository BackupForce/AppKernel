namespace SharedKernel;

public static class ResourceNodeKeys
{
    public const string MemberPrefix = "member:";

    public static string Member(Guid memberId)
    {
        return $"{MemberPrefix}{memberId:D}";
    }
}
