namespace DrimCity.WebApi;

public static class SwaggerExtensions
{
    public static WebApplicationBuilder AddSwaggerForDevelopmentEnv(this WebApplicationBuilder builder)
    {
        if (builder.Environment.IsDevelopment())
        {
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.CustomSchemaIds(type => type.ToString().Replace("+", "_"));
            });
        }

        return builder;
    }

    public static WebApplication UseSwaggerForDevelopmentEnv(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        return app;
    }
}
