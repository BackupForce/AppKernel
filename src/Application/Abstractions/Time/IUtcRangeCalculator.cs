using Application.Time;

namespace Application.Abstractions.Time;

public interface IUtcRangeCalculator
{
    UtcRange GetUtcRangeForLocalDate(string timeZoneId, DateOnly localDate);
}
