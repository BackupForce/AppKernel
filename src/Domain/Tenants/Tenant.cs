using SharedKernel;

namespace Domain.Tenants;

public sealed class Tenant : Entity
{
    private const string DefaultTimeZoneId = "UTC";
    private const int TimeZoneIdMaxLength = 128;

    private Tenant()
    {
    }

    private Tenant(Guid id, string code, string name, string timeZoneId)
        : base(id)
    {
        Code = code;
        Name = name;
        TimeZoneId = NormalizeTimeZoneId(timeZoneId);
    }

    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string TimeZoneId { get; private set; } = DefaultTimeZoneId;

    public static Tenant Create(Guid id, string code, string name, string? timeZoneId = null)
    {
        string resolvedTimeZoneId = string.IsNullOrWhiteSpace(timeZoneId)
            ? DefaultTimeZoneId
            : timeZoneId.Trim();

        EnsureTimeZoneId(resolvedTimeZoneId);

        return new Tenant(id, code, name, resolvedTimeZoneId);
    }

    public void SetTimeZone(string timeZoneId)
    {
        EnsureTimeZoneId(timeZoneId);
        TimeZoneId = timeZoneId.Trim();
    }

    private static string NormalizeTimeZoneId(string timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
        {
            return DefaultTimeZoneId;
        }

        string trimmed = timeZoneId.Trim();
        EnsureTimeZoneId(trimmed);
        return trimmed;
    }

    private static void EnsureTimeZoneId(string timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
        {
            throw new ArgumentException("TimeZoneId 不可空白。", nameof(timeZoneId));
        }

        if (timeZoneId.Length > TimeZoneIdMaxLength)
        {
            throw new ArgumentException("TimeZoneId 長度不可超過 128。", nameof(timeZoneId));
        }
    }
}
