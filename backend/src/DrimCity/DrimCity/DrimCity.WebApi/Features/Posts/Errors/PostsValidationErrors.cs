namespace DrimCity.WebApi.Features.Posts.Errors;

public static class PostsValidationErrors
{
    private const string Prefix = "posts:validation:";

    public const string TitleRequired = Prefix + "title_required";
    public const string TitleMustBeLessOrEqualMaxLength = Prefix + "title_must_be_less_or_equal_max_length";
    public const string ContentRequired = Prefix + "content_required";
    public const string ContentMustBeLessOrEqualMaxLength = Prefix + "content_must_be_less_or_equal_max_length";
}
