using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Amas.Api.Tests;

internal static class TestJwt
{
    public const string Issuer = "amas-api-tests";
    public const string Audience = "amas-client-tests";
    public const string Secret = "test-secret-with-at-least-32-characters";

    public static string CreateToken()
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Email, "admin@test.local"),
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim("permission", "admin.full_access")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            Issuer,
            Audience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
