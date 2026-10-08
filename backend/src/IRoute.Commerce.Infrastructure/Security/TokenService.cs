using System.Security.Claims;
using System.Text;
using IRoute.Commerce.Application.Abstractions;
using IRoute.Commerce.Application.Auth;
using IRoute.Commerce.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace IRoute.Commerce.Infrastructure.Security;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public AuthToken Generate(User user)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_configuration.GetValue("Jwt:ExpirationMinutes", 60));
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

        var token = new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            Expires = expiresAt,
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256),
            Subject = new ClaimsIdentity(
            [
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
                new Claim(JwtRegisteredClaimNames.Name, user.FullName)
            ])
        });

        return new AuthToken(token, expiresAt);
    }
}
