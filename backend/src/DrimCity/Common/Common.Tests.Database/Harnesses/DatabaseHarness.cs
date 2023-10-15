using System.Collections;
using System.Data;
using System.Linq.Expressions;
using Common.Tests.Harnesses;
using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Respawn;
using Testcontainers.PostgreSql;

namespace Common.Tests.Database.Harnesses;

public class DatabaseHarness<TProgram, TDbContext> : IHarness<TProgram>
    where TProgram : class
    where TDbContext : DbContext
{
    private PostgreSqlContainer? _postgres;
    private WebApplicationFactory<TProgram>? _factory;
    private bool _started;

    public void ConfigureWebHostBuilder(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "ConnectionStrings:AppDbContext", _postgres!.GetConnectionString() },
            });
        });
    }

    public async Task Start(WebApplicationFactory<TProgram> factory, CancellationToken cancellationToken)
    {
        _factory = factory;

        _postgres = new PostgreSqlBuilder()
            .WithImage("postgres:15.4-alpine3.18")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
            .Build();

        await _postgres.StartAsync(cancellationToken);

        _started = true;
    }

    public async Task Stop(CancellationToken cancellationToken)
    {
        if (_postgres is not null)
        {
            await _postgres.StopAsync(cancellationToken);
            await _postgres.DisposeAsync();
        }

        _started = false;
    }

    public async Task Migrate(CancellationToken cancellationToken)
    {
        ThrowIfNotStarted();

        await using var scope = _factory!.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<TDbContext>();
        await db.Database.MigrateAsync(cancellationToken);
    }

    public async Task<T?> SingleOrDefault<T>(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken)
        where T : class
    {
        await using var scope = _factory!.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<TDbContext>();

        return await db.Set<T>().SingleOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task Execute(Func<TDbContext, Task> action)
    {
        ThrowIfNotStarted();

        await using var scope = _factory!.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<TDbContext>();
        await action(db);
    }

    public async Task<T> Execute<T>(Func<TDbContext, Task<T>> action)
    {
        if (!_started)
        {
            throw new InvalidOperationException($"Database harness is not started. Call {nameof(Start)} first.");
        }

        await using var scope = _factory!.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<TDbContext>();
        return await action(db);
    }

    public async Task Save(params object[] entities)
    {
        ThrowIfNotStarted();

        await using var scope = _factory!.Services.CreateAsyncScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();

        var collections = entities.OfType<IEnumerable>();

        foreach (var collection in collections)
        {
            dbContext.AddRange(collection);
        }

        var singleEntities = entities.Where(e => e is not IEnumerable);

        dbContext.AddRange(singleEntities);
        await dbContext.SaveChangesAsync();
    }

    public async Task Clear(CancellationToken cancellationToken)
    {
        await using var scope = _factory!.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<TDbContext>();

        await using var connection = db.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        var respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            SchemasToInclude = new [] { "public" },
            DbAdapter = DbAdapter.Postgres,
        });

        await respawner.ResetAsync(connection);
    }

    private void ThrowIfNotStarted()
    {
        if (!_started)
        {
            throw new InvalidOperationException($"Database harness is not started. Call {nameof(Start)} first.");
        }
    }
}
