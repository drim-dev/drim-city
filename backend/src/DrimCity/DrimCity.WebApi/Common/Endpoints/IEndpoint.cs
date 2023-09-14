using System.Reflection;

namespace DrimCity.WebApi.Common.Endpoints;

public interface IEndpoint
{
    void MapEndpoint(WebApplication app);
}

public static class WebApplicationExtensions
{
    public static void MapEndpoints(this WebApplication app)
    {
        var endpoints = Assembly.GetExecutingAssembly().GetTypes()
            .Where(x => typeof(IEndpoint).IsAssignableFrom(x) && x is { IsInterface: false, IsAbstract: false });

        foreach (var endpoint in endpoints)
        {
            var instance = (IEndpoint?)Activator.CreateInstance(endpoint);
            instance?.MapEndpoint(app);
        }
    }
}
