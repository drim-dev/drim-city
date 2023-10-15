using DrimCity.WebApi.Features.Accounts.Options;
using DrimCity.WebApi.Features.Accounts.Services;

namespace DrimCity.WebApi.Features.Accounts.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddAccounts(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<PasswordHashOptions>(builder.Configuration.GetSection("Features:Accounts:PasswordHash"));

        builder.Services.AddSingleton<PasswordHasher>();

        return builder;
    }
}
