using IRoute.Commerce.Api.Contracts;
using IRoute.Commerce.Application.Auth;
using Microsoft.AspNetCore.Mvc;

namespace IRoute.Commerce.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _service;

    public AuthController(AuthService service)
    {
        _service = service;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await _service.LoginAsync(request.Username, request.Password);

        if (!result.Success)
            return Problem(title: result.Error, statusCode: StatusCodes.Status401Unauthorized);

        return Ok(new
        {
            accessToken = result.AccessToken,
            expiresAt = result.ExpiresAt,
            username = result.Username,
            fullName = result.FullName
        });
    }
}
