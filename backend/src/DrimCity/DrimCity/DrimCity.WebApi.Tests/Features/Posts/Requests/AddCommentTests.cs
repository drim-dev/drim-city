using System.Net;
using AutoBogus;
using Common.Tests.Database.Harnesses;
using Common.Tests.Http.Extensions;
using Common.Tests.Http.Harnesses;
using DrimCity.WebApi.Database;
using DrimCity.WebApi.Domain;
using DrimCity.WebApi.Features.Posts.Requests;
using DrimCity.WebApi.Tests.Features.Posts.Contracts;
using DrimCity.WebApi.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;

namespace DrimCity.WebApi.Tests.Features.Posts.Requests;

[Collection(PostsTestsCollection.Name)]
public class AddCommentTests : IAsyncLifetime
{
    private readonly DatabaseHarness<Program, AppDbContext> _database;
    private readonly HttpClientHarness<Program> _httpClient;

    public AddCommentTests(TestFixture fixture)
    {
        _database = fixture.Database;
        _httpClient = fixture.HttpClient;
    }

    [Fact]
    private async Task Should_create_comment()
    {
        var post = await CreatePost();
        var request = AutoFaker.Generate<CreateCommentRequestContract>();

        var (responseComment, httpResponse) = await Act(post.Slug, request);

        httpResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        responseComment.Should().NotBeNull();

        httpResponse.Headers.Location.Should().Be($"/posts/{post.Slug}/comments/{responseComment!.Id}");

        responseComment.Id.Should().BeGreaterOrEqualTo(1);
        responseComment.Content.Should().Be(request.Content);
        responseComment.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, 1.Seconds());
        responseComment.AuthorId.Should().Be(1);

        var dbComment = await _database.Execute(async dbContext => await dbContext.Comments
            .SingleOrDefaultAsync(c => c.Id == responseComment.Id, CreateCancellationToken()));

        dbComment.Should().NotBeNull();
        dbComment.Should().BeEquivalentTo(responseComment);
        dbComment!.PostId.Should().Be(post.Id);
    }

    [Fact]
    private async Task Should_return_not_found_when_post_does_not_exist()
    {
        const string postSlug = "nonExistingPostSlug";
        var request = AutoFaker.Generate<CreateCommentRequestContract>();

        var (responseComment, httpResponse) = await Act(postSlug, request);

        httpResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        responseComment.Should().BeNull();
    }

    private async Task<(CommentContract? responseComment, HttpResponseMessage httpResponse)> Act(
        string postSlug,
        CreateCommentRequestContract request)
    {
        var httpClient = _httpClient.CreateClient();
        var (responseComment, httpResponse) = await httpClient
            .PostTyped<CommentContract>($"/posts/{postSlug}/comments", request, CreateCancellationToken());
        return (responseComment, httpResponse);
    }

    private async Task<Post> CreatePost()
    {
        return await _database.Execute(async dbContext =>
        {
            var post = new Post(0, "anyTitle", "anyContent", DateTime.UtcNow, 0, "postSlug");
            await dbContext.Posts.AddAsync(post, CreateCancellationToken());
            await dbContext.SaveChangesAsync();
            return post;
        });
    }

    public async Task InitializeAsync()
    {
        await _database.Clear(CreateCancellationToken());
    }

    public Task DisposeAsync() => Task.CompletedTask;
}

public record CreateCommentRequestContract(string Content);

public class AddCommentValidatorTests
{
    private readonly AddComment.BodyValidator _validator = new();

    [Fact]
    private void Should_not_have_errors_when_request_is_valid()
    {
        var body = new AddComment.Body("Valid content");

        var result = _validator.TestValidate(body);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Should_have_error_when_content_empty(string content)
    {
        var body = new AddComment.Body(content);

        var result = _validator.TestValidate(body);

        result
            .ShouldHaveValidationErrorFor(x => x.Content)
            .WithErrorCode("posts:validation:comment_content_required");
    }

    [Fact]
    public void Should_have_error_when_content_exceeds_max_length()
    {
        var body = new AddComment.Body(new string('a', 10_001));

        var result = _validator.TestValidate(body);

        result
            .ShouldHaveValidationErrorFor(x => x.Content)
            .WithErrorCode("posts:validation:comment_content_exceeds_max_length");
    }
}