namespace Application.Abstractions.Authentication;

public interface IRefreshTokenHasher
{
    string Hash(string tokenPlain);

    bool Verify(string tokenPlain, string tokenHash);
}
