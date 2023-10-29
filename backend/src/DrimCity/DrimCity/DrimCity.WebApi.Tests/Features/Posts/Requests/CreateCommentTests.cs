using System.Net;
using AutoBogus;
using Common.Tests.Database.Harnesses;
using Common.Tests.Http.Extensions;
using DrimCity.WebApi.Database;
using DrimCity.WebApi.Domain;
using DrimCity.WebApi.Features.Posts.Requests;
using DrimCity.WebApi.Tests.Features.Posts.Contracts;
using DrimCity.WebApi.Tests.Fixtures;

namespace DrimCity.WebApi.Tests.Features.Posts.Requests;

[Collection(PostsTestsCollection.Name)]
public class CreateCommentTests : IAsyncLifetime
{
    private readonly TestFixture _fixture;
    private readonly DatabaseHarness<Program, AppDbContext> _database;

    public CreateCommentTests(TestFixture fixture)
    {
        _fixture = fixture;
        _database = fixture.Database;
    }

    public Task InitializeAsync() => _fixture.Reset(CreateCancellationToken());

    public Task DisposeAsync() => Task.CompletedTask;

    private static async Task<(CommentContract? responseComment, HttpResponseMessage httpResponse)> Act(
        HttpClient httpClient, string postSlug, CreateCommentRequestContract request)
    {
        var (responseComment, httpResponse) = await httpClient.PostTyped<CommentContract>(
            $"/posts/{postSlug}/comments", request, CreateCancellationToken());
        return (responseComment, httpResponse);
    }

    [Fact]
    public async Task Should_create_comment()
    {
        var (httpClient, account) = await _fixture.CreatedAuthedHttpClient();

        var post = CreatePost(account.Id);
        await _database.Save(post);

        var request = AutoFaker.Generate<CreateCommentRequestContract>();

        var (responseComment, httpResponse) = await Act(httpClient, post.Slug, request);

        httpResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        responseComment.Should().NotBeNull();

        httpResponse.Headers.Location.Should().Be($"/posts/{post.Slug}/comments/{responseComment!.Id}");

        responseComment.Id.Should().BeGreaterOrEqualTo(1);
        responseComment.Content.Should().Be(request.Content);
        responseComment.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, 1.Seconds());
        responseComment.AuthorId.Should().Be(account.Id);

        var dbComment = await _database.SingleOrDefault<Comment>(c => c.Id == responseComment.Id,
            CreateCancellationToken());

        dbComment.Should().NotBeNull();
        dbComment!.Should().BeEquivalentTo(responseComment);
        dbComment!.PostId.Should().Be(post.Id);
    }

    [Fact]
    public async Task Should_return_not_found_when_post_does_not_exist()
    {
        const string postSlug = "nonExistingPostSlug";

        var (httpClient, _) = await _fixture.CreatedAuthedHttpClient();

        var request = AutoFaker.Generate<CreateCommentRequestContract>();

        var (responseComment, httpResponse) = await Act(httpClient, postSlug, request);

        httpResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        responseComment.Should().BeNull();
    }

    [Fact]
    public async Task Should_not_authenticate_with_wrong_jwt()
    {
        var request = AutoFaker.Generate<CreateCommentRequestContract>();

        var (httpClient, _) = await _fixture.CreateWronglyAuthedHttpClient();

        var (_, httpResponse) = await Act(httpClient, "slug", request);

        httpResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

public record CreateCommentRequestContract(string Content);

public class AddCommentValidatorTests
{
    private const string AnyValidSlug = "any-valid-slug";
    private readonly CreateComment.RequestValidator _validator = new();

    [Fact]
    public void Should_not_have_errors_when_request_is_valid()
    {
        var request = new CreateComment.Request(1, "Valid content", AnyValidSlug);

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Should_have_error_when_content_empty(string content)
    {
        var request = new CreateComment.Request(1, content, AnyValidSlug);

        var result = _validator.TestValidate(request);

        result
            .ShouldHaveValidationErrorFor(x => x.Content)
            .WithErrorCode("posts:validation:comment_content_must_not_be_empty");
    }

    [Fact]
    public void Should_have_error_when_content_greater_max_length()
    {
        var request = new CreateComment.Request(1, new string('a', 10_001), AnyValidSlug);

        var result = _validator.TestValidate(request);

        result
            .ShouldHaveValidationErrorFor(x => x.Content)
            .WithErrorCode("posts:validation:comment_content_must_be_less_or_equal_max_length");
    }
}
