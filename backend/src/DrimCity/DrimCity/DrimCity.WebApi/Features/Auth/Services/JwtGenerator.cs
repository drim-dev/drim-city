using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DrimCity.WebApi.Domain;
using DrimCity.WebApi.Features.Auth.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DrimCity.WebApi.Features.Auth.Services;

public class JwtGenerator
{
    private readonly JwtOptions _options;

    public JwtGenerator(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public string Generate(Account account)
    {
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, account.Id.ToString()),
                new Claim(ClaimTypes.Name, account.Login)
            }),
            IssuedAt = DateTime.UtcNow,
            Expires = DateTime.UtcNow.Add(_options.Expiration),
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_options.Key)), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
