using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DrimCity.WebApi.Domain;
using DrimCity.WebApi.Features.Auth.Options;
using DrimCity.WebApi.Features.Auth.Services;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DrimCity.WebApi.Tests.Features.Auth.Services;

public class JwtGeneratorTests
{
    [Fact]
    public void Should_generate_correct_jwt()
    {
        var options = new JwtOptions
        {
            Issuer = "issuer",
            Audience = "audience",
            Key = "veryveryverysecretkey",
            Expiration = TimeSpan.FromMinutes(10),
        };

        var jwtGenerator = new JwtGenerator(new OptionsWrapper<JwtOptions>(options));

        var account = new Account(1, "login", "hash", DateTime.UtcNow);

        var token = jwtGenerator.Generate(account);

        token.Should().NotBeNullOrEmpty();

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, new()
            {
                ValidateIssuer = true,
                ValidIssuer = options.Issuer,
                ValidateAudience = true,
                ValidAudience = options.Audience,
                ValidateLifetime = true,
                ValidTypes = new [] { "JWT" },
                ValidAlgorithms = new [] { "HS256" },
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Key)),
            },
            out var securityToken);

        principal.Should().NotBeNull();
        principal!.Claims.Should().Contain(x => x.Type == ClaimTypes.NameIdentifier && x.Value == account.Id.ToString());
        principal.Claims.Should().Contain(x => x.Type == ClaimTypes.Name && x.Value == account.Login);

        securityToken.ValidFrom.Should().BeCloseTo(DateTime.UtcNow, 10.Seconds());
        securityToken.ValidTo.Should().BeCloseTo(DateTime.UtcNow.Add(options.Expiration), 10.Seconds());
    }
}
