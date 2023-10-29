using System.Text;
using DrimCity.WebApi.Features.Auth.Options;
using DrimCity.WebApi.Features.Auth.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace DrimCity.WebApi.Features.Auth.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddAuth(this WebApplicationBuilder builder)
    {
        const string jwtOptionsSection = "Features:Auth:Jwt";
        const string passwordHashOptionsSection = "Features:Auth:PasswordHash";

        builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(jwtOptionsSection));

        builder.Services.Configure<PasswordHashOptions>(builder.Configuration.GetSection(passwordHashOptionsSection));

        builder.Services.AddSingleton<JwtGenerator>();
        builder.Services.AddSingleton<PasswordHasher>();

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            var jwtOptions = builder.Configuration.GetSection(jwtOptionsSection).Get<JwtOptions>();

            options.TokenValidationParameters = new TokenValidationParameters {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtOptions!.Issuer,
                ValidAudience = jwtOptions.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
            };
        });

        builder.Services.AddAuthorization();

        return builder;
    }
}
