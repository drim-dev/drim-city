using Microsoft.EntityFrameworkCore;

namespace DrimCity.WebApi.Database;

public static class DatabaseWebApplicationExtensions
{
    public static async Task<WebApplication> MigrateDatabase(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(1));
        await dbContext.Database.MigrateAsync(cts.Token);
        return app;
    }
}
