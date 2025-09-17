using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DLT.Service.Repository.Implementation;

public class AuthenticationRepository
{
    private readonly IConfiguration _config;

    public AuthenticationRepository(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateToken(string userSId, string role)
    {
        Log.Information("Generating JWT token for UserSID: {UserSID}, Role: {Role}", userSId, role);

        try
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

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            Log.Information("JWT token generated successfully for UserSID: {UserSID}", userSId);

            return tokenString;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error occurred while generating JWT token for UserSID: {UserSID}, Role: {Role}", userSId, role);
            throw;
        }
    }
}