using Common.Web.Endpoints;

namespace DrimCity.Registry.HttpApi.Features.Manifests.Requests;

public static class UploadManifest
{
    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(WebApplication app)
        {
            app.MapPut("/upload", async (Stream body) =>
            {
                await using var stream = File.OpenWrite(@"d:\result.txt");
                await body.CopyToAsync(stream);
            });
        }
    }
}
