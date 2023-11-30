namespace DrimCity.WebApi.Features.Posts.Extensions;

public static class StringExtensions
{
    public static string Ellipsize(this string text, int maxLength)
    {
        if (text.Length <= maxLength)
        {
            return text;
        }

        return text[..maxLength] + "...";
    }
}
