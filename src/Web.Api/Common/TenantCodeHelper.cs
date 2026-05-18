namespace Web.Api.Common;

public static class TenantCodeHelper
{
    public static string Normalize(string tenantCode)
    {
        return tenantCode.Trim().ToUpperInvariant();
    }

    public static bool IsValid(string tenantCode)
    {
        if (tenantCode.Length != 3)
        {
            return false;
        }

        for (int index = 0; index < tenantCode.Length; index++)
        {
            if (!char.IsLetterOrDigit(tenantCode[index]))
            {
                return false;
            }
        }

        return true;
    }
}
