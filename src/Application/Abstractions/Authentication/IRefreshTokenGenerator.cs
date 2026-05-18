namespace Application.Abstractions.Authentication;

public interface IRefreshTokenGenerator
{
    string GenerateToken();
}
