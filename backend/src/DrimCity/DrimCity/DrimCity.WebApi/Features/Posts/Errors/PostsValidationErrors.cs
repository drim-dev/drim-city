namespace DrimCity.WebApi.Features.Posts.Errors;

public static class PostsValidationErrors
{
    private const string Prefix = "posts:validation:";

    public const string TitleMustNotBeEmpty = Prefix + "title_must_not_be_empty";
    public const string TitleMustBeLessOrEqualMaxLength = Prefix + "title_must_be_less_or_equal_max_length";
    public const string ContentMustNotBeEmpty = Prefix + "content_must_not_be_empty";
    public const string ContentMustBeLessOrEqualMaxLength = Prefix + "content_must_be_less_or_equal_max_length";
    public const string PageSizeMustBePositive = Prefix + "page_size_must_be_positive";

    public const string CommentContentMustNotBeEmpty = Prefix + "comment_content_must_not_be_empty";

    public const string CommentContentMustBeLessOrEqualMaxLength = Prefix +
                                                                   "comment_content_must_be_less_or_equal_max_length";
}
