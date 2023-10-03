using System.Diagnostics;

namespace Common.Tests.Factories;

public static class Factory
{
    public static CancellationToken CreateCancellationToken(int timeoutInSeconds = 10) =>
        new CancellationTokenSource(
            Debugger.IsAttached
                ? TimeSpan.FromMinutes(30)
                : TimeSpan.FromSeconds(timeoutInSeconds))
            .Token;
}
