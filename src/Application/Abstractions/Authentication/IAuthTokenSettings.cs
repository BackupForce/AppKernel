namespace Application.Abstractions.Authentication;

public interface IAuthTokenSettings
{
    int RefreshTokenTtlDays { get; }
}
