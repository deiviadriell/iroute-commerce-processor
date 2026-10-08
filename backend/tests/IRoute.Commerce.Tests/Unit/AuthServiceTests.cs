using IRoute.Commerce.Application.Abstractions;
using IRoute.Commerce.Application.Auth;
using IRoute.Commerce.Domain.Entities;
using NSubstitute;
using Xunit;

namespace IRoute.Commerce.Tests.Unit;

public class AuthServiceTests
{
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly ITokenService _tokenService = Substitute.For<ITokenService>();

    private AuthService CreateService() => new(_users, _passwordHasher, _tokenService);

    [Fact]
    public async Task LoginAsync_ShouldReturnToken_WhenCredentialsAreValid()
    {
        var user = new User { Id = 1, Username = "admin", PasswordHash = "hash", FullName = "Administrador", IsActive = true };
        _users.GetByUsernameAsync("admin").Returns(user);
        _passwordHasher.Verify("secret", "hash").Returns(true);
        _tokenService.Generate(user).Returns(new AuthToken("jwt", DateTime.UtcNow.AddHours(1)));

        var result = await CreateService().LoginAsync(" admin ", "secret");

        Assert.True(result.Success);
        Assert.Equal("jwt", result.AccessToken);
        Assert.Equal("Administrador", result.FullName);
    }

    [Fact]
    public async Task LoginAsync_ShouldFail_WhenPasswordIsWrong()
    {
        _users.GetByUsernameAsync("admin").Returns(new User { Username = "admin", PasswordHash = "hash", IsActive = true });
        _passwordHasher.Verify("wrong", "hash").Returns(false);

        var result = await CreateService().LoginAsync("admin", "wrong");

        Assert.False(result.Success);
        _tokenService.DidNotReceive().Generate(Arg.Any<User>());
    }

    [Fact]
    public async Task LoginAsync_ShouldFail_WhenUserDoesNotExist()
    {
        _users.GetByUsernameAsync("ghost").Returns((User?)null);

        var result = await CreateService().LoginAsync("ghost", "secret");

        Assert.False(result.Success);
    }

    [Fact]
    public async Task LoginAsync_ShouldFail_WhenUserIsInactive()
    {
        _users.GetByUsernameAsync("old").Returns(new User { Username = "old", PasswordHash = "hash", IsActive = false });
        _passwordHasher.Verify("secret", "hash").Returns(true);

        var result = await CreateService().LoginAsync("old", "secret");

        Assert.False(result.Success);
    }

    [Fact]
    public async Task LoginAsync_ShouldFail_WhenCredentialsAreEmpty()
    {
        var result = await CreateService().LoginAsync("", "");

        Assert.False(result.Success);
        await _users.DidNotReceive().GetByUsernameAsync(Arg.Any<string>());
    }
}
