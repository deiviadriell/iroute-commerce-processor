namespace IRoute.Commerce.Application.Auth;

public class LoginResult
{
    public bool Success { get; private set; }
    public string? Error { get; private set; }
    public string AccessToken { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public string Username { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;

    private LoginResult()
    {
    }

    public static LoginResult Ok(AuthToken token, string username, string fullName) =>
        new() { Success = true, AccessToken = token.Token, ExpiresAt = token.ExpiresAt, Username = username, FullName = fullName };

    public static LoginResult Fail(string error) => new() { Success = false, Error = error };
}
