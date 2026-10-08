namespace IRoute.Commerce.Application.Auth;

public class AuthToken
{
    public string Token { get; }
    public DateTime ExpiresAt { get; }

    public AuthToken(string token, DateTime expiresAt)
    {
        Token = token;
        ExpiresAt = expiresAt;
    }
}
