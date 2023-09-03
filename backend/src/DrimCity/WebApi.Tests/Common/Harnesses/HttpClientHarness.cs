using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using WebApi.Tests.Common.Harnesses.Base;

namespace WebApi.Tests.Common.Harnesses;

public class HttpClientHarness<TProgram> : IHarness<TProgram>
    where TProgram : class
{
    public void ConfigureWebHostBuilder(IWebHostBuilder builder)
    {
        throw new NotImplementedException();
    }

    public Task Start(WebApplicationFactory<TProgram> factory, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task Stop(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
