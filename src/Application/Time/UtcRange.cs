namespace Application.Time;

public readonly record struct UtcRange(DateTimeOffset StartUtc, DateTimeOffset EndUtc);
