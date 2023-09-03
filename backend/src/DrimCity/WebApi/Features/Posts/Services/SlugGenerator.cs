using Slugify;

namespace WebApi.Features.Posts.Services;

public static class SlugGenerator
{
    public static string CreateSlug(string text)
    {
        var slugHelper = new SlugHelper();
        var slug = slugHelper.GenerateSlug(text);

        var suffixBytes = new byte[4];
        Random.Shared.NextBytes(suffixBytes);

        return $"{slug}-{Convert.ToHexString(suffixBytes).ToLower()}";
    }
}
