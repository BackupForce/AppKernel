using Application.Abstractions.Time;

namespace Application.Time;

internal sealed class UtcRangeCalculator(ITimeZoneResolver timeZoneResolver) : IUtcRangeCalculator
{
    public UtcRange GetUtcRangeForLocalDate(string timeZoneId, DateOnly localDate)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
        {
            throw new ArgumentException("TimeZoneId 不可空白。", nameof(timeZoneId));
        }

        TimeZoneInfo timeZone = timeZoneResolver.Resolve(timeZoneId.Trim());

        DateTime startLocal = localDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
        DateTime endLocal = localDate.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);

        // 中文註解：使用 TimeZoneInfo 轉換，避免 DST 日的 23/25 小時錯誤。
        DateTime startUtc = TimeZoneInfo.ConvertTimeToUtc(startLocal, timeZone);
        DateTime endUtc = TimeZoneInfo.ConvertTimeToUtc(endLocal, timeZone);

        return new UtcRange(new DateTimeOffset(startUtc), new DateTimeOffset(endUtc));
    }
}
