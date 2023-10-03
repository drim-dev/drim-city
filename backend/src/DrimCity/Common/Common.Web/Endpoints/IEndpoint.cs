using System.Reflection;
using Microsoft.AspNetCore.Builder;

namespace Common.Web.Endpoints;

public interface IEndpoint
{
    void MapEndpoint(WebApplication app);
}

public static class WebApplicationExtensions
{
    public static void MapEndpoints(this WebApplication app)
    {
        var endpoints = Assembly.GetCallingAssembly().GetTypes()
            .Where(x => typeof(IEndpoint).IsAssignableFrom(x) && x is { IsInterface: false, IsAbstract: false });

        foreach (var endpoint in endpoints)
        {
            var instance = (IEndpoint?)Activator.CreateInstance(endpoint);
            instance?.MapEndpoint(app);
        }
    }
}
