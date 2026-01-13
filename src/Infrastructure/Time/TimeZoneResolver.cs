using Application.Abstractions.Time;
using TimeZoneConverter;

namespace Infrastructure.Time;

internal sealed class TimeZoneResolver : ITimeZoneResolver
{
    public TimeZoneInfo Resolve(string timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
        {
            throw new ArgumentException("TimeZoneId 不可空白。", nameof(timeZoneId));
        }

        return TZConvert.GetTimeZoneInfo(timeZoneId.Trim());
    }
}
