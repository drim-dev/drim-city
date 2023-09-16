using Slugify;

namespace DrimCity.WebApi.Features.Posts.Services;

public static class SlugGenerator
{
    public static string CreateSlug(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException(null, nameof(text));
        }

        var slugHelper = new SlugHelperForNonAsciiLanguages();
        var slug = slugHelper.GenerateSlug(text);

        var suffixBytes = new byte[4];
        Random.Shared.NextBytes(suffixBytes);

        return $"{slug}-{Convert.ToHexString(suffixBytes).ToLower()}";
    }
}
