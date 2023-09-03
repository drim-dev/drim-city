using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using WebApi.Tests.Common.Harnesses.Base;

namespace WebApi.Tests.Common.Harnesses;

public class DatabaseHarness<TProgram, TDbContext> : IHarness<TProgram>
    where TProgram : class
    where TDbContext : DbContext
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
