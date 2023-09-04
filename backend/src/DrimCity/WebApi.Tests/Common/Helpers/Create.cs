using System.Diagnostics;

namespace WebApi.Tests.Common.Helpers;

public static class Create
{
    public static CancellationToken CancellationToken(int timeoutInSeconds = 10) =>
        new CancellationTokenSource(
            Debugger.IsAttached
                ? TimeSpan.FromMinutes(10)
                : TimeSpan.FromSeconds(timeoutInSeconds))
            .Token;
}
