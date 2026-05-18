using SharedKernel;

namespace Application.Time;

public static class TimeZoneErrors
{
    public static readonly Error Required = Error.Validation(
        "TimeZone.Required",
        "TimeZoneId 不可空白。");

    public static Error Invalid(string timeZoneId) => Error.Validation(
        "TimeZone.Invalid",
        $"TimeZoneId 無效：{timeZoneId}");
}
