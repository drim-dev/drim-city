namespace DrimCity.WebApi.Features.Posts.Errors;

public static class PostsValidationErrors
{
    private const string Prefix = "posts:validation:";

    public const string TitleRequired = Prefix + "title_required";
    public const string TitleExceedsMaxLength = Prefix + "title_exceeds_max_length";
    public const string ContentRequired = Prefix + "content_required";
    public const string ContentExceedsMaxLength = Prefix + "content_exceeds_max_length";
    public const string IdRequired = Prefix + "id_required";
    public const string IdNotExists = Prefix + "not_exists";
}
