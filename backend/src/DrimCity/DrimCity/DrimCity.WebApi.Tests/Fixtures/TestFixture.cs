using System.Net.Http.Headers;
using Common.Tests.Database.Harnesses;
using Common.Tests.Harnesses;
using Common.Tests.Http.Harnesses;
using DrimCity.WebApi.Database;
using DrimCity.WebApi.Domain;
using DrimCity.WebApi.Features.Auth.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace DrimCity.WebApi.Tests.Fixtures;

public class TestFixture : IAsyncLifetime
{
    static TestFixture()
    {
        SetupFluentAssertions();
    }

    private readonly WebApplicationFactory<Program> _factory;

    public TestFixture()
    {
        Database = new();
        HttpClient = new();

        _factory = new WebApplicationFactory<Program>()
            .AddHarness(Database)
            .AddHarness(HttpClient);
    }

    public WebApplicationFactory<Program> Factory => _factory;
    public DatabaseHarness<Program, AppDbContext> Database { get; }
    public HttpClientHarness<Program> HttpClient { get; }

    public async Task Reset(CancellationToken cancellationToken)
    {
        await Database.Clear(cancellationToken);
    }

    public async Task<(HttpClient, Account)> CreatedAuthedHttpClient()
    {
        var account = CreateAccount();
        await Database.Save(account);

        await using var scope = _factory.Services.CreateAsyncScope();
        var jwtGenerator = scope.ServiceProvider.GetRequiredService<JwtGenerator>();
        var jwt = jwtGenerator.Generate(account);

        var client = HttpClient.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

        return (client, account);
    }

    public async Task<(HttpClient, Account)> CreateWronglyAuthedHttpClient()
    {
        var account = CreateAccount();
        await Database.Save(account);

        await using var scope = _factory.Services.CreateAsyncScope();
        var jwtGenerator = scope.ServiceProvider.GetRequiredService<JwtGenerator>();
        var incorrectJwt = jwtGenerator.Generate(account) + "123";

        var client = HttpClient.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", incorrectJwt);

        return (client, account);
    }

    public async Task InitializeAsync()
    {
        await Database.Start(_factory, CreateCancellationToken(60));
        await HttpClient.Start(_factory, CreateCancellationToken());

        _ = _factory.Server;
    }

    public async Task DisposeAsync()
    {
        await HttpClient.Stop(CreateCancellationToken());
        await Database.Stop(CreateCancellationToken());
    }

    // Workaround to fix FluentAssertion concurrency issue
    // https://github.com/fluentassertions/fluentassertions/issues/1932#issuecomment-1137366562
    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void SetupFluentAssertions()
    {
        AssertionOptions.AssertEquivalencyUsing(options => options
            .Using<DateTimeOffset>(ctx => ctx.Subject.Should().BeSameDateAs(ctx.Expectation))
            .WhenTypeIs<DateTimeOffset>()
            .Using<DateTime>(ctx => ctx.Subject.Should().BeSameDateAs(ctx.Expectation))
            .WhenTypeIs<DateTime>()
        );
    }
}
