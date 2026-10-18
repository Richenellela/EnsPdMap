using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EnsPdMap.Api.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EnsPdMap.Api.Services;

public class JwtOptions
{
    public string Key { get; set; } = "";
    public string Issuer { get; set; } = "";
    public string Audience { get; set; } = "";
    public int ExpireMinutes { get; set; } = 1440;
}

public class TokenService
{
    readonly JwtOptions _options;

    public TokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public (string Token, DateTime ExpiresAt) Create(Student student)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, student.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, student.Email),
            new Claim("matricule", student.Matricule),
            new Claim(ClaimTypes.Name, student.FullName)
        };
        DateTime expires = DateTime.UtcNow.AddMinutes(_options.ExpireMinutes);
        var token = new JwtSecurityToken(_options.Issuer, _options.Audience, claims, expires: expires, signingCredentials: credentials);
        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}
