using Common.Tests.Harnesses;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Common.Tests.Http.Harnesses;

public class HttpClientHarness<TProgram> : IHarness<TProgram>
    where TProgram : class
{
    private WebApplicationFactory<TProgram>? _factory;
    private bool _started;

    public void ConfigureWebHostBuilder(IWebHostBuilder builder)
    {
    }

    public Task Start(WebApplicationFactory<TProgram> factory, CancellationToken cancellationToken)
    {
        _factory = factory;
        _started = true;

        return Task.CompletedTask;
    }

    public Task Stop(CancellationToken cancellationToken)
    {
        _started = false;

        return Task.CompletedTask;
    }

    public HttpClient CreateClient()
    {
        ThrowIfNotStarted();

        return _factory!.CreateClient();
    }

    private void ThrowIfNotStarted()
    {
        if (!_started)
        {
            throw new InvalidOperationException($"HTTP client harness is not started. Call {nameof(Start)} first.");
        }
    }
}
