using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using TestSignalR.Models;

namespace TestSignalR.Controllers;

// login, solo nome utente
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var userId = request.UserName?.Trim();
        if (string.IsNullOrEmpty(userId))
            return BadRequest("User name is required.");

        //NameIdentifier è il claim che SignalR usa come UserIdentifier(per Client.UserId)
        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, userId), new Claim(ClaimTypes.Name, userId)],
            CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        return Ok(new { userId });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return NoContent();
    }
}
