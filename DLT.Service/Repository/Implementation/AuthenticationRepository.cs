using Microsoft.Extensions.Configuration;

namespace DLT.Service.Repository.Implementation;

using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class AuthenticationRepository
{
    private readonly IConfiguration _config;

    public AuthenticationRepository(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateToken(string userSId, string role)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userSId),
            new Claim(ClaimTypes.Role, role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_config["Jwt:ExpireMinutes"])),
            signingCredentials: creds
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}