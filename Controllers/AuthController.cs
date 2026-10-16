using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using TestSignalR.Models;

namespace TestSignalR.Controllers;

// emette un JWT per qualsiasi nome utente, solo per demo: in produzione serve un vero identity provider
[ApiController]
[Route("api/auth")]
public class AuthController(IConfiguration configuration) : ControllerBase
{
    [HttpPost("token")]
    public IActionResult Token(TokenRequest request)
    {
        var userId = request.UserName.Trim();
        if (string.IsNullOrEmpty(userId))
            return BadRequest("User name is required.");

        var jwt = configuration.GetSection("Jwt");
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: [new Claim(JwtRegisteredClaimNames.Sub, userId), new Claim(ClaimTypes.Name, userId)],
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256));

        return Ok(new { accessToken = new JwtSecurityTokenHandler().WriteToken(token), userId });
    }
}
