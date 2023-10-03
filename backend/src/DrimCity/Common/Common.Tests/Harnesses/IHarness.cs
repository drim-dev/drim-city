using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Common.Tests.Harnesses;

public interface IHarness<T> where T : class
{
    void ConfigureWebHostBuilder(IWebHostBuilder builder);

    Task Start(WebApplicationFactory<T> factory, CancellationToken cancellationToken);

    Task Stop(CancellationToken cancellationToken);
}

public static class HarnessExtensions
{
    public static WebApplicationFactory<T> AddHarness<T>(this WebApplicationFactory<T> factory, IHarness<T> harness) where T : class =>
        factory.WithWebHostBuilder(harness.ConfigureWebHostBuilder);
}
