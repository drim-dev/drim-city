using System.Net;
using AutoBogus;
using Common.Tests.Database.Harnesses;
using DrimCity.WebApi.Database;
using DrimCity.WebApi.Domain;
using DrimCity.WebApi.Features.Posts.Requests;
using DrimCity.WebApi.Tests.Extensions;
using DrimCity.WebApi.Tests.Features.Posts.Contracts;
using DrimCity.WebApi.Tests.Fixtures;
using RestSharp;

namespace DrimCity.WebApi.Tests.Features.Posts.Requests;

[Collection(PostsTestsCollection.Name)]
public class CreateCommentTests : IAsyncLifetime
{
    private readonly DatabaseHarness<Program, AppDbContext> _database;
    private readonly TestFixture _fixture;

    public CreateCommentTests(TestFixture fixture)
    {
        _fixture = fixture;
        _database = fixture.Database;
    }

    public Task InitializeAsync() => _fixture.Reset(CreateCancellationToken());

    public Task DisposeAsync() => Task.CompletedTask;

    private static async Task<RestResponse<CommentContract>> Act(
        HttpClient httpClient, string postSlug, CreateCommentRequestContract request) =>
        await new RestClient(httpClient).ExecutePostAsync<CommentContract>(
            $"/posts/{postSlug}/comments", request, CreateCancellationToken());

    [Fact]
    public async Task Should_create_comment()
    {
        var (httpClient, account) = await _fixture.CreatedAuthedHttpClient();

        var post = CreatePost(account.Id);
        await _database.Save(post);

        var request = AutoFaker.Generate<CreateCommentRequestContract>();

        var restResponse = await Act(httpClient, post.Slug, request);

        restResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var responseComment = restResponse.Data;
        responseComment.ShouldNotBeNull();

        restResponse.Headers.Location().Should().Be($"/posts/{post.Slug}/comments/{responseComment.Id}");

        responseComment.Id.Should().BeGreaterOrEqualTo(1);
        responseComment.Content.Should().Be(request.Content);
        responseComment.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, 1.Seconds());
        responseComment.AuthorId.Should().Be(account.Id);

        var dbComment = await _database.SingleOrDefault<Comment>(c => c.Id == responseComment.Id,
            CreateCancellationToken());

        dbComment.ShouldNotBeNull();
        dbComment.Should().BeEquivalentTo(responseComment);
        dbComment.PostId.Should().Be(post.Id);
    }

    [Fact]
    public async Task Should_return_not_found_when_post_does_not_exist()
    {
        const string postSlug = "nonExistingPostSlug";

        var (httpClient, _) = await _fixture.CreatedAuthedHttpClient();

        var request = AutoFaker.Generate<CreateCommentRequestContract>();

        var restResponse = await Act(httpClient, postSlug, request);

        restResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var responseComment = restResponse.Data;
        responseComment.Should().BeNull();
    }

    [Fact]
    public async Task Should_not_authenticate_with_wrong_jwt()
    {
        var request = AutoFaker.Generate<CreateCommentRequestContract>();

        var (httpClient, _) = await _fixture.CreateWronglyAuthedHttpClient();

        var restResponse = await Act(httpClient, "slug", request);

        restResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}

public record CreateCommentRequestContract(string Content);

public class CreateCommentValidatorTests
{
    private readonly CreateComment.RequestValidator _validator = new();

    [Fact]
    public void Should_not_have_errors_when_request_is_valid()
    {
        var request = CreateRequest();

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Should_have_error_when_content_empty(string content)
    {
        var request = CreateRequest() with { Content = content };

        var result = _validator.TestValidate(request);

        result
            .ShouldHaveValidationErrorFor(x => x.Content)
            .WithErrorCode("posts:validation:comment_content_must_not_be_empty");
    }

    [Fact]
    public void Should_have_error_when_content_greater_max_length()
    {
        var request = CreateRequest() with { Content = new string('a', 10_001) };

        var result = _validator.TestValidate(request);

        result
            .ShouldHaveValidationErrorFor(x => x.Content)
            .WithErrorCode("posts:validation:comment_content_must_be_less_or_equal_max_length");
    }

    private static CreateComment.Request CreateRequest() =>
        new AutoFaker<CreateComment.Request>()
            .RuleFor(request => request.Content, faker => faker.Random.Words())
            .Generate();
}
