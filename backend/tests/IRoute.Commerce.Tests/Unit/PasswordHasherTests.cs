using IRoute.Commerce.Infrastructure.Security;
using Xunit;

namespace IRoute.Commerce.Tests.Unit;

public class PasswordHasherTests
{
    private readonly PasswordHasher _hasher = new();

    [Fact]
    public void Verify_ShouldReturnTrue_WhenPasswordMatches()
    {
        var hash = _hasher.Hash("Secret#1");

        Assert.True(_hasher.Verify("Secret#1", hash));
        Assert.False(_hasher.Verify("Secret#2", hash));
    }

    [Theory]
    [InlineData("plain-text")]
    [InlineData("PBKDF2-SHA256$1000$not-base64$also-not")]
    public void Verify_ShouldReturnFalse_WhenHashIsMalformed(string hash)
    {
        Assert.False(_hasher.Verify("x", hash));
    }
}
