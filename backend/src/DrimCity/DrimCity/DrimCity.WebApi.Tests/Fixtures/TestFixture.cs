using Common.Tests.Database.Harnesses;
using Common.Tests.Harnesses;
using Common.Tests.Http.Harnesses;
using DrimCity.WebApi.Database;
using FluentAssertions;
using FluentAssertions.Extensions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace DrimCity.WebApi.Tests.Fixtures;

public class TestFixture : IAsyncLifetime
{
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

    public async Task InitializeAsync()
    {
        await Database.Start(_factory, CreateCancellationToken(60));
        await HttpClient.Start(_factory, CreateCancellationToken());

        _ = _factory.Server;

        AssertionOptions.AssertEquivalencyUsing(options => options
            .Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, 100.Milliseconds()))
            .WhenTypeIs<DateTime>()
            .Using<DateTimeOffset>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, 100.Milliseconds()))
            .WhenTypeIs<DateTimeOffset>());
    }

    public async Task DisposeAsync()
    {
        await HttpClient.Stop(CreateCancellationToken());
        await Database.Stop(CreateCancellationToken());
    }
}
